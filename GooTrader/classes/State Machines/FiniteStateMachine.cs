using System;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Extensions;
using Appccelerate.SourceTemplates.Log4Net;

namespace IBSampleApp
{
    // Finite state machine class
    public abstract class FiniteStateMachine
    {      
        // Internal state machine. See Appccelerate docs for other types
        private ActiveStateMachine<string, string> _fsm = new ActiveStateMachine<string, string>();
        
        // These two states are for initialization and cleanup
        private static string _initializeStateName = "Initialize";
        private static string _terminateStateName = "Terminate";

        // used to make the initial state transition from entry state
        private static string _initalizedEventName = "Initialized";

        // object which is using the FSM. Can store custom data for the state machine
        private Object _fsmData;

        // Only events of this type may be "fired". Used for sanity checking so we can't b
        private Type _requiredEventType;

        // Required state methods. Normally they do nothing, but they can be overridden.
        #region State Book-keeping Methods
        protected virtual void Initialize() { }

        protected virtual void Terminate() { }
        #endregion

        // These are "placeholder" functions that user MUST override and return the appropriate type for a specific state machine!
        #region "PlaceHolder" Methods

        // Allows user to manually assign the Appccelerate ".Execute", ".ExecuteOnEntry", and ".ExecuteOnExit" transition actions.
        // Must pass "true" in the constructor for this to get called!
        protected virtual void AssignStateActions()
        {
            // TODO: We'll implement this later if we need it. If we get the exception, then we know we need it!
            throw new NotImplementedException();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generate transition event
        /// </summary>
        /// <param name="newEvent">Event to generate</param>
        /// <param name="eventArg">argument to pass (default=null)</param>
        public void FireEvent(Enum newEvent, object eventArg=null)
        {
            Type eventType = newEvent.GetType();

            // Make sure event passed was of the same enum type. We're not checking that we passed the right event for the situation,
            // just making sure we didn't send in the wrong enum by accident.
            if(eventType == _requiredEventType)
            {
                _fsm.Fire(newEvent.ToString(), eventArg);
            } else
            {
                throw new NotSupportedException("Event is incompatible with this state machine.");
            }
        }

        /// <summary>
        /// Start execution of a FSM
        /// </summary>
        /// <param name="eventArg">argument to pass (default=null)</param>
        public void Start(object eventArg=null)
        {
            // Start the FSM. This will call the "Initialize" state, which is the first state in every FSM.
            _fsm.Start();
            // Transition from Initialize to first state
            _fsm.Fire(_initalizedEventName, eventArg);
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="states">Enum of states.</param>
        /// <param name="events">Enum of events.</param>
        /// <param name="transitions">Array of StateTransitions.</param>
        /// <param name="methodSignature">[OPTIONAL]Action signature (Action<type1, type2,...>) Default is method with no parameters</type1></param>
        /// <param name="fsmObj">[OPTIONAL]The object instance which uses this FSM. Can be null</param>
        /// <param name="startFSM">[OPTIONAL]false=>FSM must be manually stated with Start(). Usually only needed if you don't have required parameters right away.</param>
        /// <param name="assignActionsManually">[OPTIONAL]false</param>
        public FiniteStateMachine(Type states, Type events, StateTransition[] transitions, 
                                    Type methodSignature=null, object fsmObj=null, bool startFSM=false, bool assignActionsManually=false)
        {
            // Save instance of object that is using the FSM so it can be accessed later by the FSM.
            _fsmData = fsmObj;

            // Set the type of events that this state machine can respond to.
            _requiredEventType = events;

            var stateNames = Enum.GetNames(states);

            // Build all allowed transitions.
            foreach (StateTransition t in transitions)
            {
                _fsm.In(t.InState.ToString()).On(t.OnEvent.ToString()).Goto(t.GotoState.ToString());
            }

            // TODO: Add error checking to make sure "Initialize" and "Terminate" states are in state list
            // TODO: Add a call to a manual setup which allows any state method signatures

            if (assignActionsManually == false)
            {
                // Create an action for each state method with a GooContract parameter
                foreach (string stateName in stateNames)
                {
                    // All states except initialization and termination states have signatures. Those get done below
                    if (!stateName.Equals(_initializeStateName) && !stateName.Equals(_terminateStateName))
                    {
                        // User can provide for more complex method signatures. Default is 'Action' (i.e: method with no parameters)
                        Type t = methodSignature ?? typeof(Action);

                        // var methodAction = (Action<GooContract>)Delegate.CreateDelegate(typeof(Action<GooContract>), this, stateName);
                        // dynamic allows us to cast without run-time static checking. Line above was previous implementation.
                        dynamic methodAction = Delegate.CreateDelegate(t, this, stateName);
                        
                        _fsm.In(stateName).ExecuteOnEntry(methodAction);
                    }
                }
            }
            else
            {
                // Up to the user to define the Appccelerate ".Execute" type stuff. See Appccelerate: http://www.appccelerate.com/statemachineactions.html
                AssignStateActions();
            }

            // Always have starting and termination states
            _fsm.In(_initializeStateName).ExecuteOnEntry(this.Initialize);
            _fsm.In(_terminateStateName).ExecuteOnEntry(this.Terminate);
            
            // Starting state is always the "Initialize" state.
            _fsm.Initialize(_initializeStateName);

            // Default to begin operation of the FSM.
            if(startFSM == true)
            {
                Start();
            }

            // Add some handlers so we can track states/transitions while debugging
            _fsm.TransitionCompleted += _fsm_TransitionCompleted;
            _fsm.TransitionDeclined += _fsm_TransitionDeclined;
            _fsm.TransitionExceptionThrown += _fsm_TransitionExceptionThrown;

            // Implement a logging extension. TState & TEvent are both strings to make this work
            //_fsm.AddExtension(new StateMachineLogExtension<string, string>());
        }

        #region Debug Stuff
        private void _fsm_TransitionExceptionThrown(object sender, Appccelerate.StateMachine.Machine.Events.TransitionExceptionEventArgs<string, string> e)
        {
            // TODO: Not sure how we get here, but it could be important so leave exception to flag it for now so we can investigate later.
            throw new NotImplementedException();
        }
        private void _fsm_TransitionDeclined(object sender, Appccelerate.StateMachine.Machine.Events.TransitionEventArgs<string, string> e)
        {
            // TODO: Not sure how we get here, but it could be important so leave exception to flag it for now so we can investigate later.
            throw new NotImplementedException();
        }

        private Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<string, string> _debugLastTransition;
        private void _fsm_TransitionCompleted(object sender, Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<string, string> e)
        {
            // make a copy of the latest transition so we can debug.
            // TODO: Transition logger
            _debugLastTransition = e;
        }
        #endregion

        // contains some reflection code we may want to use later. Ignore it for now
        #region Old Example Code
        //private void Initialize2()
        //{
            //Type classtype = this.GetType();
            //MethodInfo[] classmethods = Type.GetType(this.ToString()).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            //MemberInfo[] classmembers = classtype.GetMembers();
            //var qualifiedStateEnumName = String.Format("{0}.{1}+{2}", classtype.Namespace, classtype.Name, FiniteStateMachine.StatesEnumName);
            //var entryAssembly = Assembly.GetEntryAssembly();
            //string[] stateNames = null;

            // LINQ to pull the "States" enum from the class members so we can get a list of the FSM states
            //var stateEnum = from member in classmembers where member.Name == FiniteStateMachine.StatesEnumName select member;
            //if (stateEnum.Any())
            //{
            //    var stateEnumType = entryAssembly.GetType(qualifiedStateEnumName);
            //   stateNames = Enum.GetNames(stateEnumType);
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}

            // TODO: Assign execution for all defined states
            //foreach (string stateName in stateNames)
            //{
            //    var stateMethod = from method in classmethods where method.Name == stateName select method;

            //    if (!stateMethod.Any())
            //    {
            //        throw new NotImplementedException();
            //    }
            //    Action methodAction = (Action)Delegate.CreateDelegate(typeof(Action), this, stateName);
            //}
        //}
        #endregion
    }

    /// <summary>
    /// Define transition to next state based on current state and received event
    /// </summary>
    public class StateTransition
    {
        public System.Enum InState;
        public System.Enum OnEvent;
        public System.Enum GotoState;
        public StateTransition(System.Enum instate, System.Enum onevent, System.Enum gotostate)
        {
            InState = instate;
            OnEvent = onevent;
            GotoState = gotostate;
        }
    }
}

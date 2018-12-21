using System;
using Appccelerate.StateMachine;

namespace IBSampleApp
{
    // Finite state machine class
    public class FiniteStateMachine
    {
        // Internal state machine. See Appccelerate docs for other types
        private ActiveStateMachine<string, string> _fsm = new ActiveStateMachine<string, string>();
        
        // These two states are for initialization and cleanup
        private static string _initializeStateName = "Initialize";
        private static string _terminateStateName = "Terminate";

        // used to make the initial state transition from entry state
        private static string _initalizedEventName = "Initialized";

        // object which is using the FSM. Can store custom data
        private Object _fsmObject;

        // Required state methods. Normally they do nothing, but they can be overridden.
        #region State Methods
        protected virtual void Initialize() { }

        protected virtual void Terminate() { }
        #endregion

        // These are "placeholder" functions that user MUST override and return the appropriate type for a specific state machine!
        #region "PlaceHolder" Methods
        // Return typeof(YourEventsEnum)
        public virtual Type GetEvents()
        {
            throw new NotImplementedException();
        }

        // Return typeof(YourStatesEnum)
        protected virtual Type GetStates()
        {
            throw new NotImplementedException();
        }

        // NOTE: A state object is a common object instance that can be accessed by the fsm state methods.
        // These are not necessary, but can provide flexibility if the states need to access something during operation.
        // Return typeof(<YourStateObjectClass>).
        protected virtual Type GetStateObjectType()
        {
            // This is the object type of whatever is using the FSM.
            throw new NotImplementedException();
        }

        // Return instance of the FSM host object for use by the FSM states.
        protected Object GetStateObjectInstance()
        {
            return _fsmObject;
        }

        // Return array of your StateTransitions
        protected virtual StateTransition[] GetTransitions()
        {
            throw new NotImplementedException();
        }

        // Return an action method signature used by all state methods.
        protected virtual Type GetStateMethodSignature()
        {
            // Default Action has no parameters. Override in your FSM to allow parameters to be passed to states via FireEvent
            return typeof(Action);
        }

        // Allows user to manually assign the Appccelerate ".Execute", ".ExecuteOnEntry", and ".ExecuteOnExit" transition actions.
        // Must pass "true" in the constructor for this to get called!
        protected virtual void AssignStateActions()
        {
            // TODO: GOOT-15 We'll implement this later if we need it. If we get the exception, then we know we need it!
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
            Type enumType = this.GetEvents();
            Type eventType = newEvent.GetType();

            // Make sure event passed was of the same enum type. We're not checking that we passed the right event for the situation,
            // just making sure we didn't send in the wrong type by accident.
            if(eventType == enumType)
            {
                _fsm.Fire(newEvent.ToString(), eventArg);
            } else
            {
                throw new NotSupportedException("Invalid Enum Type");
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
        /// <param name="fsmObj">The object instance which uses this FSM. Can be accessed by GetStateObjectInstance(). Can be null</param>
        /// <param name="startFSM">false=>FSM must be manually stated with Start(). Usually only needed if you don't have required parameters right away.</param>
        /// <param name="assignActionsManually">false</param>
        public FiniteStateMachine(object fsmObj=null, bool startFSM=false, bool assignActionsManually=false)
        {
            // Save instance of object that is using the FSM so it can be accessed later by the FSM.
            _fsmObject = fsmObj;
            
            Type states = this.GetStates();
            Type events = this.GetEvents();
            StateTransition[] transitions = this.GetTransitions();
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
                        // TODO: Future use could add "Entry" and "Exit" to method signatures to allow more flexible execution

                        //var methodAction = (Action<GooContract>)Delegate.CreateDelegate(typeof(Action<GooContract>), this, stateName);
                        // dynamic allows us to cast without run-time static checking. Line above was previous implementation.
                        // This allows user to provide an Action method signature.
                        Type t = this.GetStateMethodSignature();
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
        private void Initialize2()
        {
            #region Old Example Code
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
            #endregion
        }
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

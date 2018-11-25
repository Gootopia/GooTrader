using System;
using System.Linq;
using System.Reflection;
using Appccelerate.StateMachine;

namespace IBSampleApp
{
    // Basic interface/methods of any finite state machines
    public interface IFiniteStateMachine
    {
        // Return enum type for all allowed states
        Type GetStates();

        // Return enum type for all allowed events
        Type GetEvents();

        // Return all defined state transitions for the FSM
        StateTransition[] GetTransitions();

        // Fire an event for the state machine
        void FireEvent(System.Enum newEvent);

        // Required entry and exit state methods.
        void Initialize();
        void Terminate();
    }

    // Finite state machine class
    public class FiniteStateMachine
    {
        // Convenience fields to identify the Enums used for States and Events definition in every FSM
        private static string StatesEnumName = "States";

        // Internal state machine. See Appccelerate docs for other types
        private PassiveStateMachine<string, string> _fsm = new PassiveStateMachine<string, string>();
        // These two states are for initialization and cleanup
        private static string _initializeStateName = "Initialize";
        private static string _terminateStateName = "Terminate";

        // used to make the initial state transition from entry state
        private static string _initalizedEventName = "Initialized";

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

        // Return array of your StateTransitions
        protected virtual StateTransition[] GetTransitions()
        {
            throw new NotImplementedException();
        }

        // Return an action method signature used by all state methods.
        protected virtual Type GetActionSignature()
        {
            // Default Action has no parameters.
            return typeof(Action<>);
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
            // Start the FSM. This will call the "Initialized" state
            _fsm.Start();
            // Initial transition from Entry to first state
            _fsm.Fire(_initalizedEventName, eventArg);
        }
        #endregion

        // Constructor
        /// <summary>
        /// FSM Constructor
        /// </summary>
        /// <param name="startFSM">true=>start FSM.</param>
        public FiniteStateMachine(bool startFSM=true, bool assignActionsManually=false)
        {
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
                        dynamic methodAction = Delegate.CreateDelegate(this.GetActionSignature(), this, stateName);
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
                _fsm.Start();
            }
        }

        // contains some reflection code we may want to use later. Ignore it for now
        private void Initialize2()
        {
            Type classtype = this.GetType();
            MethodInfo[] classmethods = Type.GetType(this.ToString()).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MemberInfo[] classmembers = classtype.GetMembers();
            var qualifiedStateEnumName = String.Format("{0}.{1}+{2}", classtype.Namespace, classtype.Name, FiniteStateMachine.StatesEnumName);
            var entryAssembly = Assembly.GetEntryAssembly();
            string[] stateNames = null;

            // LINQ to pull the "States" enum from the class members so we can get a list of the FSM states
            var stateEnum = from member in classmembers where member.Name == FiniteStateMachine.StatesEnumName select member;
            if (stateEnum.Any())
            {
                var stateEnumType = entryAssembly.GetType(qualifiedStateEnumName);
                stateNames = Enum.GetNames(stateEnumType);
            }
            else
            {
                throw new NotImplementedException();
            }

            // TODO: Assign execution for all defined states
            foreach (string stateName in stateNames)
            {
                var stateMethod = from method in classmethods where method.Name == stateName select method;

                if (!stateMethod.Any())
                {
                    throw new NotImplementedException();
                }
                Action methodAction = (Action)Delegate.CreateDelegate(typeof(Action), this, stateName);
            }
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

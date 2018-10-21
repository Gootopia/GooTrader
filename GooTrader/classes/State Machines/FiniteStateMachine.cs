using System;
using System.Linq;
using System.Reflection;
using Appccelerate.StateMachine;

namespace IBSampleApp
{
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
        void Entry();
        void Exit();
    }

    public class FiniteStateMachine : IFiniteStateMachine
    {
        // Convenience fields to identify the Enums used for States and Events definition in every FSM
        private static string StatesEnumName = "States";

        // Internal state machine. See Appccelerate docs for other types
        private ActiveStateMachine<string, string> _fsm = new ActiveStateMachine<string, string>();
        private static string _entryStateName = "Entry";

        // Required state methods. Normally they do nothing, but they can be overridden.
        #region State Methods
        public virtual void Entry() { }

        public virtual void Exit() { }
        #endregion

        // These are "placeholder" functions that user MUST override!
        #region "PlaceHolder" Methods
        // Return typeof(YourEventsEnum)
        public virtual Type GetEvents()
        {
            throw new NotImplementedException();
        }

        // Return typeof(YourStatesEnum)
        public virtual Type GetStates()
        {
            throw new NotImplementedException();
        }

        // Return array of your StateTransitions
        public virtual StateTransition[] GetTransitions()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generate event to transition from one state to another
        /// </summary>
        /// <param name="newEvent"></param>
        public void FireEvent(Enum newEvent)
        {
            Type enumType = this.GetEvents();
            Type eventType = newEvent.GetType();

            // Make sure event passed was of the same enum type. We're not checking that we passed the right event for the situation,
            // just making sure we didn't send in the wrong type by accident.
            if(eventType == enumType)
            {
                _fsm.Fire(newEvent.ToString());
            }
        }
        #endregion

        // Constructor
        public FiniteStateMachine()
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

            // TODO: Add error checking to make sure "Entry" and "Exit" states are in state list

            // Create an action for each state method
            foreach (string stateName in stateNames)
            {
                // TODO: Future use could add "Entry" and "Exit" to method signatures to allow more flexible execution
                Action methodAction = (Action)Delegate.CreateDelegate(typeof(Action), this, stateName);
                _fsm.In(stateName).ExecuteOnEntry(methodAction);
            }

            // Starting state is always the "Entry" state.
            _fsm.Initialize(_entryStateName);
        }

        // contains some reflection code we may want to use later. 
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

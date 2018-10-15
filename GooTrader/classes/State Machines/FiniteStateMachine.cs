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

        // Required entry and exit state methods.
        void Entry();
        void Exit();
    }

    public class FiniteStateMachine : IFiniteStateMachine
    {
        // Convenience fields to identify the Enums used for States and Events definition in every FSM
        private static string StatesEnumName = "States";

        // Internal state machine. See Appccelerate docs for other types (async, active)
        private PassiveStateMachine<string, string> _fsm = new PassiveStateMachine<string, string>();
        private static string _entryStateName = "Entry";

        // Required state methods. Normally they do nothing, but they can be overridden.
        #region State Methods
        public virtual void Entry()
        {
            throw new NotImplementedException();
        }

        public virtual void Exit()
        {
        }
        #endregion

        #region Methods
        // These are "placeholder" functions that user MUST override!
        public virtual Type GetEvents()
        {
            throw new NotImplementedException();
        }

        public virtual Type GetStates()
        {
            throw new NotImplementedException();
        }

        public virtual StateTransition[] GetTransitions()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void Initialize()
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
            _fsm.Start();
        }

        public void Initialize2()
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

            // Build all allowed transitions. Note that hierarchy is not currently supported with this implementation
            //foreach (StateTransition t in this.Transitions)
            //{
            //    _fsm.In(t.InState.ToString()).On(t.OnEvent.ToString()).Goto(t.GotoState.ToString());
            //}

            // Assign execution code for required states (by default they do nothing, but user can override)
            //_fsm.In(States.Entry.ToString()).ExecuteOnEntry(this.Entry);
            //_fsm.In(States.Exit.ToString()).ExecuteOnEntry(this.Exit);

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

            // Initial state is always the Entry state
            //_fsm.Initialize(States.Entry.ToString());

            // State machine must be started for events to process. If not started, they will queue.
            //_fsm.Start();
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

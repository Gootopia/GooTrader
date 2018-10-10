using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Appccelerate.StateMachine;

namespace IBSampleApp
{
    public class FiniteStateMachine
    {
        // Convenience fields to identify the Enums used for States and Events definition in every FSM
        private static string StatesEnumName = "States";
        private static string EventsEnumName = "Events";

        // States Definitions. Override with "new" in your state machine class definition
        public enum States { Entry, Exit }

        // Internal state machine. See Appccelerate docs for other types (async, active)
        private PassiveStateMachine<string, string> _fsm = new PassiveStateMachine<string, string>();

        // List of state transitions. Override this field with "new" in your actual state machine
        public StateTransition[] Transitions;

        // Required state methods
        #region State Methods
        public virtual void Entry()
        {
            throw new NotImplementedException();
        }

        public virtual void Exit()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void Initialize()
        {
            // TODO: Need to assign an "ExecuteOnEntry" to each state
            Type classtype = this.GetType();

            MethodInfo[] classmethods = Type.GetType(this.ToString()).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MemberInfo[] classmembers = classtype.GetMembers();
            string[] stateNames;

            // Need to find the "States" enum so we can 
            foreach (MemberInfo m in classmembers)
            {
                var memberName = m.Name;
                var entryAssembly = Assembly.GetEntryAssembly();

                if(memberName.Equals(FiniteStateMachine.StatesEnumName) == true)
                {
                    var qualifiedStateEnum = String.Format("{0}.{1}+{2}", classtype.Namespace, classtype.Name, FiniteStateMachine.StatesEnumName);
                    var stateEnumType = entryAssembly.GetType(qualifiedStateEnum);
                    stateNames = Enum.GetNames(stateEnumType);
                }
            }

            // Build all allowed transitions. Note that hierarchy is not currently supported with this implementation
            foreach (StateTransition t in Transitions)
            {
                _fsm.In(t.InState.ToString()).On(t.OnEvent.ToString()).Goto(t.GotoState.ToString());
            }

            // Assign execution code for each state (both Entry and Exit must be defined)


            // Initial state is always the Entry state
            _fsm.Initialize(States.Entry.ToString());

            // State machine must be started for events to process. If not started, they will queue.
            _fsm.Start();
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

    public class FSM_DownloadHistoricalData : FiniteStateMachine
    {
        public enum States
        {
            Entry,
            RequestContractDetails,
            GetHeadTimeStamp,
            RequestHistoricalData,
            StoreData,
            SelectNextDownloadDate,
            Exit
        }

        public enum Events
        {
            EvGotContractDetails,
            EvGotHeadTimeStamp,
            EvGotHistoricalDataPacket,
            EvHistoricalRequestDone
        }

        public new StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Entry, Events.EvGotContractDetails, States.Exit)
        };
    }


    
}

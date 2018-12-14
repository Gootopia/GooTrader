using System;
using System.Globalization;

namespace IBSampleApp
{
    /// <summary>
    /// $itemname$
    /// Summary of the state machine goes here.
    /// </summary>
    public class $itemname$ : FiniteStateMachine
    {
        /// <summary>
        /// See FiniteStateMachine for constructor details.
        /// </summary>
        public $itemname$() : base(Nullable, true, false) { }
        
        // These define the state and event names and the transitions
        #region FSM Definition
        // All valid states
        public enum States
        {
            Initialize,
            EXAMPLE_STATE1,
            EXAMPLE_STATE2,
            Terminate
        }

        // All valid transition events.
        public enum Events
        {
            Initialized,
            EXAMPLE_TRANSITION_EVENT,
            Finished
        }

        // Valid state transitions
        protected StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.Initialized, States.EXAMPLE_STATE1),
            new StateTransition(States.EXAMPLE_STATE1, Events.EXAMPLE_TRANSITION_EVENT, States.EXAMPLE_STATE2),
            new StateTransition(States.EXAMPLE_STATE2, Events.Finished, States.Terminate)
        };
        #endregion

        // The methods are needed to initialize the FSM.
        // NOTE: You don't need to do anything with these
        #region FSM Initialization
        #region NO_MODIFY
        protected override Type GetStates()
        {
            return typeof(States);
        }

        public override Type GetEvents()
        {
            return typeof(Events);
        }

        protected override StateTransition[] GetTransitions()
        {
            return Transitions;
        }
        #endregion NO_MODIFY

        #region MODIFY
        protected override Type GetStateObjectType()
        {
            // This is the host object type of whatever is using the FSM.
            throw new NotImplementedException();
        }

        protected override Type GetStateMethodSignature()
        {
            // All types will use this signature
            //return typeof(Action<FSM_EventArgs>);
            return base.GetStateMethodSignature(); 
        }
        #endregion MODIFY
        #endregion

        // Methods for individual states. Should be private or protected to hide them since they don't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods

        private void EXAMPLE_STATE1(FSM_EventArgs e)
        {
            // Do Stuff
            if (condition) { FireEvent(FSM_Template.Events.EXAMPLE_TRANSITION_EVENT); }
        }
        #endregion
    }
}

using System;
using System.Globalization;

namespace IBSampleApp
{
    /// <summary>
    /// FSM_TwsConnectivity
    /// State machine to handle client connectivity to TWS
    /// </summary>
    public class FSM_TwsConnectivity : FiniteStateMachine
    {
        /// <summary>
        /// See FiniteStateMachine constructor for parameters.
        /// </summary>
        public FSM_TwsConnectivity() : base(null, false, false) { }

        // These define the state and event names and the transitions
        #region FSM Definition
        // All valid states
        public enum States
        {
            Initialize,
            NotConnected,
            ReadyToConnect,
            TryConnection,
            Connected,
            BrokenSocket,
            FailedConnection,
            Terminate
        }

        // All valid transition events.
        public enum Events
        {
            Initialized,
            TwsRunning,
            Connect,
            TWS_Error_0,
            TWS_nextValidId,
            TWS_Error_507,
            Disconnected,
            Finished
        }

        // Valid state transitions
        protected StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.Initialized, States.NotConnected),
            new StateTransition(States.NotConnected,Events.TwsRunning, States.ReadyToConnect),
            new StateTransition(States.ReadyToConnect, Events.Connect, States.TryConnection),
            new StateTransition(States.TryConnection, Events.TWS_Error_0, States.FailedConnection),
            new StateTransition(States.TryConnection, Events.TWS_nextValidId, States.Connected),
            new StateTransition(States.FailedConnection, Events.Disconnected, States.NotConnected),
            new StateTransition(States.Connected, Events.TWS_Error_507, States.BrokenSocket),
            new StateTransition(States.BrokenSocket, Events.Disconnected, States.NotConnected)
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
            // All types will use this signature, which defaults to no parameters and no return.
            return base.GetStateMethodSignature();
        }
        #endregion MODIFY
        #endregion

        // Methods for individual states. Should be private or protected to hide them since they don't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods

        // Disconnected from TWS.
        public void NotConnected()
        {
            // TODO: Detect if TWS application is running. For now we will assume it is.
            FireEvent(Events.TwsRunning);
        }

        // TWS is verified to be running so is available for connection
        private void ReadyToConnect()
        {
        }

        private void TryConnection()
        {
            TWS.Connect();
        }

        private void Connected()
        {
            TWS.Connected();
        }

        private void BrokenSocket()
        {
        }

        private void FailedConnection()
        {

        }

        #endregion
    }
}

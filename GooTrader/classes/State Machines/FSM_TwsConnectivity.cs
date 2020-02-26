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
        public FSM_TwsConnectivity() :
            base(typeof(States), typeof(Events), Transitions, typeof(Action<FSM_EventArgs.Payload_Only>)) { }

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
        static protected StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.Initialized, States.NotConnected),
            new StateTransition(States.NotConnected,Events.TwsRunning, States.ReadyToConnect),
            new StateTransition(States.ReadyToConnect, Events.Connect, States.TryConnection),
            new StateTransition(States.TryConnection, Events.TWS_Error_0, States.FailedConnection),
            new StateTransition(States.TryConnection, Events.TWS_nextValidId, States.Connected),
            new StateTransition(States.FailedConnection, Events.Disconnected, States.NotConnected),
            new StateTransition(States.Connected, Events.TWS_Error_507, States.FailedConnection),
            new StateTransition(States.Connected, Events.TWS_Error_0, States.FailedConnection)
        };
        #endregion

        // Methods for individual states. Should be private or protected to hide them since they don't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods

        // Disconnected from TWS.
        public void NotConnected(FSM_EventArgs.Payload_Only e)
        {
            // TODO: Detect if TWS application is running. For now we will assume it is.
            FireEvent(Events.TwsRunning);
        }

        // TWS is verified to be running so is available for connection
        private void ReadyToConnect(FSM_EventArgs.Payload_Only e)
        {
        }

        private void TryConnection(FSM_EventArgs.Payload_Only e)
        {
            TWS.Connect();
        }

        private void Connected(FSM_EventArgs.Payload_Only e)
        {
            TWS.Connected();
        }

        private void FailedConnection(FSM_EventArgs.Payload_Only e)
        {
            TWS.Disconnect();
            FireEvent(Events.Disconnected);
        }

        #endregion
    }
}

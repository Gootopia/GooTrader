using System;

namespace IBSampleApp
{
    // FSM - DownloadHistoricalData
    // Downloads historical price data from broker platform
    public class FSM_DownloadHistoricalData : FiniteStateMachine
    {
        // These define the state and event names and the transitions
        #region FSM Definition
        // All valid states
        public enum States
        {
            Initialize,
            GetHeadTimeStamp,
            StartHistoricalDownload,
            DataReceived,
            RequestDone,
            Terminate
        }

        // All valid transition events.
        public enum Events
        {
            Initialized,
            HeadTimeStamp,
            HistoricalData,
            HistoricalDataEnd,
            DownloadNextDay,
            Finished
        }

        // Valid state transitions
        protected StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.Initialized, States.GetHeadTimeStamp),
            new StateTransition(States.GetHeadTimeStamp, Events.HeadTimeStamp, States.StartHistoricalDownload),
            new StateTransition(States.StartHistoricalDownload, Events.HistoricalData, States.DataReceived),
            new StateTransition(States.DataReceived, Events.HistoricalData, States.DataReceived),
            new StateTransition(States.DataReceived, Events.HistoricalDataEnd, States.RequestDone),
            new StateTransition(States.RequestDone, Events.DownloadNextDay, States.StartHistoricalDownload),
            new StateTransition(States.RequestDone, Events.Finished, States.Terminate)
        };
        #endregion

        // The methods are needed to initialize the FSM.
        #region FSM Initialization
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

        protected override Type GetActionSignature()
        {
            // All types will use this signature
            return typeof(Action<GooContract>);
        }
        #endregion

        // Methods for individual states. Should be private or protected as they shouldn't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods
        private void GetHeadTimeStamp(GooContract c)
        {
            TWS.RequestHeadTimeStamp(c);
        }

        private void StartHistoricalDownload(GooContract c)
        {
            throw new NotImplementedException();
        }

        private void DataReceived(GooContract c)
        {
            throw new NotImplementedException();
        }

        private void RequestDone(GooContract c)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

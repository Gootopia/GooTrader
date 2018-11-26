using System;

namespace IBSampleApp
{
    // FSM Event packet.
    // Payload data can be anything
    public class FSM_EventArgs
    {
        // Contract used by the event
        public GooContract Contract { get; set; }
        // Object payload. Can be anything. The event will convert
        public object Payload { get; set; }

        public FSM_EventArgs(GooContract c, object p=null)
        {
            Contract = c;
            Payload = p;
        }
    }

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
            //return typeof(Action<GooContract>);
            return typeof(Action<FSM_EventArgs>);
        }
        #endregion

        // Methods for individual states. Should be private or protected as they shouldn't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods
        private void GetHeadTimeStamp(FSM_EventArgs evtData)
        {
            TWS.RequestHeadTimeStamp(evtData.Contract);
        }

        private void StartHistoricalDownload(FSM_EventArgs evtData)
        {
            evtData.Contract.HistDataRequestDateTime = DateTime.Now;
            TWS.RequestHistoricalData(evtData.Contract, evtData.Contract.HistDataRequestDateTime, TWSInfo.TWS_StepSizes.Day_1, TWSInfo.TWS_BarSizeSetting.Min_1);
        }

        private void DataReceived(FSM_EventArgs evtData)
        {
            throw new NotImplementedException();
        }

        private void RequestDone(FSM_EventArgs evtData)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Globalization;

namespace IBSampleApp
{
    // FSM - DownloadHistoricalData
    // Downloads historical price data from broker platform
    // See FSM_DownloadHistoricalData in draw.io editor
    public class FSM_DownloadHistoricalData : FiniteStateMachine
    {
        public FSM_DownloadHistoricalData(GooContract c) : base(c) { }
        
        // These define the state and event names and the transitions
        #region FSM Definition
        // All valid states
        public enum States
        {
            Initialize,
            GetHeadTimeStamp,
            TimeStampReceived,
            DownloadHistoricalData,
            DataReceived,
            DataRequestDone,
            Terminate
        }

        // All valid transition events.
        public enum Events
        {
            Initialized,
            HeadTimeStamp,
            StartDownload,
            HistoricalData,
            HistoricalDataEnd,
            DownloadNextDay,
            Finished
        }

        // Valid state transitions
        protected StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.Initialized, States.GetHeadTimeStamp),
            new StateTransition(States.GetHeadTimeStamp, Events.HeadTimeStamp, States.TimeStampReceived),
            new StateTransition(States.TimeStampReceived, Events.StartDownload, States.DownloadHistoricalData),
            new StateTransition(States.DownloadHistoricalData, Events.HistoricalData, States.DataReceived),
            new StateTransition(States.DataReceived, Events.HistoricalData, States.DataReceived),
            new StateTransition(States.DataReceived, Events.HistoricalDataEnd, States.DataRequestDone),
            new StateTransition(States.DataRequestDone, Events.DownloadNextDay, States.DownloadHistoricalData),
            new StateTransition(States.DataRequestDone, Events.Finished, States.Terminate)
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

        protected override Type GetHostType()
        {
            // This is the host object type of whatever is using the FSM.
            return typeof(GooContract);
        }

        protected override StateTransition[] GetTransitions()
        {
            return Transitions;
        }

        protected override Type GetActionSignature()
        {
            // All types will use this signature
            return typeof(Action<FSM_EventArgs>);
        }
        #endregion

        // Methods for individual states. Should be private or protected to hide them since they don't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods
        private void GetHeadTimeStamp(FSM_EventArgs e)
        {
            TWS.RequestHeadTimeStamp(e.Contract);
        }

        private void TimeStampReceived(FSM_EventArgs e)
        {
            // Convert head time stamp to DateTime for processing convenience later on.
            var headTimeStampString = e.Payload as string;
            DateTime dt = DateTime.ParseExact(headTimeStampString, TWSInfo.TimeStampStringFormat, CultureInfo.InvariantCulture);
            e.Contract.HeadTimeStamp = dt;

            // We'll work backwards to the headtimestamp from current time
            e.Contract.HistRequestTimeStamp = DateTime.Now;

            // Transition to start historical download. Can reuse the event args as they'll be the same.
            FireEvent(FSM_DownloadHistoricalData.Events.StartDownload, e);
        }

        private void DownloadHistoricalData(FSM_EventArgs e)
        {
            // Submit a request for 24 hours of 1-min data
            TWS.RequestHistoricalData(e.Contract, e.Contract.HistRequestTimeStamp, TWSInfo.HistoricalDataStepSizes.Day_1, TWSInfo.BarSizeSetting.Min_1);
        }

        private void DataReceived(FSM_EventArgs e)
        {         
            OHLCQuote dataBar = e.Payload as OHLCQuote;

            if(e.Contract.HistoricalData.Data.ContainsKey(dataBar.Date) == false)
            {
                e.Contract.HistoricalData.Data.Add(dataBar.Date, dataBar);
            }
        }

        private void DataRequestDone(FSM_EventArgs e)
        {
            var c = e.Contract;

            // Skip to next day
            e.Contract.HistRequestTimeStamp = e.Contract.HistRequestTimeStamp.AddDays(-1);

            if(DateTime.Compare(c.HeadTimeStamp, c.HistRequestTimeStamp) < 0)
            {
                FireEvent(FSM_DownloadHistoricalData.Events.DownloadNextDay, e);

                string msg = String.Format("{0}: {1}", e.Contract.Symbol, e.Contract.HistRequestTimeStamp.ToString());
                MessageLogger.LogMessage(msg);
            }
            else
            {
                FireEvent(FSM_DownloadHistoricalData.Events.Finished);
            }
        }
        #endregion
    }
}

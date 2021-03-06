﻿using System;
using System.Globalization;

namespace IBSampleApp
{
    // FSM - DownloadHistoricalData
    // Downloads historical price data from broker platform
    // See FSM_DownloadHistoricalData in draw.io editor
    public class FSM_DownloadHistoricalData : FiniteStateMachine
    {       
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

        // Valid state transitions. Must be static so we can pass to the constructor 
        static StateTransition[] Transitions = new StateTransition[]
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

        // Constructor
        public FSM_DownloadHistoricalData(GooContract c) :
            base(typeof(States), typeof(Events), Transitions, typeof(Action<FSM_EventArgs.GooContract_With_Payload>), c) { }

        // Methods for individual states. Should be private or protected to hide them since they don't need to be called directly.
        // NOTE: NAMES NEED TO MATCH STATES EXACTLY OR EXCEPTIONS WILL BE GENERATED!
        #region State Methods
        private void GetHeadTimeStamp(FSM_EventArgs.GooContract_With_Payload e)
        {
            TWS.RequestHeadTimeStamp(e.Contract);
        }

        private void TimeStampReceived(FSM_EventArgs.GooContract_With_Payload e)
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

        private void DownloadHistoricalData(FSM_EventArgs.GooContract_With_Payload e)
        {
            // Submit a request for 24 hours of 1-min data
            TWS.RequestHistoricalData(e.Contract, e.Contract.HistRequestTimeStamp, TWSInfo.HistoricalDataStepSizes.Day_1, TWSInfo.BarSizeSetting.Min_1);
        }

        private void DataReceived(FSM_EventArgs.GooContract_With_Payload e)
        {         
            OHLCQuote dataBar = e.Payload as OHLCQuote;

            if(e.Contract.HistoricalData.Data.ContainsKey(dataBar.Date) == false)
            {
                e.Contract.HistoricalData.Data.Add(dataBar.Date, dataBar);
            }
        }

        private void DataRequestDone(FSM_EventArgs.GooContract_With_Payload e)
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

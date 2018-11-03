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
            RequestHistoricalData,
            StoreData,
            SelectNextDownloadDate,
            Terminate
        }

        // All valid transition events.
        public enum Events
        {
            GotContractDetails,
            GotHeadTimeStamp,
            GotHistoricalDataPacket,
            HistoricalRequestDone,
            AllDataReceived
        }

        // Valid state transitions
        protected StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.GotContractDetails, States.GetHeadTimeStamp),
            new StateTransition(States.GetHeadTimeStamp, Events.GotHeadTimeStamp, States.RequestHistoricalData),
            new StateTransition(States.RequestHistoricalData, Events.GotHistoricalDataPacket, States.StoreData),
            new StateTransition(States.StoreData, Events.GotHistoricalDataPacket, States.StoreData),
            new StateTransition(States.StoreData, Events.HistoricalRequestDone, States.SelectNextDownloadDate),
            new StateTransition(States.SelectNextDownloadDate, Events.GotHistoricalDataPacket, States.StoreData),
            new StateTransition(States.SelectNextDownloadDate, Events.AllDataReceived, States.Terminate)
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

        // Methods for individual states. Should be private or protected as they shouldn't need to be called directly
        #region State Methods
        private void GetHeadTimeStamp(GooContract c)
        {
            //throw new NotImplementedException();
        }

        private void RequestHistoricalData(GooContract c)
        {
            throw new NotImplementedException();
        }

        private void StoreData(GooContract c)
        {
            throw new NotImplementedException();
        }

        private void SelectNextDownloadDate(GooContract c)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

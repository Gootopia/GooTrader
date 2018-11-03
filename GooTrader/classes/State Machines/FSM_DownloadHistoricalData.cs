using System;

namespace IBSampleApp
{
    // Strongly recommended to use interface to define state methods so proper signature can be enforced.
    public interface IFSM_DownloadHistoricalData
    {
        void GetHeadTimeStamp(GooContract c);
        void RequestHistoricalData(GooContract c);
        void StoreData(GooContract c);
        void SelectNextDownloadDate(GooContract c);
    }

    // FSM - DownloadHistoricalData
    // Downloads historical price data from broker platform
    public class FSM_DownloadHistoricalData : FiniteStateMachine, IFSM_DownloadHistoricalData
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
        public StateTransition[] Transitions = new StateTransition[]
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
        public override Type GetStates()
        {
            return typeof(States);
        }

        public override Type GetEvents()
        {
            return typeof(Events);
        }

        public override StateTransition[] GetTransitions()
        {
            return Transitions;
        }

        public override Type GetActionSignature()
        {
            // All types will use this signature
            return typeof(Action<GooContract>);
        }
        #endregion

        // Methods for individual states
        #region State Methods
        public void GetHeadTimeStamp(GooContract c)
        {
            //throw new NotImplementedException();
        }

        public void RequestHistoricalData(GooContract c)
        {
            throw new NotImplementedException();
        }

        public void StoreData(GooContract c)
        {
            throw new NotImplementedException();
        }

        public void SelectNextDownloadDate(GooContract c)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

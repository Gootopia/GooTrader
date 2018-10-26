using System;

namespace IBSampleApp
{
    // Strongly recommended to use interface to define state methods so proper signature can be enforced.
    public interface IFSM_DownloadHistoricalData
    {
        void RequestContractDetails();
        void GetHeadTimeStamp();
        void RequestHistoricalData();
        void StoreData();
        void SelectNextDownloadDate();
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
            RequestContractDetails,
            GetHeadTimeStamp,
            RequestHistoricalData,
            StoreData,
            SelectNextDownloadDate,
            Terminate
        }

        // All valid transition events.
        public enum Events
        {
            RequestContractDetails,
            GotContractDetails,
            GotHeadTimeStamp,
            GotHistoricalDataPacket,
            HistoricalRequestDone,
            AllDataReceived
        }

        // Valid state transitions
        public StateTransition[] Transitions = new StateTransition[]
        {
            new StateTransition(States.Initialize, Events.RequestContractDetails, States.RequestContractDetails),
            new StateTransition(States.RequestContractDetails, Events.GotContractDetails, States.GetHeadTimeStamp),
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
        #endregion

        // Methods for individual states
        #region State Methods
        public void RequestContractDetails()
        {
            throw new NotImplementedException();
        }

        public void GetHeadTimeStamp()
        {
            throw new NotImplementedException();
        }

        public void RequestHistoricalData()
        {
            throw new NotImplementedException();
        }

        public void StoreData()
        {
            throw new NotImplementedException();
        }

        public void SelectNextDownloadDate()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

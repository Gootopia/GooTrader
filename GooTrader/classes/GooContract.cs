using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;

namespace IBSampleApp
{
    // Container class with all required state machines for implementing TWS operations.
    public class TWSFiniteStateMachines
    {
        // All the various state machines can go here
        public FSM_DownloadHistoricalData DownloadHistoricalData { get; set; }

        public TWSFiniteStateMachines(GooContract c)
        {
            // state machines need the contract so it can be accessed during execution of the FSM.
            DownloadHistoricalData = new FSM_DownloadHistoricalData(c);
        }
    }

    // Information about a specific trading instrument
    public class GooContract : PropertyUpdater
    {
        // Properties below don't need UpdateProperty in the settor as ViewModel doesn't access them
        #region Standard Properties
        // Contract details for the given contract. Note that we must be specific enough when requesting details that
        // the details apply only to a single tradable instrument (i.e: futures contract). At most, the contract
        // differences should be limited to a single instrument with multiple expirations.

        // Active TWS contract. This would be what is used for live data/orders, etc. (IB Contract is inside contract details).
        // For stocks, there is only one. For other stuff (futures), there could be multiple expirations, which are held in the list below
        public IBApi.ContractDetails TWSActiveContractDetails { get; set; }

        // All available contract detail instances for this particular instrument
        public List<IBApi.ContractDetails> TWSContractDetailsList = new List<IBApi.ContractDetails>();

        // Timestamp of the furthest out data that is available for this contract. TWS returns a string, but DateTimes are easier to use
        public DateTime HeadTimeStamp { get; set; }
        
        // Keeps track of where we are in the data request. We download one day at a time and need a separate request for each.
        public DateTime HistRequestTimeStamp { get; set; }

        // Collection of all historical data for the contract. We only store data for one contract (the active one)
        public OHLCData HistoricalData = new OHLCData();
        #endregion

        // Properties below should call UpdateProperty() in the settor as they could be referenced by the ViewModel
        #region Properties with UpdateNotify
        // Contract Descriptions ("S&P500", "Nasdaq", etc.)
        private string _name;
        public string Name
        {
            get { return _name; }
            set => UpdateProperty(ref _name, value);
        }

        // Active expiration of a contract
        private string _expiration;
        public string Expiration
        {
            get { return _expiration; }
            set => UpdateProperty(ref _expiration, value);
        }

        // Contract Symbol ("ES", "NQ", etc.)
        private string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set => UpdateProperty(ref _symbol, value);
        }

        // Contract Price Info
        private double _bid;
        private double _ask;
        private double _last;
        public double Bid
        {
            get { return _bid; }
            set => UpdateProperty(ref _bid, value);
        }
        public double Ask
        {
            get { return _ask; }
            set => UpdateProperty(ref _ask, value);
        }
        public double Last
        {
            get { return _last; }
            set => UpdateProperty(ref _last, value);
        }
        #endregion

        // This is used by TWS to perform various framework functions for this contract (data downloads, order processing, etc.)
        public TWSFiniteStateMachines FSM { get; set; }

        public GooContract()
        {
            // Save off the FSM host object for use during FSM execution
            FSM = new TWSFiniteStateMachines(this);

            // Some dummy values just so we know we've got the correct binding
            Bid = 0.0;
            Ask = 1.0;
            Last = 2.0;
        }
    }
}

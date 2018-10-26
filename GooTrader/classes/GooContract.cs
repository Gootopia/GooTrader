﻿using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;

namespace IBSampleApp
{
    public class ContractFSM
    {
        readonly FSM_DownloadHistoricalData fsm_download;
    }

    // Information about a specific trading instrument
    public class GooContract : PropertyUpdater
    {
        // Properties below don't need UpdateProperty in the settor as ViewModel doesn't access them
        #region Standard Properties
        // Information specific to IB Platform
        // Contract details for the given contract. Note that we must be specific enough when requesting details that
        // the details apply only to a single tradable instrument (i.e: futures contract). At most, the contract
        // differences should be limited to a single instrument with multiple expirations.
        public IBApi.ContractDetails TWSContractDetails { get; set; }

        // Timestamp of the furthest out data that is available for this contract
        public string HeadTimeStampString { get; set; }
        // Keeps track of where we are in the data request. We download one day at a time and need a separate request for each.
        public DateTime HistDataRequestDateTime { get; set; }
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

        public GooContract()
        {
            Bid = 0.0;
            Ask = 1.0;
            Last = 2.0;
        }
    }
}

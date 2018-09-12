using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System;

namespace IBSampleApp
{
    /// <summary>
    /// Model is the domain object for program data. It knows nothing of the ViewModel or the View.
    /// To incorporate data from the model, a mirror object should be created in the View model which pulls out the relevant data.
    /// </summary>
    public class Model
    {
        // Offset between local and TWS time
        public TimeSpan ServerTimeOffset { get; set; }

        // Contracts list. Keys are "Ticker_PrimaryExchange"
        public Dictionary<string, GooContract> Contracts { get; set; }

        // Data request lookup. Used for indexing tick data streams
        // Stores which contractkey (TWS_ContractKey) for a given tick data reqId since that is what is returned in the tick events
        // This is a two-step process:
        // 1) Index the contract key given the data request Id
        // 2) Index the contract given the contract key
        public Dictionary<int, string> DataRequests { get; set; }

        #region Constructor
        public Model()
        {
            Contracts = new Dictionary<string, GooContract>();
            DataRequests = new Dictionary<int, string>();
        }
        #endregion Constructor
    }
}

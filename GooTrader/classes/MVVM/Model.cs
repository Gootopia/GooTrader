using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System;

namespace GooTrader
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

        #region Constructor
        public Model()
        {
            Contracts = new Dictionary<string, GooContract>();
        }
        #endregion Constructor
    }
}

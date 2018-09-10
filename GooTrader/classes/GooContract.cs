using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace IBSampleApp
{
    public class GooContract : PropertyUpdater
    {
        // Information specific to IB Platform
        // Contract details for the given contract. Note that we must be specific enough when requesting details that
        // the details apply only to a single tradable instrument (i.e: futures contract). At most, the contract
        // differences should be limited to a single instrument with multiple expirations.
        public IBApi.ContractDetails TWSContractDetails { get; set; }

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
    }
}

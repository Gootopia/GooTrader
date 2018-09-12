using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IBSampleApp
{
    /// <summary>
    /// ViewModel class which exposes model data that can be bound to the View.
    /// If you want something in the UI, put it here and bind it in XAML. No Exceptions!
    /// </summary>
    public class ViewModel : PropertyUpdater
    {
        // Available contracts
        public ObservableCollection<GooContract> Contracts { get; set; }

        // Message Log
        public ObservableCollection<LogMessage> Messages { get; set; }

        // Connection Status to TWS
        private bool _istwsconnected;
        public bool IsTwsConnected
        {
            get { return _istwsconnected; }
            set { UpdateProperty(ref _istwsconnected, value); }
        }

        // System Time for running clock display
        private DateTime _systemtime;
        public DateTime SystemTime
        {
            get { return _systemtime; }
            set { UpdateProperty(ref _systemtime, value); }
        }

        #region Constructor
        public ViewModel()
        {
            // Need to create an instance for each view item
            Contracts = new ObservableCollection<GooContract>();
            Messages = new ObservableCollection<LogMessage>();
        }
        #endregion Constructor
    }
}

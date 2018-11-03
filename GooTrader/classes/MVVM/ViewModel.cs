﻿using System.Collections.ObjectModel;
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
        public static ObservableCollection<GooContract> Contracts { get; set; }

        // Message Log
        public static ObservableCollection<LogMessage> Messages { get; set; }

        // Connection status to TWS
        #region IsTwsConnected
        private bool _isTwsConnected;
        public bool IsTwsConnected
        {
            get => _isTwsConnected;
            set => UpdateProperty(ref _isTwsConnected, value);
        }

        #endregion

        #region SystemTime
        private DateTime _systemTime;
        public DateTime SystemTime
        {
            get { return _systemTime; }
            set => UpdateProperty(ref _systemTime, value);
        }
        #endregion

        public void SetTwsConnectionState(bool state)
        {
            // From in here, we can update this property => PropertyUpdater works ok (we have this instance)
            IsTwsConnected = state;
        }

        #region Constructor
        public ViewModel()
        {
            // Need to create an instance for each view item
            Contracts = new ObservableCollection<GooContract>();
            Messages = new ObservableCollection<LogMessage>();

            // Events to trigger updates in the UI
            TWS.OnConnectionStatusChanged += this.SetTwsConnectionState;
        }

        #endregion Constructor
    }
}

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

        // The following is a rather roundabout way to update a bound property from an external static method
        // This is due to the fact that we can't use the PropertyUpdate Invoke method because we don't have access to the instance
        // Therefore, we do the following:
        // - Create a static event, a class method to raise the event, and a class method to modify the bound property.
        // - Added the modification method to the event in the constructor
        // - In the external static method, call the method which raises the event.
        #region SetTwsConnectionState Event
        public static event Action<bool> TwsConnectionStateChanged;

        public static void RaiseTwsConnectionStateChangedEvent(bool state)
        {
            // Outside the class, an event can only be on left side (+=), so we need a helper function to do it.
            if(TwsConnectionStateChanged == null)
            {
                // It should not be null since we assign it in the constructor. If it is, need to figure out why.
                throw new NullReferenceException();
            }
            
            TwsConnectionStateChanged(state);
        }

        public void SetTwsConnectionState(bool state)
        {
            // From in here, we can update this property => PropertyUpdater works ok (we have this instance)
            IsTwsConnected = state;
        }
        #endregion

        #region Constructor
        public ViewModel()
        {
            // Need to create an instance for each view item
            Contracts = new ObservableCollection<GooContract>();
            Messages = new ObservableCollection<LogMessage>();
            TwsConnectionStateChanged += SetTwsConnectionState;
        }

        #endregion Constructor
    }
}

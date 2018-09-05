using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System;

namespace GooTrader
{
    /// <summary>
    /// ViewModel class which exposes model data that can be bound to the View.
    /// If you want something in the UI, put it here and bind it in XAML. No Exceptions!
    /// </summary>
    public class ViewModel : DependencyObject
    {
        // Available contracts
        public ObservableCollection<GooContract> Contracts { get; set; }

        // Message Log
        public ObservableCollection<LogMessage> Messages { get; set; }

        // Connection status to TWS
        #region IsTwsConnected
        public bool IsTwsConnected
        {
            get { return (bool)GetValue(IsTwsConnectedProperty); }
            set { SetValue(IsTwsConnectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTwsConnected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTwsConnectedProperty =
            DependencyProperty.Register("IsTwsConnected", typeof(bool), typeof(ViewModel), new PropertyMetadata(false));
        #endregion IsTwsConnected

        // System Time for running clock display
        #region SystemTime
         public DateTime SystemTime
        {
            get { return (DateTime)GetValue(SystemTimeProperty); }
            set { SetValue(SystemTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SystemTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SystemTimeProperty =
            DependencyProperty.Register("SystemTime", typeof(DateTime), typeof(ViewModel), new PropertyMetadata(new DateTime(1,1,1)));
        #endregion

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

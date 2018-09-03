using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

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
        #endregion IsConnected

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

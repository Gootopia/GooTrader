using System.Collections.ObjectModel;

namespace GooTrader
{
    /// <summary>
    /// ViewModel class which may be bound to the View.
    /// </summary>
    public class ViewModel
    {
        // Available contracts
        public ObservableCollection<GooContract> contracts { get; set; }

        // Message Log
        public ObservableCollection<LogMessage> messages { get; set; }

        #region Constructor
        public ViewModel()
        {
            // Need to create an instance for each view item
            contracts = new ObservableCollection<GooContract>();
            messages = new ObservableCollection<LogMessage>();
        }
        #endregion Constructor
    }
}

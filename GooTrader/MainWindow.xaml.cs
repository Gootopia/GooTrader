using System;
using System.Windows;
using System.Windows.Threading;

// Use default namespace so we don't have to rename stuff when updating to a new TWS API
namespace IBSampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Instance of the model
        public Model m = new Model();
        
        // Need an instance of the view model so we can bind to UI
        public ViewModel vm = new ViewModel();

        public MainWindow()
        {
            #region System Initialization
            InitializeComponent();

            // Top level data context is the viewmodel. 
            main.DataContext = vm;

            // MessageLogger is static class, so need to assign a messages collection. Use the one from the Viewmodel as UI binding is already set.
            MessageLogger.messages = ViewModel.Messages;

            // System timer for clock
            DispatcherTimer sysClock = new DispatcherTimer();
            sysClock.Interval = TimeSpan.FromSeconds(1);
            sysClock.Tick += SysClock_Tick;
            sysClock.Start();
            #endregion System Initialization

            // Start the TWS connectivity state machine.
            TWS.FSM.Connection.Start();
            MessageLogger.LogMessage("Starting connection state machine.");
        }

        // 1 second tick timer for updating clock
        private void SysClock_Tick(object sender, EventArgs e)
        {
            // While connected to TWS, need to sync the time
            if (vm.IsTwsConnected == true)
                vm.SystemTime = DateTime.Now + TWS.ServerTimeOffset;
            else
                // Tiny, tiny offset to force notify of change property, but won't change the year.
                vm.SystemTime = vm.SystemTime.AddTicks(1);
        }
    }
}

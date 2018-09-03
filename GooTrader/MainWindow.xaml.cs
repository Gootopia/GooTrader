using IBApi;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace GooTrader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // MVVM components
        public ViewModel vm = new ViewModel();
        public Model model = new Model();

        // Reader for passing message between this app and TWS host
        private EReaderMonitorSignal signal = new EReaderMonitorSignal();

        // ib client for interaction with TWS
        public IBClient ib;
        
        public MainWindow()
        {
            InitializeComponent();

            ib = new IBClient(signal);
           
            // Top level data context is the viewmodel. 
            main.DataContext = vm;

            // MessageLogger is static class, so need to assign a messages collection. Just use the one from the ViewModel
            MessageLogger.messages = vm.messages;

            // Event handlers for all desired TWS events
            ib.NextValidId += Ib_NextValidId;
            ib.ConnectionClosed += Ib_ConnectionClosed;
            ib.Error += Ib_Error;
            ib.ContractDetails += Ib_ContractDetails;
            ib.ContractDetailsEnd += Ib_ContractDetailsEnd;
         }
    }
}

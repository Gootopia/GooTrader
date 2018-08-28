using IBApi;
using System;
using System.Collections.ObjectModel;
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
        // ViewModel component of MVVM.
        public ViewModel viewmodel = new ViewModel();

        public GooContract c1 = new GooContract("SP500");
        public GooContract c2 = new GooContract("NQ");
        private EReaderMonitorSignal signal = new EReaderMonitorSignal();
        public IBClient ib;

        public MainWindow()
        {
            InitializeComponent();

            ib = new IBClient(signal);
            viewmodel.contracts.Add(c1);
            viewmodel.contracts.Add(c2);

            // Top level data context is the viewmodel.
            main.DataContext = viewmodel;

            // MessageLogger is static class, so need to assign a messages collection. Just use the one from the ViewModel
            MessageLogger.messages = viewmodel.messages;

            // Event handlers for all desired TWS events
            ib.NextValidId += Ib_NextValidId;
            ib.ConnectionClosed += Ib_ConnectionClosed;
            ib.Error += Ib_Error;
         }
    }
}

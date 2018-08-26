using IBApi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

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
         }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    ib.ClientId = 0;
                    ib.ClientSocket.eConnect("127.0.0.1", 7497, 0);

                    var reader = new EReader(ib.ClientSocket, signal);

                    reader.Start();

                    new Thread(() => { while (ib.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
                }
                catch (Exception)
                {
                throw new Exception();
                }
            c1.Name = c1.Name + "-*";
            MessageLogger.LogMessage("Pinky!");
        }
    }
}

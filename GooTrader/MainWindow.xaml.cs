using IBApi;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
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
        // MVVM components.
        public ViewModel vm = new ViewModel();
        public Model model = new Model();

        // Reader for passing message between this app and TWS host
        private EReaderMonitorSignal signal = new EReaderMonitorSignal();

        // ib client for interaction with TWS
        IBClient ib;

        public FSM_DownloadHistoricalData fsm_hd = new FSM_DownloadHistoricalData();

        public MainWindow()
        {
            InitializeComponent();

            ib = new IBClient(signal);

            // Top level data context is the viewmodel. 
            main.DataContext = vm;

            // MessageLogger is static class, so need to assign a messages collection. Just use the one from the ViewModel
            MessageLogger.messages = vm.Messages;

            // Event handlers for all desired TWS events
            #region Add Event Handlers
            ib.NextValidId += Ib_NextValidId;
            ib.ConnectionClosed += Ib_ConnectionClosed;
            ib.Error += Ib_Error;
            ib.ContractDetails += Ib_ContractDetails;
            ib.ContractDetailsEnd += Ib_ContractDetailsEnd;
            ib.CurrentTime += Ib_CurrentTime;
            ib.tickByTickAllLast += Ib_tickByTickAllLast;
            ib.tickByTickBidAsk += Ib_tickByTickBidAsk;
            ib.HeadTimestamp += Ib_HeadTimestamp;
            ib.HistoricalData += Ib_HistoricalData;
            ib.HistoricalDataEnd += Ib_HistoricalDataEnd;
            ib.HistoricalDataUpdate += Ib_HistoricalDataUpdate;
            #endregion Add Event Handlers

            // System timer for clock
            DispatcherTimer sysClock = new DispatcherTimer();
            sysClock.Interval = TimeSpan.FromSeconds(1);
            sysClock.Tick += SysClock_Tick;
            sysClock.Start();

            fsm_hd.Initialize();
        }

        private void Ib_HistoricalDataUpdate(messages.HistoricalDataMessage obj)
        {
            throw new NotImplementedException();
        }

        private void Ib_HistoricalDataEnd(messages.HistoricalDataEndMessage hDataEnd)
        {
            GooContract c = TWS_GetDataRequestContract(hDataEnd.RequestId);
            //throw new NotImplementedException();
        }

        private void Ib_HistoricalData(messages.HistoricalDataMessage hdata)
        {
            var msg = String.Format("Time={0},Open={1},High={2},Low={3},Close={4}", hdata.Date, hdata.Open, hdata.High, hdata.Low, hdata.Close);
            MessageLogger.LogMessage(msg);
        }

        // 1 second tick timer for updating clock
        private void SysClock_Tick(object sender, EventArgs e)
        {
            // While connected to TWS, need to sync the time
            if (vm.IsTwsConnected == true)
                vm.SystemTime = DateTime.Now + model.ServerTimeOffset;
            else
                // Tiny, tiny offset to force notify of change property, but won't change the year.
                vm.SystemTime = vm.SystemTime.AddTicks(1);
        }
    }
}

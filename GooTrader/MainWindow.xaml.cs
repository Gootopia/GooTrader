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
        // ViewModel component of MVVM.
        public ViewModel vm = new ViewModel();

        // Reader for passing message between this app and TWS host
        private EReaderMonitorSignal signal = new EReaderMonitorSignal();

        // ib client for interaction with TWS
        public IBClient ib;

        public Dictionary<string, GooContract> contracts = new Dictionary<string, GooContract>();
        
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

        private void Ib_ContractDetailsEnd(int obj)
        {
            //throw new NotImplementedException();
        }

        private void Ib_ContractDetails(int arg1, ContractDetails contractDetails)
        {
            // Because contract is bound in Viewmodel, it must be created on the UI thread.
            // Also, because thread operations
            UIThread.Update(() =>
            {
                var contractKey = contractDetails.MarketName + "_" + contractDetails.ValidExchanges;
                GooContract currentContract;

                if(contracts.ContainsKey(contractKey) == false)
                {
                    currentContract = new GooContract();
                    currentContract.Name = contractDetails.LongName;
                    currentContract.Symbol = contractDetails.MarketName;
                    contracts.Add(contractKey, currentContract);
                    vm.contracts.Add(currentContract);
                }
            });

            //throw new NotImplementedException();
        }
    }
}

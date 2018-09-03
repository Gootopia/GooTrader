using IBApi;
using System;
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
        public static class Exchanges
        {
            public static string Globex = "GLOBEX";
        }

        /// <summary>
        /// Open a connection to TWS
        /// </summary>
        /// <returns></returns>
        public void TWS_Connect()
        {
            try
            {
                // Open a connection to TWS
                ib.ClientId = 0;
                ib.ClientSocket.eConnect("127.0.0.1", 7497, 0);

                // Start an IB reader thread
                var reader = new EReader(ib.ClientSocket, signal);
                reader.Start();

                // background thread to process TWS messages
                new Thread(() => { while (ib.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        #region TWS Event Handlers
        private void Ib_ConnectionClosed()
        {
            MessageLogger.LogMessage("Connection Lost!");
        }

        // Error message handler.
        // Handles both standard error codes as well as exceptions
        private void Ib_Error(int id, int errorCode, string errorMsg, Exception exception)
        {
            // Default message just prints error codes
            var errMsg = String.Format("ID={0},Error={1}:{2}", id.ToString(), errorCode.ToString(), errorMsg);

            // Exception implies stream issues (TWS went away)
            if (exception != null)
            {
                ib.ClientSocket.eDisconnect();
                errMsg = "TWS Disconnected!";
            }

            MessageLogger.LogMessage(errMsg);
        }

        // Valid ID signal from TWS implies TWS is ready to recieve commands
        private void Ib_NextValidId(int id)
        {
            ib.NextOrderId = id;
            // Access on viewmodel is prohibited outside UI thread unless using Dispatcher!
            UIThread.Update(() => vm.IsTwsConnected = true);

            // nextValidId event means TWS is ready to Go!
            ib.ClientSocket.IsConnected();
        }

        private void Ib_ContractDetailsEnd(int reqId)
        {
            MessageLogger.LogMessage(String.Format("ContractDetails request {0} completed", reqId.ToString()));
        }

        private void Ib_ContractDetails(int reqId, ContractDetails contractDetails)
        {
            var contractName = contractDetails.LongName + contractDetails.ContractMonth;
            MessageLogger.LogMessage(String.Format("ContractDetails request {0}: {1}", reqId.ToString(), contractName));

            // Because contract is bound in Viewmodel, it must be created on the UI thread.
            // Also, because thread operations
            UIThread.Update(() =>
            {
                var contractKey = contractDetails.MarketName + "_" + contractDetails.ValidExchanges;
                GooContract currentContract;

                if (model.Contracts.ContainsKey(contractKey) == false)
                {
                    currentContract = new GooContract();
                    currentContract.Name = contractDetails.LongName;
                    currentContract.Symbol = contractDetails.MarketName;
                    model.Contracts.Add(contractKey, currentContract);
                    vm.Contracts.Add(currentContract);
                }
            });
        }
        #endregion TWS Event Handlers
    }
}
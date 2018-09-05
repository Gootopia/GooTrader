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
        /// Open a connection to TWS platform
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

        /// <summary>
        /// Operations to perform once TWS connection is acquired.
        /// Typically this is assumed to occur once a "nextValidID" event is triggered from TWS
        /// </summary>
        public void TWS_Connected()
        {
            MessageLogger.LogMessage("Requesting TWS Time");
            ib.ClientSocket.reqCurrentTime();
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
            // Access on viewmodel is prohibited outside UI thread unless using Dispatcher
            UIThread.Update(() => vm.IsTwsConnected = ib.ClientSocket.IsConnected());

            // perform any actions needed after a connection has occurred.
            TWS_Connected();
        }

        // TWS has finished with all details for ContractDetailsRequest (all expirations, etc.)
        private void Ib_ContractDetailsEnd(int reqId)
        {
            MessageLogger.LogMessage(String.Format("ContractDetails request {0} completed", reqId.ToString()));
        }

        // TWS response for a single instance of contractDetails for a given request
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

                    // TWS returns contracts in calendar order (front month first).
                    // First month is normally highest volume, so we'll use that for now
                    currentContract.Expiration = contractDetails.ContractMonth;

                    // Add this contract information to the model as well as the view model
                    model.Contracts.Add(contractKey, currentContract);
                    vm.Contracts.Add(currentContract);
                }
            });
        }

        // TWS response with request for server time. Used to get local offset
        private void Ib_CurrentTime(long time)
        {
            var twsTime = new DateTime(1970, 1, 1);
            twsTime = twsTime.AddSeconds(time).ToLocalTime();
            var localTime = DateTime.Now;

            model.ServerTimeOffset = twsTime - localTime;      

            var msg = String.Format("Current TWS Server Time: {0}. Local: {1}, Difference {2}ms", 
                twsTime.ToLongTimeString(), localTime.ToLongTimeString(), model.ServerTimeOffset.TotalMilliseconds.ToString());
            MessageLogger.LogMessage(msg);
        }
        #endregion TWS Event Handlers
    }
}
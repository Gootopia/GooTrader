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
                ib.ClientSocket.eDisconnect();

            MessageLogger.LogMessage(errMsg);
        }

        // Valid ID signal from TWS implies TWS is ready to recieve commands
        private void Ib_NextValidId(int id)
        {
            ib.NextOrderId = id;
            // Access on viewmodel is prohibited outside UI thread unless using Dispatcher!
            UIThread.Update(() => viewmodel.IsTwsConnected = true);

            // nextValidId event means TWS is ready to Go!
            ib.ClientSocket.IsConnected();
        }
        #endregion TWS Event Handlers
    }
}
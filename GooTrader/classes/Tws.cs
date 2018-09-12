using IBApi;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace IBSampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Convenience classes for TWS Contract Properties
        /// </summary>
        #region TWS Information Classes
        // Defines the various exchanges.
        public static class Exchanges
        {
            public static string Globex = "GLOBEX";
            //TODO: Add GetPrimaryExchange(ticker) to pick the right exchange for a given symbol as there may be multiple
        }

        // Allowed security types (STK, FUT, etc.)
        public static class SecType
        {
            public static string Future = "FUT";
        }

        // Data types for Top market data (Level 1)
        public static class TickType
        {
            public static string Last = "Last";
            public static string BidAsk = "BidAsk";
        }
        #endregion

        #region TWS Methods
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
        /// Request full contract details based on some initial specifications.
        /// Enough info should be provided so that only a single instrument type (FUT,Stock) will be returned.
        /// </summary>
        public void TWS_RequestContractDetails(string symbol, string sectype, string primaryExchange)
        {
            IBApi.Contract requestContract = new Contract();
            requestContract.Symbol = symbol;
            requestContract.SecType = sectype;
            requestContract.Exchange = primaryExchange;
            ib.ClientSocket.reqContractDetails(TWS_GetOrderId(), requestContract);
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

        /// <summary>
        /// Convenience function which automatically updates order ID.
        /// For a single client, connection, it is sufficient to simply increment after each use.
        /// For multiple clients, a more elaborate system is needed. See 10.3.1.1 of API 973.07
        /// </summary>
        /// <returns></returns>
        public int TWS_GetOrderId()
        {
            ib.NextOrderId++;
            return ib.NextOrderId;
        }

        /// <summary>
        /// Convenience wrapper to simply return the current order id without auto-increment.
        /// </summary>
        /// <returns></returns>
        public int TWS_GetCurrentOrderId()
        {
            return ib.NextOrderId;
        }

        /// <summary>
        /// Generate a unique key based on contract information
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string TWS_ContractKey(Contract c)
        {
            string contractkey = string.Empty;

            if(c != null)
            {
                contractkey = String.Format("{0}_{1}_{2}", c.SecType,c.Symbol,c.Exchange);
            }
            return contractkey;
        }

        /// <summary>
        /// Request tick market data (Level I bid/ask/last)
        /// </summary>
        /// <param name="cd"></param>
        public void TWS_RequestTickData(Contract c)
        {
            int id_last = TWS_GetOrderId();
            int id_bidask = TWS_GetOrderId();
            string contractKey = TWS_ContractKey(c);
            model.DataRequests.Add(id_last, contractKey);
            model.DataRequests.Add(id_bidask, contractKey);
            ib.ClientSocket.reqTickByTickData(id_last, c, TickType.Last, 0, false);
            ib.ClientSocket.reqTickByTickData(id_bidask, c, TickType.BidAsk, 0, false);
        }

        #endregion TWS Methods

        #region TWS Event Handlers
        private void Ib_ConnectionClosed()
        {
            MessageLogger.LogMessage("Connection Lost!");
        }

        // Valid ID signal from TWS implies TWS is ready to recieve commands
        private void Ib_NextValidId(messages.ConnectionStatusMessage obj)
        {
            ib.NextOrderId = 0;

            vm.IsTwsConnected = ib.ClientSocket.IsConnected();

            // perform any actions needed after a connection has occurred.
            TWS_Connected();
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

        // TWS has finished with all details for ContractDetailsRequest (all expirations, etc.)
        private void Ib_ContractDetailsEnd(int reqId)
        {
            MessageLogger.LogMessage(String.Format("ContractDetails request {0} completed", reqId.ToString()));
        }

        // TWS response for a single instance of contractDetails for a given request
        private void Ib_ContractDetails(messages.ContractDetailsMessage msg_cd)
        {
            ContractDetails cd = msg_cd.ContractDetails;
            string contractKey = TWS_ContractKey(cd.Contract);

            MessageLogger.LogMessage(String.Format("ContractDetails request {0}: {1}", msg_cd.RequestId.ToString(), contractKey));

            // Contract is created only for first expiration received.
            // TODO: Need to check this when front month is near expiration as it may not be the highest volume contract
            if (model.Contracts.ContainsKey(contractKey) == false)
            {
                var currentContract = new GooContract();
                currentContract.TWSContractDetails = cd;

                // TWS returns contracts in calendar order (front month first).
                // First month is normally highest volume, so we'll use that for now
                currentContract.Expiration = cd.ContractMonth;
                currentContract.Name = cd.LongName;
                currentContract.Symbol = cd.MarketName;

                // Add this contract information to the model as well as the viewmodel
                model.Contracts.Add(contractKey, currentContract);
                vm.Contracts.Add(currentContract);

                // submit request for tick bid/ask/last data for this contract
                TWS_RequestTickData(cd.Contract);
            }
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

        // TWS tick data message for ReqTickByTickData(): Bid/Ask prices
        private void Ib_tickByTickBidAsk(messages.TickByTickBidAskMessage bidask)
        {
            string contractkey = model.DataRequests[bidask.ReqId];
            GooContract c = model.Contracts[contractkey];
            c.Bid = bidask.BidPrice;
            c.Ask = bidask.AskPrice;
        }

        // TWS tick data message for ReqTickByTickData(). Last price
        private void Ib_tickByTickAllLast(messages.TickByTickAllLastMessage last)
        {
            string contractkey = model.DataRequests[last.ReqId];
            GooContract c = model.Contracts[contractkey];
            c.Last = last.Price;
        }
        #endregion TWS Event Handlers
    }
}
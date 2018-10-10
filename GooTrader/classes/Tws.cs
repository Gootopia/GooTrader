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
            TWS_AddContractRequest(id_last, c);
            TWS_AddContractRequest(id_bidask, c);
            ib.ClientSocket.reqTickByTickData(id_last, c, TWSInfo.TWS_TickType.Last, 0, false);
            ib.ClientSocket.reqTickByTickData(id_bidask, c, TWSInfo.TWS_TickType.BidAsk, 0, false);
        }

        /// <summary>
        /// Submit request for historical data
        /// </summary>
        /// <param name="c"></param>
        public void TWS_RequestHistoricalData(Contract c)
        {
            int id_historical = TWS_GetOrderId();
            TWS_AddContractRequest(id_historical, c);
            
            // Find out how much data is available for given contract. Once we know that, we can submit for data in "chunks" (due to TWS data limits)
            ib.ClientSocket.reqHeadTimestamp(id_historical, c, TWSInfo.TWS_WhatToShow.Trades , TWSInfo.TWS_UseRTHOnly.No, TWSInfo.TWS_FormatDate.Standard);
        }

        // Associate a contract with a particular data request
        private void TWS_AddContractRequest(int reqId, Contract c)
        {
            string contractKey = TWS_ContractKey(c);

            // Need to add this request so we can look up what contract is related to the reqId when we receive the events
            if(model.DataRequests.ContainsKey(reqId) == false)
            {
                model.DataRequests.Add(reqId, contractKey);
            } else
            {
                // reqID are unique so it shouldn't be here. If so, we need to investigate further.
                throw new NotImplementedException();
            }
        }

        // Remove the association between a contract and the data request
        private void TWS_DeleteContractRequest(int reqId)
        {
            model.DataRequests.Remove(reqId);
        }

        // Get the contract associated with a given request (if any)
        private GooContract TWS_GetDataRequestContract(int reqId)
        {
            string contractKey = model.DataRequests[reqId];
            GooContract c = model.Contracts[contractKey];
            return c;
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

            MessageLogger.LogMessage(String.Format("ContractDetails request {0}: {1}", msg_cd.RequestId.ToString(), TWS_ContractKey(cd.Contract)));

            string contractKey = TWS_ContractKey(cd.Contract);

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
                // Also submit a request for historical data
                TWS_RequestHistoricalData(cd.Contract);
            }

            //
            TWS_DeleteContractRequest(msg_cd.RequestId);
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

        // TWS message response to real-time data: Bid/Ask update
        private void Ib_tickByTickBidAsk(messages.TickByTickBidAskMessage bidask)
        {
            GooContract c = TWS_GetDataRequestContract(bidask.ReqId);
            c.Bid = bidask.BidPrice;
            c.Ask = bidask.AskPrice;
        }

        // TWS message response to real-time data: Last update
        private void Ib_tickByTickAllLast(messages.TickByTickAllLastMessage last)
        {
            GooContract c = TWS_GetDataRequestContract(last.ReqId);
            c.Last = last.Price;
        }

        // TWS message response to request for how much data is available. Returns timestamp of furthest out data.
        private void Ib_HeadTimestamp(messages.HeadTimestampMessage headTimeStamp)
        {
            GooContract c = TWS_GetDataRequestContract(headTimeStamp.ReqId);
            c.HeadTimeStampString = headTimeStamp.HeadTimestamp;
                        
            // Delete the request associated with the timestamp. We'll create others for each chunk of historical data
            TWS_DeleteContractRequest(headTimeStamp.ReqId);
            
            int histDataReqId = TWS_GetOrderId();

            // Start requesting historical data 1 day at a time. We'll go until we hit the head time stamp.
            c.HistDataRequestDateTime = DateTime.Now;
            var startStr = c.HistDataRequestDateTime.ToString(TWSInfo.TWS_TimeStampFormat);
            // Add a new data request
            TWS_AddContractRequest(histDataReqId, c.TWSContractDetails.Contract);

            // Submit initial request for 1-min historical data. Subsequent requests will come from HistoricalData events until all data is obtained.
            ib.ClientSocket.reqHistoricalData(histDataReqId, c.TWSContractDetails.Contract, startStr,
                TWSInfo.TWS_StepSizes.Day_1, TWSInfo.TWS_BarSizeSetting.Min_1, TWSInfo.TWS_WhatToShow.Trades, 0, 1, false, null);
        }
        #endregion TWS Event Handlers
    }
}
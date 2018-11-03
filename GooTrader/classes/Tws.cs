using IBApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace IBSampleApp
{
    // Singleton TWS class
    public class TWS
    {
        // Offset between local and TWS time
        public static TimeSpan ServerTimeOffset;

        // Reader for passing message between this app and TWS host
        private readonly static EReaderMonitorSignal signal = new EReaderMonitorSignal();
        
        // ib client for interaction with TWS
        private readonly static IBClient ibclient;

        // Data request lookup. Used for indexing tick data streams
        // Stores which contractkey (TWS_ContractKey) for a given tick data reqId since that is what is returned in the tick events
        // This is a two-step process:
        // 1) Index the contract key given the data request Id
        // 2) Index the contract given the contract key
        private static Dictionary<int, string> datarequests = new Dictionary<int, string>();

        #region Constructor
        static TWS()
        {
            ibclient = new IBClient(signal);

            ibclient.NextValidId += Ibclient_NextValidId;
            ibclient.ConnectionClosed += Ibclient_ConnectionClosed;
            ibclient.Error += Ibclient_Error;

            ibclient.ContractDetails += Ibclient_ContractDetails;
            ibclient.ContractDetailsEnd += Ibclient_ContractDetailsEnd;
            ibclient.CurrentTime += Ibclient_CurrentTime;
            ibclient.tickByTickAllLast += Ibclient_tickByTickAllLast;
            ibclient.tickByTickBidAsk += Ibclient_tickByTickBidAsk;
            ibclient.HeadTimestamp += Ibclient_HeadTimestamp;
            ibclient.HistoricalData += Ibclient_HistoricalData;
            ibclient.HistoricalDataEnd += Ibclient_HistoricalDataEnd;
            ibclient.HistoricalDataUpdate += Ibclient_HistoricalDataUpdate;
        }
        #endregion

        #region TWS Event Handlers
        // Received an updated historical data packet
        private static void Ibclient_HistoricalDataUpdate(messages.HistoricalDataMessage obj)
        {
            throw new NotImplementedException();
        }

        // Received final packet of data for the duration requested in the current historical data request.
        private static void Ibclient_HistoricalDataEnd(messages.HistoricalDataEndMessage hDataEnd)
        {
            GooContract c = GetDataRequestContract(hDataEnd.RequestId, true);
        }

        // Received packet of historical data from TWS
        private static void Ibclient_HistoricalData(messages.HistoricalDataMessage hdata)
        {
            var msg = String.Format("Time={0},Open={1},High={2},Low={3},Close={4}", hdata.Date, hdata.Open, hdata.High, hdata.Low, hdata.Close);
            MessageLogger.LogMessage(msg);
        }

        // TWS message response to request for how much data is available. Returns timestamp of furthest out data.
        private static void Ibclient_HeadTimestamp(messages.HeadTimestampMessage headTimeStamp)
        {
            GooContract c = GetDataRequestContract(headTimeStamp.ReqId, true);
            c.HeadTimeStampString = headTimeStamp.HeadTimestamp;

            int histDataReqId = GetOrderId();

            // Start requesting historical data 1 day at a time. We'll go until we hit the head time stamp.
            c.HistDataRequestDateTime = DateTime.Now;
            var startStr = c.HistDataRequestDateTime.ToString(TWSInfo.TWS_TimeStampFormat);

            // Add a new data request
            AddContractRequest(histDataReqId, c);

            // Submit initial request for 1-min historical data. Subsequent requests will come from HistoricalData events until all data is obtained.
            ibclient.ClientSocket.reqHistoricalData(histDataReqId, c.TWSContractDetails.Contract, startStr,
                TWSInfo.TWS_StepSizes.Day_1, TWSInfo.TWS_BarSizeSetting.Min_1, TWSInfo.TWS_WhatToShow.Trades, 0, 1, false, null);
        }

        // TWS message response to real-time data: Bid/Ask update
        private static void Ibclient_tickByTickBidAsk(messages.TickByTickBidAskMessage bidask)
        {
            GooContract c = GetDataRequestContract(bidask.ReqId, false);
            c.Bid = bidask.BidPrice;
            c.Ask = bidask.AskPrice;
        }

        // TWS message response to real-time data: Last update
        private static void Ibclient_tickByTickAllLast(messages.TickByTickAllLastMessage last)
        {
            GooContract c = GetDataRequestContract(last.ReqId, false);
            c.Last = last.Price;
        }

        // TWS response with request for server time. Used to get local offset
        private static void Ibclient_CurrentTime(long time)
        {
            var twsTime = new DateTime(1970, 1, 1);
            twsTime = twsTime.AddSeconds(time).ToLocalTime();
            var localTime = DateTime.Now;

            ServerTimeOffset = twsTime - localTime;

            var msg = String.Format("Current TWS Server Time: {0}. Local: {1}, Difference {2}ms",
                twsTime.ToLongTimeString(), localTime.ToLongTimeString(), TWS.ServerTimeOffset.TotalMilliseconds.ToString());
            MessageLogger.LogMessage(msg);
        }

        private static void Ibclient_ContractDetailsEnd(int reqId)
        {
            MessageLogger.LogMessage(String.Format("ContractDetails request {0} completed", reqId.ToString()));
        }

        // TWS response for a single instance of contractDetails for a given request
        private static void Ibclient_ContractDetails(messages.ContractDetailsMessage msg_cd)
        {
            ContractDetails cd = msg_cd.ContractDetails;
            var ib_contract = cd.Contract;
            string contractKey = GetContractKey(ib_contract);
            string logMsg = String.Format("ContractDetails request {0}: {1}", msg_cd.RequestId.ToString(), contractKey);
            MessageLogger.LogMessage(logMsg);
            
            // Contract is created only for first expiration received.
            // TODO: Need to check this when front month is near expiration as it may not be the highest volume contract
            if (Model.Contracts.ContainsKey(contractKey) == false)
            {
                var currentContract = new GooContract();
                currentContract.TWSContractDetails = cd;

                // TWS returns contracts in calendar order (front month first).
                // First month is normally highest volume, so we'll use that for now
                currentContract.Expiration = cd.ContractMonth;
                currentContract.Name = cd.LongName;
                currentContract.Symbol = cd.MarketName;

                // Add this contract information to the model as well as the viewmodel
                Model.Contracts.Add(contractKey, currentContract);
                ViewModel.Contracts.Add(currentContract);

                // submit request for tick bid/ask/last data for this contract. This request should persist indefinitely.
                RequestTickData(currentContract);

                // Also submit a request for historical data
                RequestHistoricalData(currentContract);
            }

            // request has been processed so remove it from pending list.
            DeleteContractRequest(msg_cd.RequestId);
        }

        // Error message handler. Handles both standard error codes as well as exceptions
        private static void Ibclient_Error(int id, int errorCode, string errorMsg, Exception exception)
        {
            // Default message just prints error codes
            var errMsg = String.Format("ID={0},Error={1}:{2}", id.ToString(), errorCode.ToString(), errorMsg);

            // Exception implies stream issues (TWS went away)
            if (exception != null)
            {
                ibclient.ClientSocket.eDisconnect();
                errMsg = "TWS Disconnected!";
            }

            MessageLogger.LogMessage(errMsg);
        }

        private static void Ibclient_ConnectionClosed()
        {
            throw new NotImplementedException();
        }

        private static void Ibclient_NextValidId(messages.ConnectionStatusMessage obj)
        {
            ibclient.NextOrderId = 0;

            // Need to update the view model 
            ViewModel.RaiseTwsConnectionStateChangedEvent(ibclient.ClientSocket.IsConnected());

            Connected();
        }
        #endregion

        #region Private Methods
        // Operations to perform once TWS connection is acquired.
        // Typically this is assumed to occur once a "nextValidID" event is triggered from TWS
        private static void Connected()
        {
            MessageLogger.LogMessage("Requesting TWS Time");
            ibclient.ClientSocket.reqCurrentTime();
        }

        // Connection with TWS client has been closed
        private static void ConnectionClosed()
        {
            MessageLogger.LogMessage("Connection Lost!");
        }

        // Associate a contract with a particular data request
        private static void AddContractRequest(int reqId, GooContract c)
        {
            string contractKey = GetContractKey(c.TWSContractDetails.Contract);

            // Need to add this request so we can look up what contract is related to the reqId when we receive the events
            if (datarequests.ContainsKey(reqId) == false)
            {
                datarequests.Add(reqId, contractKey);
            }
            else
            {
                // reqID are unique so it shouldn't be here. If so, we need to investigate further.
                throw new NotImplementedException();
            }
        }

        // Remove the association between a contract and the data request
        private static void DeleteContractRequest(int reqId)
        {
            datarequests.Remove(reqId);
        }

        // Get the contract associated with a given request (if any)
        private static GooContract GetDataRequestContract(int reqId, bool isSingleUse)
        {
            string contractKey = datarequests[reqId];
            GooContract c = Model.Contracts[contractKey];

            // No need to keep track of this request any more if it was single-use only.
            if (isSingleUse == true)
            {
                DeleteContractRequest(reqId);
            }
            return c;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Open local connection to TWS
        /// </summary>
        public static void Connect()
        {
            try
            {
                // Open a connection to TWS
                ibclient.ClientId = 0;
                ibclient.ClientSocket.eConnect("127.0.0.1", 7497, 0);

                // Start an IB reader thread
                var reader = new EReader(ibclient.ClientSocket, signal);
                reader.Start();

                // background thread to process TWS messages
                new Thread(() => { while (ibclient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
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
        public static void RequestContractDetails(string symbol, string sectype, string primaryExchange)
        {
            IBApi.Contract requestContract = new Contract();
            requestContract.Symbol = symbol;
            requestContract.SecType = sectype;
            requestContract.Exchange = primaryExchange;
            var req_id = GetOrderId();

            // Transmit request for details of all contracts as described above. Info will be returned via TWS events.
            ibclient.ClientSocket.reqContractDetails(req_id, requestContract);
        }

        /// <summary>
        /// Convenience function which automatically updates order ID.
        /// For a single client, connection, it is sufficient to simply increment after each use.
        /// For multiple clients, a more elaborate system is needed. See 10.3.1.1 of API 973.07
        /// </summary>
        /// <returns></returns>
        public static int GetOrderId()
        {
            ibclient.NextOrderId++;
            return ibclient.NextOrderId;
        }

        /// <summary>
        /// Convenience wrapper to simply return the current order id without auto-increment.
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentOrderId()
        {
            return ibclient.NextOrderId;
        }

        /// <summary>
        /// Generate a unique key based on IB contract information
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string GetContractKey(Contract c)
        {
            string contractkey = string.Empty;
           
            if (c != null)
            {
                contractkey = String.Format("{0}_{1}_{2}", c.SecType, c.Symbol, c.Exchange);
            }
            return contractkey;
        }

        /// <summary>
        /// Request tick market data (Level I bid/ask/last)
        /// </summary>
        /// <param name="cd"></param>
        public static void RequestTickData(GooContract c)
        {
            int id_last = GetOrderId();
            int id_bidask = GetOrderId();
            var ib_contract = c.TWSContractDetails.Contract;
            string contractKey = GetContractKey(ib_contract);
            AddContractRequest(id_last, c);
            AddContractRequest(id_bidask, c);
            ibclient.ClientSocket.reqTickByTickData(id_last, ib_contract, TWSInfo.TWS_TickType.Last, 0, false);
            ibclient.ClientSocket.reqTickByTickData(id_bidask, ib_contract, TWSInfo.TWS_TickType.BidAsk, 0, false);
        }

        /// <summary>
        /// Submit request for historical data
        /// </summary>
        /// <param name="c"></param>
        public static void RequestHistoricalData(GooContract c)
        {
            int id_historical = GetOrderId();
            AddContractRequest(id_historical, c);
            var ib_contract = c.TWSContractDetails.Contract;

            //TODO: Need to hide actual state methods and make them private (still need to check if this works)
            // Find out how much data is available for given contract. Once we know that, we can submit for data in "chunks" (due to TWS data limits)
            ibclient.ClientSocket.reqHeadTimestamp(id_historical, ib_contract, TWSInfo.TWS_WhatToShow.Trades, TWSInfo.TWS_UseRTHOnly.No, TWSInfo.TWS_FormatDate.Standard);
        }
        #endregion
    }
}
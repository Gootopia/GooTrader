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
    public partial class TWS
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
        
        // Used to maintain an internal catalog of all available contracts.
        private static Dictionary<string, GooContract> contracts = new Dictionary<string, GooContract>();

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

        /// <summary>
        /// Get a new request id and associate a contract with it.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>new request id</returns>
        private static int AddContractRequest(GooContract c)
        {
            int reqId = GetOrderId();
            AddContractRequest(reqId, c);

            // Return in case it is needed for use in a TWS call.
            return reqId;
        }

        /// <summary>
        /// Associate contract with a specifc request id
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="c"></param>
        private static void AddContractRequest(int reqId, GooContract c)
        {
            string contractKey = GetContractKey(c.TWSContractDetails.Contract);
            AddContractRequest(reqId, contractKey);
        }

        /// <summary>
        /// Associate contract with specific request id.
        /// Use if you have contract key and not the GooContract
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="contractKey"></param>
        private static void AddContractRequest(int reqId, string contractKey)
        {
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

        // Remove the association between a contract and the data request when it is no longer needed
        private static void DeleteContractRequest(int reqId)
        {
            datarequests.Remove(reqId);
        }

        // Get the contract associated with a given TWS equestId
        private static GooContract GetDataRequestContract(int reqId, bool deleteAfterUse)
        {
            string contractKey = datarequests[reqId];
            GooContract c = contracts[contractKey];

            // No need to keep track of this request any more if it was single-use only.
            if (deleteAfterUse == true)
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
            string contractKey = string.Empty;
           
            if (c != null)
            {
                contractKey = GetContractKey(c.SecType, c.Symbol, c.Exchange);
            }
            return contractKey;
        }

        /// <summary>
        /// Generate contract key based on specific data
        /// </summary>
        /// <param name="secType"></param>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public static string GetContractKey(string secType, string symbol, string exchange)
        {
            string contractKey = string.Empty;

            contractKey = String.Format("{0}_{1}_{2}", secType, symbol, exchange);
            return contractKey;
        }

        /// <summary>
        /// Request full contract details based on some initial specifications.
        /// Enough info should be provided so that only a single instrument type (FUT,Stock) will be returned.
        /// </summary>
        public static GooContract GetContractDetails(string symbol, string sectype, string primaryExchange)
        {
            // This is a throw-away contract used for requesting information. TWS will return ones that are fully populated
            IBApi.Contract requestContract = new Contract();
            requestContract.Symbol = symbol;
            requestContract.SecType = sectype;
            requestContract.Exchange = primaryExchange;
            var reqId = GetOrderId();

            // New blank GooContract. TWS ContractDetails event will populate it.
            GooContract c = new GooContract();
            string contractKey = GetContractKey(requestContract);

            // Internal list
            if (contracts.ContainsKey(contractKey) == false)
            {
                contracts.Add(contractKey, c);
            }

            // Transmit request for details of all contracts as described above. Info will be returned via TWS events.
            AddContractRequest(reqId, contractKey);
            ibclient.ClientSocket.reqContractDetails(reqId, requestContract);

            // We can look it up with the contract key as well.
            return c;
        }

        /// <summary>
        /// Download all available historical data for a given contract from TWS server
        /// </summary>
        /// <param name="c"></param>
        public static void GetHistoricalData(GooContract c)
        {
            // Historical data is retrieved using a state machine
            c.FSM.DownloadHistoricalData.Start(c);
        }

        /// <summary>
        /// Wrapper to request tick market data (Level I bid/ask/last)
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
        /// Wrapper to request historical data starting point for a given contract
        /// </summary>
        /// <param name="c"></param>
        public static void RequestHeadTimeStamp(GooContract c)
        {
            int id_historical = GetOrderId();
            AddContractRequest(id_historical, c);

            var ib_contract = c.TWSContractDetails.Contract;
            ibclient.ClientSocket.reqHeadTimestamp(id_historical, ib_contract, TWSInfo.TWS_WhatToShow.Trades, TWSInfo.TWS_UseRTHOnly.No, TWSInfo.TWS_FormatDate.Standard);
        }

        /// <summary>
        /// Wrapper to submit request for historical data
        /// </summary>
        /// <param name="c"></param>
        public static void RequestHistoricalData(GooContract c)
        {
            // Get a new order id for historical data request
            int histDataReqId = AddContractRequest(c);
            
            // Submit initial request for 1-min historical data. Subsequent requests will come from HistoricalData events until all data is obtained.
            //ibclient.ClientSocket.reqHistoricalData(histDataReqId, c.TWSContractDetails.Contract, startStr,
            //    TWSInfo.TWS_StepSizes.Day_1, TWSInfo.TWS_BarSizeSetting.Min_1, TWSInfo.TWS_WhatToShow.Trades, 0, 1, false, null);
        }
        #endregion
    }
}
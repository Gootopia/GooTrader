using IBApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace IBSampleApp
{
    // TWS event handlers
    public partial class TWS
    {
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
            // request has been processed for all months (if needed) so remove it from pending list.
            DeleteContractRequest(reqId);

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
            }
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

        public static event Action<bool> OnConnectionStatusChanged;
        private static void Ibclient_NextValidId(messages.ConnectionStatusMessage obj)
        {
            ibclient.NextOrderId = 0;

            // Notify outside world that TWS is now connected
            OnConnectionStatusChanged?.Invoke(ibclient.ClientSocket.IsConnected());

            Connected();
        }
        #endregion
    }
}
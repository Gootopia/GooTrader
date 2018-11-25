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
        private static void Ibclient_HistoricalData(messages.HistoricalDataMessage hData)
        {
            GooContract c = GetDataRequestContract(hData.RequestId, true);
        }

        // TWS message response to request for how much data is available. Returns timestamp of furthest out data.
        private static void Ibclient_HeadTimestamp(messages.HeadTimestampMessage headTimeStamp)
        {
            GooContract c = GetDataRequestContract(headTimeStamp.ReqId, true);
            DeleteContractRequest(headTimeStamp.ReqId);

            // Head time stamp is only used for processing download data
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HeadTimeStamp, c);
            //c.HeadTimeStampString = headTimeStamp.HeadTimestamp;

            //int histDataReqId = GetOrderId();

            // Start requesting historical data 1 day at a time. We'll go until we hit the head time stamp.
            //c.HistDataRequestDateTime = DateTime.Now;
            //var startStr = c.HistDataRequestDateTime.ToString(TWSInfo.TWS_TimeStampFormat);

            // Add a new data request
            //AddContractRequest(histDataReqId, c);

        }

        // TWS message response to real-time data: Bid/Ask update
        private static void Ibclient_tickByTickBidAsk(messages.TickByTickBidAskMessage bidask)
        {
            // This request id will be active for as long as the data subscription is valid
            GooContract c = GetDataRequestContract(bidask.ReqId, false);
            c.Bid = bidask.BidPrice;
            c.Ask = bidask.AskPrice;
        }

        // TWS message response to real-time data: Last update
        private static void Ibclient_tickByTickAllLast(messages.TickByTickAllLastMessage last)
        {
            // This request id will be active as long as the data subscription is valid 
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

        // Event to inform outside world a new contract is created
        public static event Action<string, GooContract> OnNewContract;

        // TWS has replied with all available contract details for a given request Id
        private static void Ibclient_ContractDetailsEnd(int reqId)
        {
            GooContract c = GetDataRequestContract(reqId, true);
            
            // TODO: Active month is front month. Near expiration this may not be the best.
            c.TWSContractDetails = c.TWSContractDetailsList[0];

            // Some details we want to break out for the UI
            c.Expiration = c.TWSContractDetails.ContractMonth;
            c.Name = c.TWSContractDetails.LongName;
            c.Symbol = c.TWSContractDetails.MarketName;

            string contractKey = GetContractKey(c.TWSContractDetails.Contract);
            
            // Notify outside world that a new contract has been created.
            OnNewContract?.Invoke(contractKey, c);

            MessageLogger.LogMessage(String.Format("ContractDetails request {0} completed", reqId.ToString()));
        }

        // TWS response for a single instance of contractDetails for a given request
        private static void Ibclient_ContractDetails(messages.ContractDetailsMessage msg_cd)
        {
            ContractDetails cd = msg_cd.ContractDetails;
            string contractKey = GetContractKey(cd.Contract);
            GooContract c = GetDataRequestContract(msg_cd.RequestId, false);

            // TWS returns contracts in calendar order (front month first) so we preserve the order.
            c.TWSContractDetailsList.Add(cd);

            MessageLogger.LogMessage(String.Format("ContractDetails request {0}: {1}", msg_cd.RequestId.ToString(), contractKey));
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
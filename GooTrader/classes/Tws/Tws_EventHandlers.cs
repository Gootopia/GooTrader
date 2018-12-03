using IBApi;
using System;

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
            // Data request is now completed = delete
            GooContract c = GetDataRequestContract(hDataEnd.RequestId, true);

            FSM_EventArgs e = new FSM_EventArgs(c);
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HistoricalDataEnd, e);
        }

        // Received packet of historical data from TWS
        private static void Ibclient_HistoricalData(messages.HistoricalDataMessage hData)
        {
            // Data request is valid until HistoricalDataEnd is received.
            GooContract c = GetDataRequestContract(hData.RequestId, false);

            // Package the quote and send the data received event to the state machine.
            OHLCQuote quote = new OHLCQuote(hData.Date, hData.Open, hData.High, hData.Low, hData.Close);
            FSM_EventArgs e = new FSM_EventArgs(c, quote);
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HistoricalData, e);
        }

        // TWS message response to request for how much data is available. Returns timestamp of furthest out data.
        private static void Ibclient_HeadTimestamp(messages.HeadTimestampMessage headTimeStamp)
        {
            // Get contract. This request is finished, so don't need any more.
            GooContract c = GetDataRequestContract(headTimeStamp.ReqId, true);
            
            // Send event to the download FSM to tell it the head time stamp has been received from TWS.
            FSM_EventArgs e = new FSM_EventArgs(c, headTimeStamp.HeadTimestamp);
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HeadTimeStamp, e);
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

        // Event to inform outside world a new contract is created (after receiving ContractDetails from TWS)
        public static event Action<string, GooContract> OnNewContract;

        // TWS has replied with all available contract details for a given request Id
        private static void Ibclient_ContractDetailsEnd(int reqId)
        {
            // Details end means this request is done, so delete after it is accessed.
            GooContract c = GetDataRequestContract(reqId, true);
            
            // TODO: Active month is front month. Near expiration this may not be the best.
            c.TWSActiveContractDetails = c.TWSContractDetailsList[0];

            // Some details we want to break out for the UI
            c.Expiration = c.TWSActiveContractDetails.ContractMonth;
            c.Name = c.TWSActiveContractDetails.LongName;
            c.Symbol = c.TWSActiveContractDetails.MarketName;

            string contractKey = GetContractKey(c.TWSActiveContractDetails.Contract);
            
            // Notify outside world that a new contract has been created.
            OnNewContract?.Invoke(contractKey, c);

            MessageLogger.LogMessage(String.Format("ContractDetails request {0} completed", reqId.ToString()));
        }

        // TWS response for a single instance of contractDetails for a given request
        private static void Ibclient_ContractDetails(messages.ContractDetailsMessage msg_cd)
        {
            ContractDetails cd = msg_cd.ContractDetails;
            string contractKey = GetContractKey(cd.Contract);

            // Keep the request active until end is signaled by ContractDetailsEnd event.
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

        // TWS connection closed
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
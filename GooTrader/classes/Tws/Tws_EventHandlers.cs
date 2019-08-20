using IBApi;
using System;

namespace IBSampleApp
{
    // TWS event handlers
    public partial class TWS
    {
        // Historical data processing
        #region Historical Data Event Handling
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

            var e = new FSM_EventArgs.GooContract_With_Payload(c);
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HistoricalDataEnd, e);
        }

        // Received packet of historical data from TWS
        private static void Ibclient_HistoricalData(messages.HistoricalDataMessage hData)
        {
            // Data request is valid until HistoricalDataEnd is received.
            GooContract c = GetDataRequestContract(hData.RequestId, false);

            // Package the quote and send the data received event to the state machine.
            OHLCQuote quote = new OHLCQuote(hData.Date, hData.Open, hData.High, hData.Low, hData.Close);
            var e = new FSM_EventArgs.GooContract_With_Payload(c, quote);
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HistoricalData, e);
        }

        // TWS message response to request for how much data is available. Returns timestamp of furthest out data.
        private static void Ibclient_HeadTimestamp(messages.HeadTimestampMessage headTimeStamp)
        {
            // Get contract. This request is finished, so don't need any more.
            GooContract c = GetDataRequestContract(headTimeStamp.ReqId, true);
            
            // Send event to the download FSM to tell it the head time stamp has been received from TWS.
            var e = new FSM_EventArgs.GooContract_With_Payload(c, headTimeStamp.HeadTimestamp);
            c.FSM.DownloadHistoricalData.FireEvent(FSM_DownloadHistoricalData.Events.HeadTimeStamp, e);
        }
        #endregion Historical Data Event Handling

        // Real-time data
        #region Live Data
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
        #endregion Live Data

        // All stuff on information about contracts and instrument definitions
        #region Contract Details
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
            
            // Notify outside world that a new contract has been created and pass along the contractkey and GooContract.
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
        #endregion Contract Details

        #region Error Handling
        // Error message handler. Handles both standard error codes as well as exceptions
        private static void Ibclient_Error(int id, int errorCode, string errorMsg, Exception exception)
        {
            // Bookeeping behind the scenes so we can debug later on
            #region TWS Error Logging
            // Default message just prints error codes
            var logErrorMsg = errorMsg;

            // All message info may only be within the exception
            if(errorMsg == null)
            {
                logErrorMsg = "";
            }

            // If an exception was generated, it will provide additional information.
            if(exception != null)
            {
                logErrorMsg = String.Format("{0}. EXCEPTION: {1}", logErrorMsg, exception.Message);
            }

            logErrorMsg = String.Format("ID={0},Error={1}:{2}", id.ToString(), errorCode.ToString(), logErrorMsg);

            MessageLogger.LogMessage(logErrorMsg);
            #endregion TWS Error Logging

            switch(errorCode)
            {
                #region 0=Connection Error
                case 0:
                    TWS.FSM.Connection.FireEvent(FSM_TwsConnectivity.Events.TWS_Error_0);
                    break;
                #endregion 0=Connection Error

                default:
                    break;

            }
        }
        #endregion Error Handling

        // Order Status, execution, etc.
        #region Order Handling
        #endregion Order Handling

        #region Miscellaneous
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

        // TWS response following successful connection of client to server
        private static void Ibclient_NextValidId(messages.ConnectionStatusMessage obj)
        {
            ibclient.NextOrderId = 0;

            // Per the TWS API documentation (see Connectivity section), receiving this event means TWS has completed the connection
            Connected();
        }

        // TWS connection closed
        private static void Ibclient_ConnectionClosed()
        {
            //throw new NotImplementedException();
        }
        #endregion Miscellaneous
    }
}
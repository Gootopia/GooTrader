using System;
using IBApi;
using System.Collections.Generic;
using System.Threading;

namespace IBSampleApp
{
    // These are TWS wrapper functions for calling various client requests. Can be used by FSM as needed.
    public partial class TWS
    {
        /// <summary>
        /// Open local connection to TWS
        /// </summary>
        public static void Connect()
        {
            // Connection method per the TWS example application
            try
            {
                // Open a connection to TWS
                ibclient.ClientId = 0;
                // Connection on the local machine
                ibclient.ClientSocket.eConnect("127.0.0.1", 7497, 0);

                // Start an IB reader thread
                var reader = new EReader(ibclient.ClientSocket, signal);
                reader.Start();

                // background thread to process TWS messages and pass them back to the reader.
                new Thread(() => { while (ibclient.ClientSocket.IsConnected()) { signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
            }
            catch (Exception)
            {
                // TODO: Not quite sure when we get here. May need to add hierarchy FSM as these could possibly occur in any state.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Break connection to TWS. Can be called for both intentional and unintentional disconnections.
        /// </summary>
        public static void Disconnect()
        {
            ibclient.ClientSocket.Close();
            bool status = ConnectionStatusChanged();

            MessageLogger.LogMessage("Disconnected");
        }

        /// <summary>
        /// Connection made to TWS.
        /// </summary>
        public static void Connected()
        {
            bool status = ConnectionStatusChanged();
            
            // Always sync server time after connection. We'll do this behind the scenes unlike most TWS events as it just clutters the state machine
            ibclient.ClientSocket.reqCurrentTime();
            MessageLogger.LogMessage("Requesting TWS Time");
        }

        /// <summary>
        /// Wrapper to request tick market data (Level I bid/ask/last)
        /// </summary>
        /// <param name="cd"></param>
        public static void RequestTickData(GooContract c)
        {
            int id_last = GetOrderId();
            int id_bidask = GetOrderId();
            var ib_contract = c.TWSActiveContractDetails.Contract;
            string contractKey = GetContractKey(ib_contract);
            AddContractRequest(id_last, c);
            AddContractRequest(id_bidask, c);
            ibclient.ClientSocket.reqTickByTickData(id_last, ib_contract, TWSInfo.TickType.Last, 0, false);
            ibclient.ClientSocket.reqTickByTickData(id_bidask, ib_contract, TWSInfo.TickType.BidAsk, 0, false);
        }

        /// <summary>
        /// Wrapper to request historical data starting point for a given contract
        /// </summary>
        /// <param name="c"></param>
        public static void RequestHeadTimeStamp(GooContract c)
        {
            int id_historical = GetOrderId();
            AddContractRequest(id_historical, c);

            var ib_contract = c.TWSActiveContractDetails.Contract;
            ibclient.ClientSocket.reqHeadTimestamp(id_historical, ib_contract, TWSInfo.WhatTicksToShow.Trades, TWSInfo.UseRealTimeHoursOnly.No, TWSInfo.TimeStampType.Standard);
        }

        /// <summary>
        /// Wrapper to request historical data. Can be used from FSM
        /// </summary>
        /// <param name="c"></param>
        /// <param name="downloadDate">start date (DateTime)</param>
        /// <param name="twsinfo_stepsize">Duration (TWSInfo.TWS_StepSizes</param>
        /// <param name="twsinfo_barsize">Data bar size (TWSInfo.TWS_BarSizeSetting)</param>
        public static void RequestHistoricalData(GooContract c, DateTime downloadDate, string twsinfo_stepsize, string twsinfo_barsize)
        {
            // Get a new order id for historical data request
            int histDataReqId = AddContractRequest(c);

            // TWS requires date to be in a specific format, so must convert from DateTime
            var startStr = downloadDate.ToString(TWSInfo.TimeStampStringFormat);

            // Submit request to tws based on the desired download criteria for the active contract
            ibclient.ClientSocket.reqHistoricalData(histDataReqId, c.TWSActiveContractDetails.Contract, startStr,
                twsinfo_stepsize, twsinfo_barsize, TWSInfo.WhatTicksToShow.Trades, 0, 1, false, null);
        }
    }
}

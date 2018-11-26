namespace IBSampleApp
{
    // Static classes with a bunch of TWS related strings used for client requests.
    public static class TWSInfo
    {
        /// <summary>
        /// Convenience classes for TWS Contract Properties
        /// </summary>
        #region TWS Information Classes
        // Defines the various exchanges.
        public static class TWS_Exchanges
        {
            public static string Globex = "GLOBEX";
            //TODO: Add GetPrimaryExchange(ticker) to pick the right exchange for a given symbol as there may be multiple
        }

        // Allowed security types (STK, FUT, etc.)
        public static class TWS_SecType
        {
            public static string Future = "FUT";
        }

        // Data types for Top market data (Level 1)
        public static class TWS_TickType
        {
            public static string Last = "Last";
            public static string BidAsk = "BidAsk";
        }

        // Real-Time Hours data only
        public static class TWS_UseRTHOnly
        {
            public static int No = 0;
            public static int Yes = 1;
        }

        // Types of price data
        public static class TWS_WhatToShow
        {
            public static string Bid = "BID";
            public static string Ask = "ASK";
            public static string BidAsk = "BID_ASK";
            public static string Trades = "TRADES";
        }

        // Historical data bar size
        public static class TWS_BarSizeSetting
        {
            public static string Min_1 = "1 min";
            public static string Min_2 = "2 mins";
            public static string Min_3 = "3 mins";
            public static string Min_5 = "5 mins";
            public static string Min_10 = "10 mins";
            public static string Min_15 = "15 mins";
            public static string Min_20 = "20 mins";
            public static string Min_30 = "30 mins";
            public static string Hr_1 = "1 hour";
            public static string Hr_2 = "2 hours";
            public static string Hr_3 = "3 hours";
            public static string Hr_4 = "4 hours";
            public static string Hr_8 = "8 hours";
            public static string Day = "1 day";
            public static string Week = "1 week";
            public static string Month = "1 month";
        }

        // Historical data step sizes
        public static class TWS_StepSizes
        {
            public static string Sec_60 = "60 S";
            public static string Sec_120 = "120 S";
            public static string Min_30 = "1800 S";
            public static string Hr_1 = "3600 S";
            public static string Hr_4 = "14400 S";
            public static string Hr_8 = "28800 S";
            public static string Day_1 = "1 D";
            public static string Day_2 = "2 D";
            public static string Week = "1 W";
            public static string Month = "1 M";
            public static string Year = "1 Y";
        }

        // Price Data Timestamp format
        public static class TWS_FormatDate
        {
            // Format is "yyyyMMdd HH:mm:ss"
            public static int Standard = 1;
            // Elapsed seconds? (Probably won't ever use).
            public static int Seconds = 2;
        }

        // Note that TWS timestamp strings have TWO spaces between day and hour!
        public static string TWS_TimeStampFormat = "yyyyMMdd  HH:mm:ss";
    }
    #endregion
}
using System;
using System.Collections.Specialized;

// Use default namespace so we don't have to rename stuff when updating to a new TWS API
namespace IBSampleApp
{
    // Collection of OHLC bars
    public class OHLCData
    {
        public string BarSize;
        public ListDictionary Data = new ListDictionary();
    }

    // Single OHLC bar
    public class OHLCQuote
    {
        public string Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public OHLCQuote(string date, double open, double high, double low, double close)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }
    }
}

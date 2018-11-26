using System;

// Use default namespace so we don't have to rename stuff when updating to a new TWS API
namespace IBSampleApp
{
    // Quote data packet
    public class OHLCData
    {
        public string Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public OHLCData(string date, double open, double high, double low, double close)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }
    }
}

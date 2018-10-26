using System;

// Use default namespace so we don't have to rename stuff when updating to a new TWS API
namespace IBSampleApp
{
    public class OHLCData
    {
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public OHLCData(double open, double high, double low, double close)
        {

        }
    }
}

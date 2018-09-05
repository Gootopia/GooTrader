using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GooTrader
{

    /// <summary>
    /// Used to format the SystemTime clock in the UI
    /// </summary>
    public class SystemTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dt = (DateTime)value;
            var timeStr = dt.ToLongTimeString();

            // Year = 1 if we are not connected to TWS
            if (dt.Year == 1)
                timeStr = String.Format("{0} (Local)", DateTime.Now.ToLongTimeString());
            else
                timeStr = String.Format("{0} (TWS)", dt.ToLongTimeString());

            return timeStr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GooTrader
{
    /// <summary>
    /// Message Logger
    /// Used to log events with a timestamp
    /// </summary>
    public class MessageLogger : ObservableCollection<Tuple<String,String>>
    {
        // Constructor
        public MessageLogger()
        {
            this.Add(new Tuple<string, string>("Time1", "Message1"));
            this.Add(new Tuple<string, string>("Time2", "Message2"));
        }

        #region private Methods
        private string _getCurrentTime()
        {
            return "00:00:00";
        }
        #endregion

        #region public Methods
        /// <summary>
        /// LogMessage(string msg)
        /// Timestamps a message with the current time and inserts it at the head of the message log
        /// </summary>
        /// <param name="msg"></param>
        public void LogMessage(string msg)
        {
            var newLogEntry = Tuple.Create<string, string>(_getCurrentTime(), msg);
            this.Insert(0, newLogEntry);
        }

        /// <summary>
        /// ClearLog()
        /// Empties the message log
        /// </summary>
        public void ClearLog()
        {

        }
        #endregion
    }
}

using System;
using System.Collections.ObjectModel;

namespace GooTrader
{
    public enum LogMessageType { IB, ORDER, INFO };

    public class LogMessage
    {
        public string Time { get; set; }
        public LogMessageType Type{ get; set; }
        public string Message { get; set; }

        public LogMessage(string msg, LogMessageType type)
        {
            Time = DateTime.Now.ToLongTimeString();
            Type = type;
            Message = msg;
        }
    }

    /// <summary>
    /// Message Logger
    /// Used to log events with a timestamp
    /// </summary>
    public class MessageLogger : ObservableCollection<LogMessage>
    {
        #region public Methods
        /// <summary>
        /// LogMessage(string msg)
        /// Timestamps a message with the current time and inserts it at the head of the message log
        /// </summary>
        /// <param name="msg"></param>
        public void LogMessage(string msg, LogMessageType type = LogMessageType.INFO)
        {
            var newLogEntry = new LogMessage(msg, type);
            this.Insert(0, newLogEntry);
        }

        #endregion
    }
}

using System;
using System.Collections.ObjectModel;

namespace IBSampleApp
{
    /// <summary>
    /// Log4net logging
    /// </summary>
    public class Loggers
    {
        // log4net loggers. See App.config for configuration parameters
        public static readonly log4net.ILog log = LogHelper.GetLogger();
    }

    public enum LogMessageType { IB, ORDER, MISC };

    public enum LogMessageLevel {UI_ONLY, INFO, WARN, ERROR, DEBUG}

    /// <summary>
    /// Log Message
    /// Individual messages which are passed to the MessageLogger
    /// </summary>
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
    /// Used to log events in the UI window with a timestamp
    /// </summary>
    static public class MessageLogger
    {
        static public ObservableCollection<LogMessage> messages;
        #region public Methods
        /// <summary>
        /// LogMessage(string msg)
        /// Timestamps a message with the current time and inserts it at the head of the message log
        /// Can be called from any thread
        /// </summary>
        /// <param name="msg">Message string to log</param>
        /// <param name="type">see LogMessageType [INFO]</param>
        /// <param name="isLogToFile">true=>write to log file [true]</param>
        /// <param name="level">see LogMessageLevel [INFO]</param>
        static public void LogMessage(string msg,
            LogMessageType type = LogMessageType.MISC,
            bool isLogToFile = true, 
            LogMessageLevel level= LogMessageLevel.INFO)
        {
            var newLogEntry = new LogMessage(msg, type);
            if(messages == null) throw new NullReferenceException();

            if(isLogToFile == true)
            {
                string logMsgWithType = String.Format("[{0}]-{1}", type.ToString(), msg);
                switch(level)
                {
                    case LogMessageLevel.DEBUG:
                        Loggers.log.Debug(logMsgWithType);
                        break;
                    case LogMessageLevel.ERROR:
                        Loggers.log.Error(logMsgWithType);
                        break;
                    case LogMessageLevel.WARN:
                        Loggers.log.Warn(logMsgWithType);
                        break;
                    default:
                        Loggers.log.Info(logMsgWithType);
                        break;
                }
            }
            
            // Insert message at the head of the log so it shows up at the top.
            // messages.Insert(0, newLogEntry);
            // Can be called from any thread
            UIThread.Update(() => { messages.Insert(0, newLogEntry); });
        }

        #endregion
    }
}

using System;
using Logging.Interfaces;
using NLog;

namespace EfCfRepoCoverTests.Logging
{
    public class NLogger : ILogging
    {
        #region Constants
        private const string LOG_CLASS_NAME = "Logger";
        #endregion  // End Constants

        #region Private Fields
        private static volatile NLogger _instance;                    // 'Instance' variable (to expose the Singleton instance).
        private static readonly object SyncRoot = new object();    // 'Lock' object (to lock code while checking to see if 'Instance' is null, so multiple threads can't see '== null' inaccurately).
        private static readonly NLog.Logger NLogging;
        #endregion  // End Private Fields

        #region Properties
        public static NLogger Instance
        {
            get
            {
                if (_instance == null)
                {                // Determine if 'Instance' has been created yet (for the 1st and only time).
                    lock (SyncRoot)
                    {                  // Lock 'null check' and 'instantiate' code so no other thread will try to create at the same time.
                        if (_instance == null)
                        {        // Now that code area is locked, check again to see if 'Instance' has been created yet.
                            _instance = new NLogger();   // Create the 'singleton' instance. 
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion  // End Properties

        #region Constructor(s)
        // Private constructor called by the static 'Instance' property (for 'singleton' functionality).
        static NLogger()
        {
            NLogging = LogManager.GetLogger(LOG_CLASS_NAME);
            
            _isDebugEnabled = true;
            _isErrorEnabled = true;
            _isFatalEnabled = true;
            _isInfoEnabled = true;
            _isTraceEnabled = true;
            _isWarnEnabled = true;
        }
        #endregion // End Constructor(s)
        
        #region ILogging Implementation

        #region ILogging Property Backing Fields
        private static bool _isDebugEnabled { get; set; }
        private static bool _isErrorEnabled { get; set; }
        private static bool _isFatalEnabled { get; set; }
        private static bool _isInfoEnabled { get; set; }
        private static bool _isTraceEnabled { get; set; }
        private static bool _isWarnEnabled { get; set; }
        #endregion ILogging Property Backing Fields

        #region ILogging Properties
        public bool IsDebugEnabled { get { return _isDebugEnabled; } set { _isDebugEnabled = value; } }

        public bool IsErrorEnabled { get { return _isErrorEnabled; } set { _isErrorEnabled = value; } }

        public bool IsFatalEnabled { get { return _isFatalEnabled; } set { _isFatalEnabled = value; } }

        public bool IsInfoEnabled { get { return _isInfoEnabled; } set { _isInfoEnabled = value; } }

        public bool IsTraceEnabled { get { return _isTraceEnabled; } set { _isTraceEnabled = value; } }

        public bool IsWarnEnabled { get { return _isWarnEnabled; } set { _isWarnEnabled = value; } }
        #endregion ILogging Properties

        #region Trace (ILogging)
        public void Trace(object message)
        {
            if (this.IsTraceEnabled)
            {
                NLogging.Trace(message);
            }
        }

        public void Trace(object message, Exception exception)
        {
            if (this.IsTraceEnabled)
            {
                NLogging.Trace(message);
                NLogging.Trace(exception);
            }
        }

        public void TraceFormat(string format, params object[] args)
        {
            if (this.IsTraceEnabled)
            {
                var logMsgFormat = string.Format(format, args);
                NLogging.Trace(logMsgFormat);
            }
        }

        public void TraceFormat(string format, Exception exception, params object[] args)
        {
            //throw new NotImplementedException();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            //throw new NotImplementedException();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            //throw new NotImplementedException();
        }
        #endregion Trace (ILogging)

        #region Debug (ILogging)
        public void Debug(object message)
        {
            if (this.IsDebugEnabled)
            {
                NLogging.Debug(message);                
            }
        }

        public void Debug(object message, Exception exception)
        {
            if (this.IsDebugEnabled)
            {
                NLogging.Debug(message);
                NLogging.Debug(exception);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                var logMsgFormat = string.Format(format, args);
                NLogging.Debug(logMsgFormat);
            }
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            // throw new NotImplementedException();
        }
        #endregion Debug (ILogging)

        #region Info (ILogging)
        public void Info(object message)
        {
            if (this.IsInfoEnabled)
            {
                NLogging.Info(message);
            }
        }

        public void Info(object message, Exception exception)
        {
            NLogging.Info(message);
            NLogging.Info(exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                var messageText = string.Format(format, args);
                NLogging.Info(messageText);
            }
        }
        #endregion Info (ILogging)

        #region Warn (ILogging)
        public void Warn(object message)
        {
            if (this.IsWarnEnabled)
            {
                NLogging.Warn(message);
            }
        }

        public void Warn(object message, Exception exception)
        {
            NLogging.Warn(message);
            NLogging.Warn(exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                var message = string.Format(format, args);
                var messageText = string.Format(format, message);
                NLogging.Warn(messageText);
            }
        }
        #endregion Warn (ILogging)

        #region Error (Ilogging)
        public void Error(object message)
        {
            if (this.IsErrorEnabled)
            {
                NLogging.Error(message);
            }
        }

        public void Error(object message, Exception exception)
        {
            if (this.IsErrorEnabled)
            {
                NLogging.Error(message);
                NLogging.Error(exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                var messageText = string.Format(format, args);
                NLogging.Error(messageText);
            }
        }
        #endregion Error (Ilogging)

        #region Fatal (ILogging)
        public void Fatal(object message)
        {
            if (this.IsFatalEnabled)
            {
                NLogging.Fatal(message);
            }
        }

        public void Fatal(object message, Exception exception)
        {
            if (this.IsFatalEnabled)
            {
                NLogging.Fatal(message);
                NLogging.Fatal(exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                var messageText = string.Format(format, args);
                NLogging.Fatal(messageText);
            }
        }
        #endregion Fatal (ILogging)
        #endregion ILogging Implementation Methods
    }
}

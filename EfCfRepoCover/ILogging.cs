using System;

namespace Logging.Interfaces
{
    public interface ILogging
    {
        // Similar to ILog from Common.Logging (http://netcommon.sourceforge.net/docs/2.1.0/reference/html/ch01.html#logging-usage)
        // High-Level methods: Trace, Debug, Info, Warn, Error, Fatal

        #region Trace Methods
        void Trace(object message);
        void Trace(object message, Exception exception);
        //void Trace(FormatMessageCallback formatMessageCallback);
        //void Trace(FormatMessageCallback formatMessageCallback, Exception exception);
        //void Trace(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback);
        //void Trace(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback, Exception exception);
        void TraceFormat(string format, params object[] args);
        void TraceFormat(string format, Exception exception, params object[] args);
        //void TraceFormat(IFormatProvider formatProvider, string format, params object[] args);
        //void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        #endregion Trace Methods

        #region Debug Methods
        void Debug(object message);
        void Debug(object message, Exception exception);
        //void Debug(FormatMessageCallback formatMessageCallback);
        //void Debug(FormatMessageCallback formatMessageCallback, Exception exception);
        //void Debug(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback);
        //void Debug(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback, Exception exception);
        void DebugFormat(string format, params object[] args);
        //void DebugFormat(string format, Exception exception, params object[] args);
        //void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);
        //void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        #endregion Debug Methods

        #region Info Methods
        void Info(object message);
        void Info(object message, Exception exception);
        //void Info(FormatMessageCallback formatMessageCallback);
        //void Info(FormatMessageCallback formatMessageCallback, Exception exception);
        //void Info(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback);
        //void Info(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback, Exception exception);
        void InfoFormat(string format, params object[] args);
        //void InfoFormat(string format, Exception exception, params object[] args);
        //void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);
        //void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        #endregion Info Methods

        #region Warn Methods
        void Warn(object message);
        void Warn(object message, Exception exception);
        //void Warn(FormatMessageCallback formatMessageCallback);
        //void Warn(FormatMessageCallback formatMessageCallback, Exception exception);
        //void Warn(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback);
        //void Warn(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback, Exception exception);
        void WarnFormat(string format, params object[] args);
        //void WarnFormat(string format, Exception exception, params object[] args);
        //void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);
        //void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        #endregion Warn Methods

        #region Error Methods
        void Error(object message);
        void Error(object message, Exception exception);
        //void Error(FormatMessageCallback formatMessageCallback);
        //void Error(FormatMessageCallback formatMessageCallback, Exception exception);
        //void Error(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback);
        //void Error(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback, Exception exception);
        void ErrorFormat(string format, params object[] args);
        //void ErrorFormat(string format, Exception exception, params object[] args);
        //void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);
        //void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        #endregion Error Methods

        #region Fatal Methods
        void Fatal(object message);
        void Fatal(object message, Exception exception);
        //void Fatal(FormatMessageCallback formatMessageCallback);
        //void Fatal(FormatMessageCallback formatMessageCallback, Exception exception);
        //void Fatal(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback);
        //void Fatal(IFormatProvider formatProvider, FormatMessageCallback formatMessageCallback, Exception exception);
        void FatalFormat(string format, params object[] args);
        //void FatalFormat(string format, Exception exception, params object[] args);
        //void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);
        //void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args);
        #endregion Fatal Methods

        #region Properties
        bool IsDebugEnabled { get; }

        bool IsErrorEnabled { get; }

        bool IsFatalEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsTraceEnabled { get; }

        bool IsWarnEnabled { get; }
        #endregion Properties
    }
}

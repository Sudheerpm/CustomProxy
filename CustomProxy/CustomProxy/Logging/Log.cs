using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace CustomProxy.Logging
{
    public class Log<T> : ILog<T>
    {
        public void LogCritical(Exception ex, string title)
        {
            LogError(ex,TraceEventType.Critical, title, "", "");
        }

        public void LogStart(string message)
        {
            LogMessage("Application Start", message, TraceEventType.Start, "", "", false);
        }

        public void LogStop(string message)
        {
            LogMessage("Application Stopped", message, TraceEventType.Stop, "", "", false);
        }

        public void LogWarning(string title, string verbose)
        {
            LogMessage(title, verbose, TraceEventType.Information, "Information", "", false);
        }

        public void LogVerbose(string title, string verbose)
        {
            LogMessage(title, verbose, TraceEventType.Information, "Information", "", false);
        }

        public void LogInformation(string title, string message)
        {
            LogMessage(title, message, TraceEventType.Information, "Information", "", false);
        }

        private void LogMessage(string title, string message, TraceEventType type, string category, string parameters, Boolean listStackTrace)
        {
            var log = new LogEntry();
            log.Message = message;
            log.Severity = type;
            log.Priority = 2;
            log.TimeStamp = DateTime.Now;
            log.Categories.Add(category);
            log.ProcessName = typeof(T).Name;
            log.ProcessId = Convert.ToString(System.Diagnostics.Process.GetCurrentProcess().Id);
            log.Title = string.IsNullOrEmpty(title) ? string.Empty : title;
            if (parameters != string.Empty)
                log.ExtendedProperties.Add("Parameters", parameters);
            Logger.Write(log);
        }
        public void LogError(Exception ex, string title)
        {
            LogError(ex, TraceEventType.Error , title, "", string.Empty);
        }
        private void LogError(Exception ex, TraceEventType type, string title, string category, string parameters)
        {
            title = string.IsNullOrEmpty(title) ? string.Empty : title;
            var logEntry = new LogEntry();
            logEntry.Title = title;
            logEntry.Message = ex.Message + "\n" + ex.StackTrace;
            logEntry.Severity = type;
            logEntry.TimeStamp = DateTime.Now;
            logEntry.Priority = 1;
            logEntry.Categories.Add(category);
            logEntry.ProcessName = typeof(T).Name;
            logEntry.ProcessId = Convert.ToString(System.Diagnostics.Process.GetCurrentProcess().Id);
            if (parameters != string.Empty)
                logEntry.ExtendedProperties.Add("Parameters", parameters);

            Logger.Write(logEntry);
        }
    }
}
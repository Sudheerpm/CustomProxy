using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CustomProxy.Logging
{
    public interface ILog<T>
    {
        void LogCritical(Exception ex, string title);
        void LogError(Exception ex, string title);
        void LogWarning(string title, string verbose);
        void LogInformation(string title, string message);
        void LogVerbose(string title, string message);
        void LogStart(string message);
        void LogStop(string message);
    }
}

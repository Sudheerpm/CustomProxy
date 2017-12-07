using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomProxy.Logging
{
    public class LogManager
    {
        public static ILog<T> GetLogger<T>() where T: class
        {
            return new Log<T>();
        }
    }
}
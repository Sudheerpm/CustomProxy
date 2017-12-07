using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using CustomProxy.Logging;

namespace CustomProxy
{
    public class Global : System.Web.HttpApplication
    {
        private ILog<Global> logger = LogManager.GetLogger<Global>();
        protected void Application_Start(object sender, EventArgs e)
        {
            logger.LogStart("Custom Proxy application has started at " + DateTime.Now.ToLongDateString());
        }

        protected void Application_End(object sender, EventArgs e)
        {
            logger.LogStop("Custom Proxy application has been terminated at " + DateTime.Now.ToLongDateString());
        }
    }
}
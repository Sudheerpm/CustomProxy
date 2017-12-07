using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CustomProxy.CustomExceptions
{
    public class BaseProxyHttpException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public BaseProxyHttpException(string message) : base(message)
        {

        }
    }
}
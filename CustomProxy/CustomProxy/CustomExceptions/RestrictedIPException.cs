using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomProxy.CustomExceptions
{
    public class RestrictedIPException : BaseProxyHttpException
    {
        public RestrictedIPException(string message) : base(message)
        {
            base.StatusCode = System.Net.HttpStatusCode.Forbidden;
        }
    }
}
using System;


namespace CustomProxy.CustomExceptions
{
    public class ServiceNotMappedException : BaseProxyHttpException
    {
        public ServiceNotMappedException(string message) : base(message)
        {
            base.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable;
        }
    }
}
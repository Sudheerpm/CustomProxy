using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web;
using CustomProxy.Logging;
using CustomProxy.Faults;

namespace CustomProxy.Handler
{
    public class GenericHttpHandler : BaseHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: https://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        NameValueCollection routeSection = ConfigurationManager.GetSection("routes") as NameValueCollection;
        private ILog<GenericHttpHandler> logger;
        public GenericHttpHandler()
        {
            logger = LogManager.GetLogger<GenericHttpHandler>();
        }

        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!string.IsNullOrEmpty(context.Request.Params["key"]))
                {
                    string destUrl = routeSection[context.Request.Params["key"]];
                    if (!string.IsNullOrEmpty(destUrl))
                    {
                        logger.LogInformation("service execution", "The service execution started");
                        var webRequest = WebRequest.Create(destUrl);
                        webRequest.Method = context.Request.HttpMethod;
                        webRequest.UseDefaultCredentials = true;
                        webRequest.ContentType = context.Request.ContentType;
                        //copy all non-restricted headers
                        var nvCollection = new NameValueCollection(context.Request.Headers.Count);
                        logger.LogInformation("service execution", "Populating the data from request header started.");
                        PopulateHeaders(context.Request.Headers, webRequest.Headers);
                        logger.LogInformation("service execution", "Populating the data from request header completed.");
                        logger.LogInformation("service execution", "started gathering data from request stream.");
                        if (webRequest.Method ==  "POST" || webRequest.Method == "PUT")
                        {
                            //copy the request body
                            var outputStream = webRequest.GetRequestStream();
                            var inputStream = context.Request.InputStream;
                            //// Find number of bytes in stream.
                            var inputStreamLen = Convert.ToInt32(inputStream.Length);
                            // Create a byte array.
                            byte[] strArr = new byte[inputStreamLen];
                            // Read stream into byte array
                            Int32 strRead = inputStream.Read(strArr, 0, inputStreamLen);
                            if (strRead == 0)
                            {
                               logger.LogError(new Exception("End of stream reached when copying request content"), "Invalid Input");
                               context.Response.End();
                            }
                            webRequest.GetRequestStream().Write(strArr, 0, strArr.Length);
                            webRequest.GetRequestStream().Close();
                        }
                        logger.LogInformation("service execution", "started gathering data from request stream.");
                        var httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
                        var s = httpWebResponse.GetResponseStream();
                        var sr = new StreamReader(s);
                        var responseFromServer = sr.ReadToEnd();
                        sr.Close();
                        s.Close();
                        httpWebResponse.Close();
                        logger.LogInformation("service execution", "The service execution completed.");
                        context.Response.ContentType = httpWebResponse.ContentType;
                        logger.LogInformation("response processing", "Started response header processing.");
                        var responseCollection = new NameValueCollection(httpWebResponse.Headers.Count);
                        foreach (string hKey in httpWebResponse.Headers.Keys)
                        {
                            if (context.Response.Headers.Get(hKey) == null && !WebHeaderCollection.IsRestricted(hKey))
                            {
                                context.Response.Headers.Add(hKey, httpWebResponse.Headers[hKey]);
                            }
                        }
                        logger.LogInformation("response processing", "Completed response header processing.");
                        context.Response.Write(responseFromServer);
                        logger.LogInformation("response processing", "Response is being written and completed.");
                    }
                }
                else
                {
                    logger.LogInformation("Service not found", "The service you've requested isn't setup.");
                }
            }

            catch (NotSupportedException ex)
            {
                logger.LogCritical(ex, "Incorrect syntax in path or volume labels");
            }
            catch (TimeoutException ex)
            {
                logger.LogError(ex, "Timeout exception occurred");
            }
            
            catch (FaultException<Fault> ex)
            {
                logger.LogCritical(ex, "Errors in request input");
            }
            catch (WebException ex)
            {
                logger.LogCritical(ex, "Web Exception occurred");
            }
            catch (System.Exception ex)
            {
                logger.LogCritical(ex, "Exception occurred");
            }
        }

        #endregion
    }
}
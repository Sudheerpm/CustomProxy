using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web;
using CustomProxy.Logging;
using CustomProxy.Entities;
using System.Collections.Generic;
using CustomProxy.CustomExceptions;
using System.Linq;

namespace CustomProxy.Handler
{
    public class GenericHttpHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: https://go.microsoft.com/?linkid=8101007
        /// </summary>

        private const string SOAP = "SOAP";
        private const string REST = "REST";
        private const string CONFIG_SECTION_ROUTES = "routes";
        private const string CONFIG_SECTION_IGNORE_URLS = "IgnoreUrls";
        private const string CONFIG_SECTION_WHITELISTED_IPS = "WhitelistedIPs";

        NameValueCollection routeSection = ConfigurationManager.GetSection(CONFIG_SECTION_ROUTES) as NameValueCollection;
        NameValueCollection ignoreUrlsSection = ConfigurationManager.GetSection(CONFIG_SECTION_IGNORE_URLS) as NameValueCollection;
        NameValueCollection whitelistedIPs = ConfigurationManager.GetSection(CONFIG_SECTION_WHITELISTED_IPS) as NameValueCollection;
        private ILog<GenericHttpHandler> logger;

        public GenericHttpHandler()
        {
            logger = LogManager.GetLogger<GenericHttpHandler>();
        }

        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpContextBase wrapper = new HttpContextWrapper(context);
            this.ProcessRequestBase(wrapper);
        }

        public void ProcessRequestBase(HttpContextBase contextBase)
        {
            HttpContext context = contextBase.ApplicationInstance.Context;

            try
            {
                string originalUrl = context.Request.Url.AbsoluteUri;
                string newRouteUrl = originalUrl;

                logger.LogInformation("Processing started", $"Incoming request {originalUrl}");

                if (ignoreUrlsSection.AllKeys.Contains(context.Request.Url.AbsoluteUri))
                    return;

                // Do IP filtering
                string incomingIP = GetIPAddress();
                if (!whitelistedIPs.AllKeys.Contains(incomingIP))
                    throw new RestrictedIPException($"IP not whitelisted {incomingIP} | Request {originalUrl}");

                // Parse the url pattern to identify DNS, Source Identifer, Endpoint Identifier and Service URL
                var urlPattern = new UrlPattern(context.Request.Url);

                logger.LogInformation("URL pattern processed", $"DNS =  {urlPattern.DNS} | Source Identifier {urlPattern.SourceIdentifier} | Endpoint Identifier {urlPattern.EndpointIdentifier} | Service URL {urlPattern.ServiceUrl}");

                // Find if any route is configured against the incling request
                var routeConfiguration = FindRouteConfiguration(urlPattern);
                if (routeConfiguration is null)
                    throw new ServiceNotMappedException($"The request has no mapping in the proxy : {originalUrl}");

                // Make appropriate request              
                // Try to pass the same http request on to the new mapped service url
                newRouteUrl = urlPattern.GetRouteDestinationUrl(routeConfiguration.RouteDestination);

                logger.LogInformation("Route translation complete", $"Request received from Incoming endpoint : {originalUrl} | Routed endpoint : {newRouteUrl}");

                var webRequest = CreateRequest(context, newRouteUrl);
                var webResponse = FetchResponse(webRequest);
                if (webResponse != null)
                {
                    ProcessResponse(context, webResponse);
                }

            }
            catch (RestrictedIPException ex)
            {
                HandleException(ex, context);
            }
            catch (ServiceNotMappedException ex)
            {
                HandleException(ex, context);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    // Send response back from the target service
                    ProcessResponse(context, (HttpWebResponse)ex.Response);
                    return;
                }
                HandleException(ex, context);
            }
            catch (ProtocolViolationException ex)
            {
                HandleException(ex, context);
            }
            catch (InvalidOperationException ex)
            {
                HandleException(ex, context);
            }
            catch (Exception ex)
            {
                HandleException(ex, context);
            }
        }

        /// <summary>
        /// Finds the route configured in web config against the endpoint identifier
        /// </summary>
        /// <param name="urlPattern"></param>
        /// <returns></returns>
        private RouteConfigurationElement FindRouteConfiguration(UrlPattern urlPattern)
        {
            // Get custom route configuration
            const string SECTION_NAME = "RoutesSection";
            RouteConfigurationSection routeConfigurationSection = ConfigurationManager.GetSection(SECTION_NAME) as RouteConfigurationSection;
            RouteConfigurationElement mappedRouteConfiguratoinElement = null;

            foreach (RouteConfigurationElement routeConfig in routeConfigurationSection.Routes)
            {
                if (routeConfig.EndpointIdentifier.Trim().ToLower().Equals(urlPattern.EndpointIdentifier.Trim().ToLower()))
                {
                    mappedRouteConfiguratoinElement = routeConfig;
                    break;
                }
            }

            return mappedRouteConfiguratoinElement;
        }

        /// <summary>
        /// Make a SOAP request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="routeConfigurationElement"></param>
        private void MakeSoapRequest(HttpContext context, RouteConfigurationElement routeConfigurationElement)
        {
            logger.LogInformation("SOAP", "Request started");

            var webRequest = WebRequest.Create(routeConfigurationElement.RouteDestination);
            webRequest.Method = context.Request.HttpMethod;
            webRequest.UseDefaultCredentials = true;
            webRequest.ContentType = context.Request.ContentType;

            //copy all non-restricted headers
            var nvCollection = new NameValueCollection(context.Request.Headers.Count);
            logger.LogVerbose("SOAP", "Populating request headers complete.");

            PopulateHeaders(context.Request.Headers, webRequest.Headers);

            logger.LogVerbose("SOAP", "Populating request stream started.");
            if (webRequest.Method == "POST" || webRequest.Method == "PUT")
            {
                //copy the request body
                var outputStream = webRequest.GetRequestStream();
                var inputStream = context.Request.InputStream;
                CopyStream(inputStream, outputStream);
            }
        }

        /// <summary>
        /// Get IP address of the incoming request
        /// By default do not check for HTTP_X_FORWARDED_FOR for security
        /// </summary>
        /// <param name="checkForwardedAddress"></param>
        /// <returns></returns>
        private string GetIPAddress(bool checkForwardedAddress = false)
        {
            string ip = null;

            if (checkForwardedAddress)
            {
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }

            // If HTTP_X_FORWARDED_FOR is not set or the request is not configured to use it
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                // Using last address set in X-Forwarded-For
                ip = ip.Split(',').Last().Trim();
            }

            return ip;
        }

        #endregion

        /// <summary>
        /// Creates request object to be submitted to the back-end service based on the passed context and destination url
        /// </summary>
        /// <param name="context"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        protected WebRequest CreateRequest(HttpContext context, String url)
        {
            logger.LogInformation("Process request", "Request started");
            var webRequest = ((HttpWebRequest)WebRequest.Create(url));
            webRequest.Method = context.Request.HttpMethod;
            webRequest.UseDefaultCredentials = true;
            webRequest.ContentType = context.Request.ContentType;
            //copy all non-restricted headers
            var nvCollection = new NameValueCollection(context.Request.Headers.Count);
            logger.LogVerbose("Process request", "Populating request headers complete.");
            PopulateHeaders(context.Request.Headers, webRequest.Headers);

            // Copy cookies
            foreach (Cookie cookie in context.Request.Cookies)
            {
                webRequest.CookieContainer.Add(cookie);
            }

            logger.LogVerbose("Process request", "Populating request stream started.");
            if (webRequest.Method == "POST" || webRequest.Method == "PUT")
            {
                //copy the request body
                var outputStream = webRequest.GetRequestStream();
                var inputStream = context.Request.InputStream;
                CopyStream(inputStream, outputStream);
            }
            return webRequest;
        }

        /// <summary>
        /// Fetch the response by submitting the request
        /// </summary>
        /// <param name="webRequest"></param>
        /// <returns></returns>
        protected virtual HttpWebResponse FetchResponse(WebRequest webRequest)
        {
            var httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            return httpWebResponse;
        }

        /// <summary>
        /// Process the response received from the back-end service, fetch headers and the HTML/XML response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpWebResponse"></param>
        /// <param name="statusCode"></param>
        private void ProcessResponse(HttpContext context, HttpWebResponse httpWebResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            //populate the HTTP response headers
            logger.LogVerbose("Process Request", "Process response headers.");
            string responseFromServer = String.Empty;


            if (httpWebResponse != null)
            {
                var responseCollection = new NameValueCollection(httpWebResponse.Headers.Count);
                PopulateHeaders(context.Response.Headers, httpWebResponse.Headers);

                //populate the HTTP responses based on service response
                logger.LogVerbose("Process request", "Populating http response.");
                var s = httpWebResponse.GetResponseStream();
                var sr = new StreamReader(s);
                responseFromServer = sr.ReadToEnd();
                sr.Close();
                s.Close();
                httpWebResponse.Close();
                context.Response.ContentType = httpWebResponse.ContentType;
                statusCode = httpWebResponse.StatusCode;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.Write(responseFromServer);
        }

        /// <summary>
        /// Populate response headers
        /// </summary>
        /// <param name="sourceHeaders"></param>
        /// <param name="destinationHeaders"></param>
        private void PopulateHeaders(NameValueCollection sourceHeaders, WebHeaderCollection destinationHeaders)
        {
            foreach (string hKey in sourceHeaders)
            {
                if (destinationHeaders.Get(hKey) == null && !WebHeaderCollection.IsRestricted(hKey))
                {
                    destinationHeaders.Add(hKey, sourceHeaders[hKey]);
                }
            }
        }

        /// <summary>
        /// Copy response stream into the context 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        private void CopyStream(Stream inputStream, Stream outputStream)
        {
            //// Find number of bytes in stream.
            var inputStreamLen = Convert.ToInt32(inputStream.Length);
            // Create a byte array.
            byte[] strArr = new byte[inputStreamLen];
            // Read stream into byte array
            Int32 strRead = inputStream.Read(strArr, 0, inputStreamLen);
            if (strRead == 0)
            {
                // NOTE : Commenting out since it is suppressing nd 
                // throw new Exception("End of stream reached when copying request content");
            }
            outputStream.Write(strArr, 0, strArr.Length);
            outputStream.Close();
        }

        /// <summary>
        /// Log exception and send appropriate response code
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="context"></param>
        protected virtual void HandleException(Exception ex, HttpContext context)
        {
            logger.LogError(ex, ex.GetType().ToString());

            // Use default InternalServerError as default failure code to send back
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            if (ex.GetType().BaseType == typeof(BaseProxyHttpException))
            {
                // If the xception is of the type BaseProxyHttpException, use appropriate status code to send back
                statusCode = ((BaseProxyHttpException)ex).StatusCode;
            }

            ProcessResponse(context, null, statusCode);
        }

    }
}
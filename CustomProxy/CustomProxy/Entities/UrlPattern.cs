using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CustomProxy.Entities
{
    public class UrlPattern
    {
        public UrlPattern(Uri uri)
        {
            int segmentIndexSourceIdentifier = 1;
            int segmentIndexEndpointIdentifier = 2;
            int segmentIndexServiceUrl = 3;

            Uri = uri.AbsoluteUri;
            DNS = uri.DnsSafeHost;

            if(DNS.Equals("localhost"))
            {
                // For testing locally with localhose, additional segment would be present = CustomProxyHandler which will not be there on Production
                segmentIndexSourceIdentifier = 2;
                segmentIndexEndpointIdentifier = 3;
                segmentIndexServiceUrl = 4;
            }

            SourceIdentifier = ReplaceSlash(uri.Segments[segmentIndexSourceIdentifier]);
            EndpointIdentifier = ReplaceSlash(uri.Segments[segmentIndexEndpointIdentifier]);
            
            if(uri.Segments.Count() > segmentIndexServiceUrl)
            {
                string absoulutePath = uri.AbsolutePath;
                Match match = Regex.Matches(absoulutePath, Regex.Escape('/'.ToString()))
                              .Cast<Match>()
                              .Skip(segmentIndexServiceUrl - 1)
                              .FirstOrDefault();

                if(match != null)
                {
                    ServiceUrl = absoulutePath.Substring(match.Index);
                }
            }
        }

        public string Uri { get; }
        public string DNS { get; }
        public string SourceIdentifier { get; }
        public string EndpointIdentifier { get; }
        public string ServiceUrl { get; } = String.Empty;

        private string ReplaceSlash(string segment)
        {
            return segment.Replace("/", String.Empty);
        }

        public string GetRouteDestinationUrl(string routeDestination)
        {
            return $"{routeDestination}{ServiceUrl}";
        }
    }
}
using System;
using CustomProxy.Entities;
using NUnit.Framework;

namespace CustomProxy.Test
{
    [TestFixture]
    public class TestUrlPattern
    {
        [TestCase("https://dnsname.com/source-identifier/endpoint-identifier/service-url", "dnsname.com", "source-identifier", "endpoint-identifier", "/service-url", TestName = "TestIfBasicUrlPatternIsParsedCorrectly", Description = "Test case for generic url pattern")]
        [TestCase("https://dnsname.com/mortgages/PAF/PAF.svc", "dnsname.com", "mortgages", "PAF", "/PAF.svc", TestName = "TestIfSOAPUrlPatternIsParsedCorrectly", Description = "Test case for SOAP url pattern")]
        [TestCase("https://dnsname.com/banking/Person/1", "dnsname.com", "banking", "Person", "/1", TestName = "TestIfRESTUrlPatternIsParsedCorrectly", Description = "Test case for REST url pattern")]
        [TestCase("https://dnsname.com/banking/Person", "dnsname.com", "banking", "Person", "", TestName = "TestIfRESTUrlPatternWithoutResourceIdIsParsedCorrectly", Description = "Test case for REST url pattern")]
        [TestCase("https://localhost/CustomProxyHandler/banking/Person/1", "localhost", "banking", "Person", "/1", TestName = "TestIfRESTUrlPatternIsParsedCorrectlyWithLocalhost", Description = "Test case for REST url pattern with localhost")]
        public void TestIfUrlPatternIsParsedCorrectly(string url, string expectedDNS, string expectedSourceIdentifier, string expectedEndpointIndentifier, string expectedServiceUrl)
        {
            var urlPattern = new UrlPattern(new Uri(url));

            Assert.AreEqual(urlPattern.DNS, expectedDNS,"DNS not as expected");
            Assert.AreEqual(urlPattern.SourceIdentifier, expectedSourceIdentifier, "Source Identifier not as expected");
            Assert.AreEqual(urlPattern.EndpointIdentifier, expectedEndpointIndentifier, "Endpoint Identifier not as expected");
            Assert.AreEqual(urlPattern.ServiceUrl, expectedServiceUrl, "Service URL not as expected");
        }

        [Test]
        public void TestIfRouteDestinationIsFormedCorrectly()
        {
            UrlPattern urlPattern = new UrlPattern(new Uri("http://localhost/CustomProxyHandler/mortgages/paf/PAF.svc"));
            string configuredMappedRouteDestination = "http://localhost/nbs-mortgages-stub-paf";
            string expectedRouteDestination = "http://localhost/nbs-mortgages-stub-paf/PAF.svc";
            string actualRouteDestination = urlPattern.GetRouteDestinationUrl(configuredMappedRouteDestination);
            Assert.AreEqual(expectedRouteDestination, actualRouteDestination);
        }
    }
}

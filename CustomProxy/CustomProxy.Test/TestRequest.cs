using CustomProxy.Handler;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CustomProxy.Test
{
    [TestFixture]
    public class TestRequest
    {
        [Test]
        public void TestUnmappedRequest()
        {
            Mock<HttpContextBase> moqAppContext;
            Mock<HttpContextBase> moqContext;
            Mock<HttpRequestBase> moqRequest;

            HttpApplication httpApp = new HttpApplication();


            moqAppContext = new Mock<HttpContextBase>();
            moqContext = new Mock<HttpContextBase>();
            moqRequest = new Mock<HttpRequestBase>();


            moqRequest.SetupGet(x => x.HttpMethod).Returns("GET");
            moqRequest.SetupGet(x => x.Url).Returns(new Uri("http://dns.com/mortgages/unmapped"));


            moqContext.Setup(x => x.Request).Returns(moqRequest.Object);



            GenericHttpHandler genericHttpHandler = new GenericHttpHandler();
            genericHttpHandler.ProcessRequestBase(moqContext.Object);

            int actualStatusCode = moqContext.Object.Response.StatusCode;
            int expectedStatusCode = (int)HttpStatusCode.ServiceUnavailable;

            Assert.AreEqual(expectedStatusCode, actualStatusCode);
        }
    }
}

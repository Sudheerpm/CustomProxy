using CustomProxy.Entities;
using NUnit.Framework;
using System.Configuration;

namespace CustomProxy.Test
{
    [TestFixture]
    public class TestCustomRouteConfiguration
    {
        [Test]
        public void TestIfCustomRoutesConfigurationIsReadCorrectly()
        {
            const string SECTION_NAME = "RoutesSection";
            RouteConfigurationSection sec = ConfigurationManager.GetSection(SECTION_NAME) as RouteConfigurationSection;

            Assert.AreEqual(sec.Routes.Count, 2);
        }

    }
}

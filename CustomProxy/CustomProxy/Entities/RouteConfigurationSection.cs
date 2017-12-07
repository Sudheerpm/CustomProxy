using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CustomProxy.Entities
{
    public class RouteConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("Routes")]
        public RouteCollection Routes
        {
            get { return ((RouteCollection)(base["Routes"])); }
            set { base["Routes"] = value; }
        }
    }

    public class RouteConfigurationElement : ConfigurationElement
    {
        private const string ATT_NAME = "name";
        private const string ATT_SERVICE_TYPE = "serviceType";
        private const string ATT_ENDPOINT_IDENTIFIER = "endpointIdentifier";
        private const string ATT_ROUTE_DESTINATION = "routeDestination";

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base[ATT_NAME]; }
            set { base[ATT_NAME] = value; }
        }

        [ConfigurationProperty(ATT_SERVICE_TYPE, IsRequired = true)]
        public string ServiceType
        {
            get { return (string)base[ATT_SERVICE_TYPE]; }
            set { base[ATT_SERVICE_TYPE] = value; }
        }

        [ConfigurationProperty(ATT_ENDPOINT_IDENTIFIER)]
        public string EndpointIdentifier
        {
            get { return (string)base[ATT_ENDPOINT_IDENTIFIER]; }
            set { base[ATT_ENDPOINT_IDENTIFIER] = value; }
        }

        [ConfigurationProperty(ATT_ROUTE_DESTINATION)]
        public string RouteDestination
        {
            get { return (string)base[ATT_ROUTE_DESTINATION]; }
            set { base[ATT_ROUTE_DESTINATION] = value; }
        }

    }

    [ConfigurationCollection(typeof(RouteConfigurationElement))]
    public class RouteCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "Route";


        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName,
            StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RouteConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RouteConfigurationElement)(element)).Name;
        }

        public RouteConfigurationElement this[int idx]
        {
            get { return (RouteConfigurationElement)BaseGet(idx); }
        }

    }  
}
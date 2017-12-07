using CustomProxy.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml;

namespace CustomProxy.Utilities
{
    public class ServiceRoutesConfigHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {

            return null;
        }
    }
}
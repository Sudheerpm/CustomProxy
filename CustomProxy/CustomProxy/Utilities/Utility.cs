using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace CustomProxy.Utilities
{
    public class Utility
    {
        public static string GetXMLFromObject(object o)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(o.GetType());
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = false;
                settings.OmitXmlDeclaration = false;
                using (StringWriter textWriter = new StringWriterWithEncoding(Encoding.UTF8))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                    {
                        serializer.Serialize(xmlWriter, o);
                    }
                    return textWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                //ex
            }
            finally
            {

            }
            return string.Empty;
        }       
    }
}
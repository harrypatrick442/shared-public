using System;
using System.IO;
using System.Linq;
using System.Xml;
using Core.Exceptions;
namespace Core
{
    public static class XmlHelper
    {
        public static string ToNicelyFormattedString(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return ToNicelyFormattedString(document);
        }
        public static string ToNicelyFormattedString(XmlDocument document)
        {
            using (MemoryStream mStream = new MemoryStream()) {
                using (XmlTextWriter writer = new XmlTextWriter(mStream, System.Text.Encoding.Unicode)) {
                    writer.Formatting = Formatting.Indented;

                    document.WriteContentTo(writer);
                    writer.Flush();
                    mStream.Flush();
                    mStream.Position = 0;
                    StreamReader sReader = new StreamReader(mStream);
                    string formattedXml = sReader.ReadToEnd();
                    return formattedXml;
                }
            }
        }
    }
}
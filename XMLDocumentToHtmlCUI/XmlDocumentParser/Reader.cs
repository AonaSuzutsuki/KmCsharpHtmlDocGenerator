using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace XmlDocumentParser
{
    /// <summary>
    /// Read from a file as XML Document.
    /// </summary>
    public class Reader
    {
        public string XmlPath { get; } = string.Empty;

        private readonly XmlDocument document = new XmlDocument();

        public Reader(string xmlPath, bool isFile = true)
        {
            if (isFile)
            {
                XmlPath = xmlPath;
                document.Load(xmlPath);
            }
            else
            {
                document.LoadXml(xmlPath);
            }
        }
        public Reader(Stream stream)
        {
            document.Load(stream);
        }

        public List<string> GetAttributes(string attribute, string xpath, bool isContainNoValue = false)
        {
            var values = new List<string>();

            // /items/item/property/property[@name='DegradationMax']
            var nodeList = document.SelectNodes(xpath);
            foreach (var xmlNode in nodeList)
            {
                var attr = (xmlNode as XmlElement).GetAttribute(attribute);
                if (isContainNoValue)
                {
                    values.Add(attr);
                }
                else
                {
                    if (!string.IsNullOrEmpty(attr))
                        values.Add(attr);
                }
            }

            return values;
        }
        public string GetAttribute(string attribute, string xpath)
        {
            var attributes = GetAttributes(attribute, xpath);
            return attributes.Count < 1 ? string.Empty : attributes[0];
        }

        public List<string> GetValues(string xpath, bool enableLineBreak = true)
        {
            var values = new List<string>();

            var nodeList = document.SelectNodes(xpath);
            foreach (var xmlNode in nodeList)
            {
                string value = (xmlNode as XmlElement).InnerText;
                value = RemoveSpace(value, enableLineBreak);
                values.Add(value);
            }

            return values;
        }
        public string GetValue(string xpath, bool enableLineBreak = true)
        {
            var values = GetValues(xpath, enableLineBreak);
            return values.Count < 1 ? default : values[0];
        }

        private static string RemoveSpace(string text, bool isAddLine = false)
        {
            var sb = new StringBuilder();

            const string expression = "^ *(?<text>.*)$";
            var reg = new Regex(expression);
            var sr = new StringReader(text);
            while (sr.Peek() > -1)
            {
                var match = reg.Match(sr.ReadLine());
                if (match.Success)
                {
                    if (isAddLine)
                        sb.AppendLine(match.Groups["text"].Value);
                    else
                        sb.Append(match.Groups["text"].Value);
                }
                else
                {
                    sb.Append(sr.ReadLine());
                }
            }

            return sb.ToString();
        }
    }
}

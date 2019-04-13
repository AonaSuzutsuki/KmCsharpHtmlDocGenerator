using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace XmlDocumentParser.XmlWrapper
{
    /// <summary>
    /// Read from a file as XML Document.
    /// </summary>
    public class Reader
    {

        private readonly XmlDocument document = new XmlDocument();
        private XmlNamespaceManager xmlNamespaceManager;

        /// <summary>
        /// Initialize xml document from xml text
        /// </summary>
        /// <param name="xmlText">Xml text.</param>
        public void LoadFromText(string xmlText)
        {
            document.LoadXml(xmlText);
            xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
        }

        /// <summary>
        /// Initialize xml document from Stream.
        /// </summary>
        /// <param name="stream">Loaded stream</param>
        public void LoadFromStream(Stream stream)
        {
            document.Load(stream);
            xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
        }

        /// <summary>
        /// Initialize xml document from xml file.
        /// </summary>
        /// <param name="filepath">Xml file path.</param>
        public void LoadFromFile(string filepath)
        {
            document.Load(filepath);
            xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
        }

        /// <summary>
        /// Adds the namespace.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <param name="uri">URI.</param>
        public void AddNamespace(string prefix, string uri)
        {
            xmlNamespaceManager.AddNamespace(prefix, uri);
        }

        /// <summary>
        /// Get specified attributes.
        /// </summary>
        /// <param name="attributeName">Name of the attribute to retrieve.</param>
        /// <param name="xpath">XPath of the attribute to retrieve.</param>
        /// <returns>Specified attributes.</returns>
        public List<string> GetAttributes(string attributeName, string xpath)
        {
            var values = new List<string>();

            // /items/item/property/property[@name='DegradationMax']
            var nodeList = document.SelectNodes(xpath, xmlNamespaceManager);
            foreach (var xmlNode in nodeList)
            {
                var attr = (xmlNode as XmlElement).GetAttribute(attributeName);

                if (!string.IsNullOrEmpty(attr))
                    values.Add(attr);
            }

            return values;
        }

        /// <summary>
        /// Get a specified attribute.
        /// </summary>
        /// <param name="attribute">Name of the attribute to retrieve.</param>
        /// <param name="xpath">XPath of the attribute to retrieve.</param>
        /// <returns>A specified attribute.</returns>
        public string GetAttribute(string attribute, string xpath)
        {
            var attributes = GetAttributes(attribute, xpath);
            return attributes.Count < 1 ? string.Empty : attributes[0];
        }

        /// <summary>
        /// Get specified values.
        /// </summary>
        /// <param name="xpath">XPath of the attribute to retrieve.</param>
        /// <param name="enableLineBreak">Whether to add a linebreak.</param>
        /// <returns>Specified values.</returns>
        public List<string> GetValues(string xpath, bool enableLineBreak = true)
        {
            var values = new List<string>();

            var nodeList = document.SelectNodes(xpath, xmlNamespaceManager);
            foreach (var xmlNode in nodeList)
            {
                string value = (xmlNode as XmlElement).InnerXml;
                value = RemoveSpace(value, enableLineBreak);
                values.Add(value);
            }

            return values;
        }

        /// <summary>
        /// Get a specified value.
        /// </summary>
        /// <param name="xpath">XPath of the attribute to retrieve</param>
        /// <param name="enableLineBreak">Whether to add a linebreak.</param>
        /// <returns>A specified value.</returns>
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

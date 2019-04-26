using System.IO;
using System.Xml;

namespace XmlDocumentParser.XmlWrapper
{
    /// <summary>
    /// Write to a file as XML Document.
    /// </summary>
    public class Writer
    {
        private XmlDocument xDocument = new XmlDocument();
        private XmlProcessingInstruction xDeclaration;
        private XmlElement xRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:XmlDocumentParser.XmlWrapper.Writer"/> class.
        /// </summary>
        public Writer()
        {
            xDeclaration = xDocument.CreateProcessingInstruction("xml", "version=\"1.0\"");
        }

        /// <summary>
        /// Set a root.
        /// </summary>
        /// <param name="rootName">Root name</param>
        public XmlElement SetRoot(string rootName)
        {
            xRoot = xDocument.CreateElement(rootName);
            //宣言の追加
            xDocument.AppendChild(xDeclaration);
            //ServerSettingsの追加
            xDocument.AppendChild(xRoot);

            return xRoot;
        }

        /// <summary>
        /// Adds the element.
        /// </summary>
        /// <returns>The element.</returns>
        /// <param name="elementName">Element name.</param>
        /// <param name="element">Element.</param>
        public XmlElement AddElement(string elementName, XmlElement element)
        {
            var xmeta = xDocument.CreateElement(elementName);
            element.AppendChild(xmeta);
            return xmeta;
        }

        /// <summary>
        /// Adds the element.
        /// </summary>
        /// <returns>The element.</returns>
        /// <param name="element">Element.</param>
        /// <param name="xml">Xml.</param>
        public XmlElement AddElement(XmlElement element, string xml)
        {
            element.InnerXml = xml;
            return element;
        }

        /// <summary>
        /// Add an element.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="attributeInfos">Attribute informations.</param>
        /// <param name="value">Attribute value.</param>
        public void AddElement(string elementName, AttributeInfo[] attributeInfos, string value = null)
        {
            XmlElement xmeta = xDocument.CreateElement(elementName);
            foreach (AttributeInfo attributeInfo in attributeInfos)
                xmeta.SetAttribute(attributeInfo.Name, attributeInfo.Value);
            if (!string.IsNullOrEmpty(value))
                xmeta.InnerText = value;

            xRoot.AppendChild(xmeta);
        }

        /// <summary>
        /// Add an element.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="attributeInfo">Attribute information.</param>
        /// <param name="value">Attribute value.</param>
        public void AddElement(string elementName, AttributeInfo attributeInfo, string value = null)
        {
            XmlElement xmeta = xDocument.CreateElement(elementName);
            xmeta.SetAttribute(attributeInfo.Name, attributeInfo.Value);
            if (!string.IsNullOrEmpty(value))
                xmeta.InnerText = value;

            xRoot.AppendChild(xmeta);
        }

        /// <summary>
        /// Adds the element.
        /// </summary>
        /// <param name="elementName">Element name.</param>
        /// <param name="value">Value.</param>
        public void AddElement(string elementName, string value = null)
        {
            XmlElement xmeta = xDocument.CreateElement(elementName);
            if (!string.IsNullOrEmpty(value))
                xmeta.InnerText = value;

            xRoot.AppendChild(xmeta);
        }

        /// <summary>
        /// Write to a file as XML Dcoument.
        /// </summary>
        public void Write(string xmlPath)
        {
            FileStream fs = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            Write(fs);
            fs.Dispose();
        }
        /// <summary>
        /// Write to a file as XML Dcoument.
        /// </summary>
        public void Write(Stream stream)
        {
            xDocument.Save(stream);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:XmlDocumentParser.XmlWrapper.Writer"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:XmlDocumentParser.XmlWrapper.Writer"/>.</returns>
        public override string ToString()
        {
            return xDocument.InnerXml;
        }
    }
}
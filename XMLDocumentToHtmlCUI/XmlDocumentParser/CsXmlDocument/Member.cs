using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// It expresses the lowest element such as method and property.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Method type of member.
        /// </summary>
        public MethodType Type { get; set; }

        /// <summary>
        /// Namespace of member.
        /// </summary>
        public NamespaceItem NameSpace { get; set; }

        /// <summary>
        /// Name of member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parameter types of member.
        /// </summary>
        public List<string> MethodParameters { get; set; } = new List<string>();

        /// <summary>
        /// Value of member.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Return comment of member.
        /// </summary>
        public string ReturnComment { get; set; }

        /// <summary>
        /// Parameter names of member.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}

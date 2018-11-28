using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// It expresses tree structure element such as namespace.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Tree structure element type.
        /// </summary>
        public ElementType Type { get; set; }

        /// <summary>
        /// Namespace of element.
        /// </summary>
        public NamespaceItem Namespace { get; set; }

        /// <summary>
        /// Name of element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of element.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// List of namespaces this element holds.
        /// </summary>
        public List<Element> Namespaces { get; set; } = new List<Element>();

        /// <summary>
        /// Items of the lowest element such as method and property.
        /// </summary>
        public List<Member> Members { get; set; } = new List<Member>();

        /// <summary>
        /// Checks if it has the specified element of name.
        /// </summary>
        /// <param name="name">Name of element to check.</param>
        /// <returns>Whether an element was found.</returns>
        public bool HasElement(string name)
        {
            foreach (var elem in Namespaces)
            {
                if (elem.Name.Equals(name))
                    return true;
            }
            return false;
        }
    }
}

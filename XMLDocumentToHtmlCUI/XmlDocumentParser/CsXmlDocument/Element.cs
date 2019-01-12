using CommonCoreLib.Bool;
using Microsoft.CodeAnalysis;
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
    public class Element : IElementOfInheritance
    {
        /// <summary>
        /// Identifier of this element.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Tree structure element type.
        /// </summary>
        public ElementType Type { get; set; }

        /// <summary>
        /// Namespace of element.
        /// </summary>
        public NamespaceItem Namespace { get; set; } = new NamespaceItem();

        /// <summary>
        /// Name of element.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Value of element.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Accessibility of this element. Require to analyze source code.
        /// </summary>
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        /// <summary>
        /// Returns whether it is static or not. Require to analyze source code.
        /// </summary>
        public bool IsStatic { get; set; } = false;

        /// <summary>
        /// Returns whether it is abstract or not. Require to analyze source code.
        /// </summary>
        public bool IsAbstract { get; set; } = false;

        /// <summary>
        /// Returns whether it is abstract or not. Require to analyze source code.
        /// </summary>
        public bool IsSealed { get; set; } = false;

        /// <summary>
        /// List of inheritance. Require to analyze source code.
        /// </summary>
        public List<IElementOfInheritance> InheritanceList { get; set; } = new List<IElementOfInheritance>();

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

        /// <summary>
        /// Object.GetHashCode()
        /// </summary>
        /// <returns>The hash value.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Check the equivalence of this object and the argument object.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <returns>It returns True if equivalent, False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            var element = (Element)obj;
            var boolcollector = new BoolCollector();

            boolcollector.ChangeBool("Accessibility", Accessibility == element.Accessibility);
            boolcollector.ChangeBool("Id", Id.Equals(element.Id));
            boolcollector.ChangeBool("Inheritance", InheritanceList.SequenceEqual(element.InheritanceList));
            boolcollector.ChangeBool("IsAbstract", IsAbstract == element.IsAbstract);
            boolcollector.ChangeBool("IsSealed", IsSealed == element.IsSealed);
            boolcollector.ChangeBool("IsStatic", IsStatic == element.IsStatic);
            boolcollector.ChangeBool("Members", Members.SequenceEqual(element.Members));
            boolcollector.ChangeBool("Name", Name.Equals(element.Name));
            boolcollector.ChangeBool("Namespace", Namespace.Equals(element.Namespace));
            boolcollector.ChangeBool("Namespaces", Namespaces.SequenceEqual(element.Namespaces));
            boolcollector.ChangeBool("Type", Type == element.Type);
            boolcollector.ChangeBool("Value", Value.Equals(element.Value));

            return boolcollector.Value;
        }
    }
}

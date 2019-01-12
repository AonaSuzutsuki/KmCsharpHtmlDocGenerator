using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// <see cref="Element"/> interface for Inheritance.
    /// </summary>
    public interface IElementOfInheritance
    {
        /// <summary>
        /// Accessibility of this element. Require to analyze source code.
        /// </summary>
        Accessibility Accessibility { get; set; }

        /// <summary>
        /// Identifier of this element.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Name of element.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Namespace of element.
        /// </summary>
        NamespaceItem Namespace { get; set; }
    }
}
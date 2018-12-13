using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace XmlDocumentParser.CsXmlDocument
{
    public interface IElementOfInheritance
    {
        Accessibility Accessibility { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        NamespaceItem Namespace { get; set; }
    }
}
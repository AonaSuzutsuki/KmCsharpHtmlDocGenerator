using Microsoft.CodeAnalysis;

namespace XmlDocumentParser.EasyCs
{
    public interface IAccessorInfo
    {
        Accessibility Accessibility { get; set; }
        string Name { get; set; }
    }
}
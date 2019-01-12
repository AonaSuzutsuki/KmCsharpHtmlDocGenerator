using Microsoft.CodeAnalysis;

namespace XmlDocumentParser.EasyCs
{
    /// <summary>
    /// <see cref="ClassInfo"/> interface for property accessor.
    /// </summary>
    public interface IAccessorInfo
    {
        /// <summary>
        /// Accessibility of this element. Require to analyze source code.
        /// </summary>
        Accessibility Accessibility { get; set; }

        /// <summary>
        /// Name of element.
        /// </summary>
        string Name { get; set; }
    }
}
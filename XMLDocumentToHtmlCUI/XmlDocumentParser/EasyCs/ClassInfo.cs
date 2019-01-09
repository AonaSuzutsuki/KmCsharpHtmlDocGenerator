using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using XmlDocumentParser.CsXmlDocument;

namespace XmlDocumentParser.EasyCs
{
	/// <summary>
	/// Class info for <see cref="CSharpEasyAnalyzer"/>.
    /// </summary>
	public class ClassInfo : IAccessorInfo
    {
        /// <summary>
        /// Identifier of this element.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Accessibility of this element. Require to analyze source code.
        /// </summary>
        public Accessibility Accessibility { get; set; }
        public ClassType ClassType { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
		public bool IsExtern { get; set; }
		public bool IsPartial { get; set; }
		public bool IsOverride { get; set; }
		public bool IsVirtual { get; set; }
		public bool IsAsync { get; set; }
		public bool IsExtensionMethod { get; set; }
        public List<IAccessorInfo> Accessors { get; set; } = new List<IAccessorInfo>();
        public string ReturnType { get; set; } = Constants.SystemVoid;
        public string FullName { get; set; }
        public NamespaceItem Namespace { get; set; }
        public string NameWithParameter { get; set; }

        /// <summary>
        /// Name of element.
        /// </summary>
        public string Name { get; set; }
        public List<ClassInfo> Inheritance { get; set; } = new List<ClassInfo>();
    }
}

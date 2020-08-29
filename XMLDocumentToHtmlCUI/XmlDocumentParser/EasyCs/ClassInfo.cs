using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using XmlDocumentParser.CsXmlDocument;

namespace XmlDocumentParser.EasyCs
{

    public class ParameterInfo
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

        public string GetName(bool isFullname)
        {
            return isFullname ? FullName : Name;
        }
    }

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

        /// <summary>
        /// Gets or sets the type of the class.
        /// </summary>
        /// <value>The type of the class.</value>
        public ClassType ClassType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is static.
        /// </summary>
        /// <value><c>true</c> if is static; otherwise, <c>false</c>.</value>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is abstract.
        /// </summary>
        /// <value><c>true</c> if is abstract; otherwise, <c>false</c>.</value>
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is sealed.
        /// </summary>
        /// <value><c>true</c> if is sealed; otherwise, <c>false</c>.</value>
        public bool IsSealed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is extern.
        /// </summary>
        /// <value><c>true</c> if is extern; otherwise, <c>false</c>.</value>
		public bool IsExtern { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is partial.
        /// </summary>
        /// <value><c>true</c> if is partial; otherwise, <c>false</c>.</value>
		public bool IsPartial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is override.
        /// </summary>
        /// <value><c>true</c> if is override; otherwise, <c>false</c>.</value>
		public bool IsOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is virtual.
        /// </summary>
        /// <value><c>true</c> if is virtual; otherwise, <c>false</c>.</value>
		public bool IsVirtual { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is async.
        /// </summary>
        /// <value><c>true</c> if is async; otherwise, <c>false</c>.</value>
		public bool IsAsync { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:XmlDocumentParser.EasyCs.ClassInfo"/> is extension method.
        /// </summary>
        /// <value><c>true</c> if is extension method; otherwise, <c>false</c>.</value>
		public bool IsExtensionMethod { get; set; }

        /// <summary>
        /// Gets or sets the accessors.
        /// </summary>
        /// <value>The accessors.</value>
        public List<IAccessorInfo> Accessors { get; set; } = new List<IAccessorInfo>();

        /// <summary>
        /// Gets or sets the parameter types.
        /// </summary>
        /// <value>The parameter types.</value>
        public List<ParameterInfo> ParameterTypes { get; set; } = new List<ParameterInfo>();

        /// <summary>
        /// Gets or sets the type of the return.
        /// </summary>
        /// <value>The type of the return.</value>
        public ParameterInfo ReturnType { get; set; } = new ParameterInfo { Name = Constants.SystemVoid };

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public NamespaceItem Namespace { get; set; }
        //public string NameWithParameter { get; set; }

        /// <summary>
        /// Name of element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the inheritance.
        /// </summary>
        /// <value>The inheritance.</value>
        public List<ClassInfo> Inheritance { get; set; } = new List<ClassInfo>();
    }
}

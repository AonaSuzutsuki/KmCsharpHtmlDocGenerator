using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using XmlDocumentParser.CsXmlDocument;

namespace XmlDocumentParser.EasyCs
{
    /// <summary>
    /// Type information is provided.
    /// </summary>
    public class TypeInfo
    {
        /// <summary>
        /// Namespace of type.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Fullname with namespace.
        /// </summary>
        public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

        /// <summary>
        /// Get the name of type.
        /// </summary>
        /// <param name="isFullname">Whether to use full path notation for classes, etc.</param>
        /// <returns>Name of type.</returns>
        public string GetName(bool isFullname)
        {
            return isFullname ? FullName : Name;
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        protected bool Equals(TypeInfo other)
        {
            return Namespace == other.Namespace && Name == other.Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Namespace != null ? Namespace.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
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
        public List<TypeInfo> ParameterTypes { get; set; } = new List<TypeInfo>();

        /// <summary>
        /// Gets or sets the type of the return.
        /// </summary>
        /// <value>The type of the return.</value>
        public TypeInfo ReturnType { get; set; } = new TypeInfo { Name = Constants.SystemVoid };

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

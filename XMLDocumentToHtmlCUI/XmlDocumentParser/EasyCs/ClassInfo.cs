using System;
using Microsoft.CodeAnalysis;

namespace XmlDocumentParser.EasyCs
{
	/// <summary>
	/// Class info for <see cref="CSharpEasyAnalyzer"/>.
    /// </summary>
	public class ClassInfo
    {
        public string Id { get; set; }
        public Accessibility Accessibility { get; set; }
        public ClassType ClassType { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
		public string ReturnType { get; set; } = Constants.SystemVoid;
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Inheritance { get; set; }
    }
}

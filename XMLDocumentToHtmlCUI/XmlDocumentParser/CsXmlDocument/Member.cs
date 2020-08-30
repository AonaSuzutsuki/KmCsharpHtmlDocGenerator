using CommonCoreLib.Bool;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentParser.EasyCs;
using XmlDocumentParser.MethodParameter;
using TypeInfo = XmlDocumentParser.EasyCs.TypeInfo;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// It expresses the lowest element such as method and property.
    /// </summary>
    public class Member
    {
        internal ClassInfo ClassInformation { get; set; }

        /// <summary>
        /// Identifier of this element.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Method type of member.
        /// </summary>
        public MethodType Type { get; set; }

        /// <summary>
        /// Namespace of member.
        /// </summary>
        public NamespaceItem Namespace { get; set; }

        /// <summary>
        /// Name of member.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Parameter types of member.
        /// </summary>
        public List<TypeInfo> ParameterTypes { get; set; } = new List<TypeInfo>();
        
        /// <summary>
        /// Value of member.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Return comment of member.
        /// </summary>
        public string ReturnComment { get; set; } = string.Empty;

        /// <summary>
        /// Parameter names of member.
        /// </summary>
        public Dictionary<string, string> ParameterNames { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Accessibility of this element. Require to analyze source code.
        /// </summary>
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        /// <summary>
        /// Type of return value. Require to analyze source code.
        /// </summary>
        public TypeInfo ReturnType { get; set; } = new TypeInfo { Name = Constants.SystemVoid };


        /// <summary>
        /// Get the definition of this element. Require to analyze source code.
        /// </summary>
        /// <param name="isFullname">Whether to use full path notation for classes, etc.</param>
        /// <returns>Definition of this element. Require to analyze source code.</returns>
        public string GetDefinition(bool isFullname) => ClassInformation == null ? string.Empty : ConvertToDefinition(ClassInformation, isFullname);

        private string ConvertToDefinition(ClassInfo classInfo, bool isFullname)
        {
            var sb = new StringBuilder();

            if (classInfo.ClassType == ClassType.Method || classInfo.ClassType == ClassType.Constructor)
            {
                sb.AppendFormat("{0} ", classInfo.Accessibility.ToString().ToLower());

                if (classInfo.IsOverride)
                    sb.Append("override ");
                if (classInfo.IsVirtual)
                    sb.Append("virtual ");
                if (classInfo.IsStatic)
                    sb.Append("static ");
                if (classInfo.IsAsync)
                    sb.Append("async ");
                if (classInfo.IsExtern)
                    sb.Append("extern ");

                if (classInfo.ClassType == ClassType.Method)
                    sb.AppendFormat("{0} ", classInfo.ReturnType.GetName(isFullname));
                sb.AppendFormat("{0}", classInfo.Name);
                sb.AppendFormat("{0};", MethodParameterConverter.CreateMethodParameterText(this, isFullname));
            }
            else if (classInfo.ClassType == ClassType.Property)
            {
                sb.AppendFormat("{0} ", classInfo.Accessibility.ToString().ToLower());
                sb.AppendFormat("{0} ", classInfo.ReturnType.GetName(isFullname));
                sb.AppendFormat("{0} {{ ", classInfo.Name);

                foreach (var accessors in classInfo.Accessors)
                {
                    if (accessors.Accessibility == Accessibility.Public)
                        sb.AppendFormat("{0}; ", accessors.Name);
                    else if (accessors.Accessibility != Accessibility.Private)
                        sb.AppendFormat("{0} {1}; ", accessors.Accessibility.ToString().ToLower(), accessors.Name);
                }

                sb.AppendFormat("}}");
            }

            return MethodParameterConverter.ResolveGenericsTypeToHtml(sb.ToString());
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

            var member = (Member)obj;
            var boolcollector = new BoolCollector();

            boolcollector.ChangeBool("Accessibility", Accessibility == member.Accessibility);
            boolcollector.ChangeBool("Id", Id.Equals(member.Id));
            boolcollector.ChangeBool("Name", Name.Equals(member.Name));
            boolcollector.ChangeBool("Namespace", Namespace.Equals(member.Namespace));
            boolcollector.ChangeBool("Parameters", ParameterNames.SequenceEqual(member.ParameterNames));
            boolcollector.ChangeBool("ParameterTypes", ParameterTypes.SequenceEqual(member.ParameterTypes));
            boolcollector.ChangeBool("ReturnComment", ReturnComment.Equals(member.ReturnComment));
            boolcollector.ChangeBool("ReturnType", ReturnType.Equals(member.ReturnType));
            boolcollector.ChangeBool("Type", Type == member.Type);
            boolcollector.ChangeBool("Value", Value.Equals(member.Value));

            return boolcollector.Value;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:XmlDocumentParser.CsXmlDocument.Member"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:XmlDocumentParser.CsXmlDocument.Member"/>.</returns>
        public override string ToString()
        {
            return Namespace + "." + Name;
        }
    }
}

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
    /// It expresses the lowest element such as method and property.
    /// </summary>
    public class Member
    {
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
        public List<string> ParameterTypes { get; set; } = new List<string>();
        
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
        /// Difinition of this element. Require to analyze source code.
        /// </summary>
        public string Difinition { get; set; } = string.Empty;

        /// <summary>
        /// Type of return value. Require to analyze source code.
        /// </summary>
        public string ReturnType { get; set; } = Constants.SystemVoid;


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
            boolcollector.ChangeBool("Difinition", Difinition.Equals(member.Difinition));
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

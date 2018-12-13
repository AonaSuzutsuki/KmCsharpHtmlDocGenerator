using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentParser.CsXmlDocument;

namespace XmlDocumentParser.MethodParameter
{
    /// <summary>
    /// Provides functions to convert of method parameters.
    /// </summary>
    public static class MethodParameterConverter
    {

        /// <summary>
        /// Create method parameter text from <see cref="Member"/>.
        /// exm: (string arg1, string arg2)
        /// </summary>
        /// <param name="member">Target <see cref="Member"/> to convert.</param>
        /// <returns>Converted text.</returns>
        public static string CreateMethodParameterText(Member member)
        {
            var parameters = member.MethodParameters.Zip(member.Parameters.Keys, (type, name) => new { Type = type, Name = name });
            var parameterSb = new StringBuilder();
            foreach (var param in parameters.Select((v, i) => new { Index = i, Value = v }))
            {
                if (param.Index < member.Parameters.Count - 1)
                    parameterSb.AppendFormat("{0} {1}, ", ResolveType(param.Value.Type), param.Value.Name);
                else
                    parameterSb.AppendFormat("{0} {1}", ResolveType(param.Value.Type), param.Value.Name);
            }

            return "({0})".FormatString(parameterSb.ToString());
        }

        /// <summary>
        /// Convert type string to System type and escaped tag.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ResolveType(string text)
        {

            text = text.Replace("System.Byte", "byte");
            text = text.Replace("System.Int32", "int");
            text = text.Replace("System.Int64", "long");
            text = text.Replace("System.Boolean", "bool");
            text = text.Replace("System.String", "string");
            text = text.Replace("System.Object", "object");
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");

            return text.Replace("{", "&lt;").Replace("}", "&gt;");
        }
    }
}

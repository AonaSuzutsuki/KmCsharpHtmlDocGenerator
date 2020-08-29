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
        /// <param name="isFullname"></param>
        /// <param name="converter">Param type converter.</param>
        /// <returns>Converted text.</returns>
        public static string CreateMethodParameterText(Member member, bool isFullname)
        {
            var parameters = member.ParameterTypes.Zip(member.ParameterNames.Keys, (type, name) => new { Type = type, Name = name });
            var sb = new StringBuilder();

            if (member.Type == MethodType.ExtensionMethod)
                sb.Append("this ");
            sb.Append(string.Join(", ", parameters.Select(param => "{0} {1}".FormatString(
                param.Type.GetName(isFullname),
                param.Name))));

            return "({0})".FormatString(sb.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ResolveGenericsTypeToHtml(string text)
        {
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");
            return text;
        }

        internal static string ResolveIdToGenericsType(string text)
        {
            text = text.Replace("{", "<");
            text = text.Replace("}", ">");
            return text;
        }
        
        internal static string ResolveSystemType(string text)
        {
            text = text.Replace("System.Byte", "byte");
            text = text.Replace("System.Int32", "int");
            text = text.Replace("System.Int64", "long");
            text = text.Replace("System.Boolean", "bool");
            text = text.Replace("System.String", "string");
            text = text.Replace("System.Object", "object");
            return text;
        }
    }
}

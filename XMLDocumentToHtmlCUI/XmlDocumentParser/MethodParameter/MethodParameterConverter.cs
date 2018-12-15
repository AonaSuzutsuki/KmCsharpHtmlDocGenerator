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
		public static string CreateMethodParameterText(Member member, Func<string, string> converter = null)
        {
            if (converter == null)
                converter = ResolveGenericsTypeToHtml;

            var parameters = member.ParameterTypes.Zip(member.Parameters.Keys, (type, name) => new { Type = type, Name = name });
            var sb = new StringBuilder();

            if (member.Type == MethodType.ExtensionMethod)
                sb.Append("this ");

            foreach (var param in parameters.Select((v, i) => new { Index = i, Value = v }))
            {
                sb.AppendFormat("{0} {1}, ", converter(param.Value.Type), param.Value.Name);
            }
            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);

            return "({0})".FormatString(sb.ToString());
        }
        
        internal static string ResolveIdToGenericsType(string text)
        {
            text = text.Replace("{", "<");
            text = text.Replace("}", ">");
            return text;
        }

        public static string ResolveGenericsTypeToHtml(string text)
        {
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");
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

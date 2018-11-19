using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    public enum MethodType
    {
        Class,
        Constructor,
        Field,
        Property,
        Method,
    }

    public class Member
    {
        public MethodType Type { get; set; }
        public NamespaceItem NameSpace { get; set; }
        public string Name { get; set; }
        public List<string> MethodParameters { get; set; } = new List<string>();
        public string Value { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            sb.AppendFormat("\t{0}: {1}\n", "Type", Type.ToString());
            sb.AppendFormat("\t{0}: {1}\n", "NameSpace", NameSpace.ToString());
            sb.AppendFormat("\t{0}: {1}\n", "Name", Name.ToString());
            sb.AppendFormat("\t{0}: {1}\n", "MethodParameters", MethodParameters.ToString());
            sb.AppendFormat("\t{0}: {1}\n", "Value", Value.ToString());
            sb.AppendFormat("\t{0}: {1}\n", "Parameters", Parameters.ToString());
            sb.AppendLine("]");
            return sb.ToString();
        }
    }

    public class CsXmlDocumentParser
    {
        public List<Member> Members { get; private set; }

        public CsXmlDocumentParser(string xmlPath)
        {
            Parse(xmlPath);
        }

        private void Parse(string xmlPath)
        {
            using (var fs = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Members = new List<Member>();

                var reader = new Reader(fs);
                var vals = reader.GetAttributes("name", "/doc/members/member");
                foreach (var val in vals)
                {
                    var value = reader.GetValue("/doc/members/member[@name=\"{0}\"]/summary".FormatString(val));
                    value = RemoveFirstLastBreakLine(value);

                    var member = ConvertMemberNameToMember(val);
                    member.Value = value;

                    var xparams = reader.GetAttributes("name", "/doc/members/member[@name=\"{0}\"]/param".FormatString(val));
                    foreach (var param in xparams)
                    {
                        var path = "/doc/members/member[@name=\"{0}\"]/param[@name=\"{1}\"]".FormatString(val, param);
                        var value2 = reader.GetValue(path);
                        value2 = RemoveFirstLastBreakLine(value2);

                        member.Parameters.Add(param, value2);
                    }
                    Members.Add(member);
                }
            }
        }

        static Member ConvertMemberNameToMember(string text)
        {
            var member = new Member();

            var reg = new Regex("(?<Type>.*):((?<MethodName>.*)\\((?<Parameters>.*)\\)|(?<MethodName>.*))");
            var match = reg.Match(text);
            if (match.Success)
            {
                var (nameSpace, methodName) = SplitMethodName(match.Groups["MethodName"].ToString());
                var parameters = match.Groups["Parameters"].ToString().Replace(" ", "").Split(',');
                var strType = ConvertConstructorType(match.Groups["Type"].ToString(), methodName);
                var type = ConvertMethodType(strType);

                member.Type = type;
                if (type != MethodType.Class)
                    member.NameSpace = nameSpace.GetNamespace();
                else
                    member.NameSpace = nameSpace;
                member.Name = methodName;
                member.MethodParameters.AddRange(parameters);
            }

            return member;
        }

        static string ConvertConstructorType(string baseType, string methodName)
        {
            if (methodName.Equals("#ctor"))
                return "C";
            return baseType;
        }

        static (NamespaceItem nameSpaces, string methodName) SplitMethodName(string fullname)
        {
            var namespaceItem = new NamespaceItem(fullname);
            return (namespaceItem.GetNamespace(), namespaceItem.GetLastName());
        }

        static MethodType ConvertMethodType(string text)
        {
            var map = new Dictionary<string, MethodType>
            {
                { "T", MethodType.Class },
                { "P", MethodType.Property },
                { "C", MethodType.Constructor },
                { "M", MethodType.Method }
            };
            return map[text];
        }

        static string RemoveFirstLastBreakLine(string text)
        {
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\r", "\n");
            text = text.TrimStart('\n').TrimEnd('\n');
            return text;
        }
    }
}

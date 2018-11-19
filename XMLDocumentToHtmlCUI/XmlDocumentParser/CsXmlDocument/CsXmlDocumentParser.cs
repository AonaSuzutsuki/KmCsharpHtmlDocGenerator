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
    public enum ElementType
    {
        Root,
        Namespace,
        Class,
        Interface,
    }
    public class Element
    {
        public ElementType Type { get; set; }
        public string Name { get; set; }
        public List<Element> Namespaces { get; set; } = new List<Element>();
        public List<Member> Members { get; set; } = new List<Member>();

        public bool HasElement(string name)
        {
            foreach (var elem in Namespaces)
            {
                if (elem.Name.Equals(name))
                    return true;
            }
            return false;
        }

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendFormat("[{0}: {1}, ", "Type", Type.ToString());
        //    sb.AppendFormat("{0}: {1}\n", "Name", Name.ToString());
            
        //    foreach (var elem in Namespaces.Select((v, i) => new { Index = i, Value = v }))
        //    {
        //        sb.AppendFormat("\t{0}\n", elem.Value.ToString());
        //    }

        //    foreach (var elem in Members.Select((v, i) => new { Index = i, Value = v }))
        //    {
        //        sb.AppendFormat("\t{0}\n", elem.Value.ToString());
        //    }

        //    sb.AppendLine("]");
        //    return sb.ToString();
        //}
    }

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
            sb.AppendFormat("[{0}: {1},", "Type", Type.ToString());
            sb.AppendFormat("{0}: {1},", "NameSpace", NameSpace.ToString());
            sb.AppendFormat("{0}: {1},", "Name", Name.ToString());
            sb.AppendFormat("{0}: {1},", "MethodParameters", MethodParameters.ToString());
            sb.AppendFormat("{0}: {1},", "Value", Value.ToString());
            sb.AppendFormat("{0}: {1}]\n", "Parameters", Parameters.ToString());
            return sb.ToString();
        }
    }

    public class CsXmlDocumentParser
    {
        public Element TreeElement{ get; private set; }

        public CsXmlDocumentParser(string xmlPath)
        {
            var members = FirstParse(xmlPath);
            TreeElement = SecondParse(members);
        }

        private List<Member> FirstParse(string xmlPath)
        {
            var members = new List<Member>();
            using (var fs = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
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
                    members.Add(member);
                }
            }
            return members;
        }

        private Element SecondParse(List<Member> members)
        {
            var root = new Element()
            {
                Type = ElementType.Root,
                Name = "Root",
                Members = null
            };

            Element preElem = root;
            Element classElem = root;

            foreach (var member in members)
            {
                var nameSpace = member.NameSpace;
                while (true)
                {
                    var firstName = nameSpace.GetFirstName();
                    if (firstName == null)
                    {
                        break;
                    }

                    var elem = new Element()
                    {
                        Type = ElementType.Namespace,
                        Name = firstName,
                        Members = null
                    };

                    if (!preElem.HasElement(elem.Name))
                    {
                        preElem.Namespaces.Add(elem);
                        preElem = elem;
                    }
                    else
                    {
                        preElem = preElem.Namespaces[preElem.Namespaces.Count - 1];
                    }

                    nameSpace = nameSpace.GetNamespaceRemoveFirst();
                }

                if (member.Type == MethodType.Class)
                {
                    classElem = new Element()
                    {
                        Type = ElementType.Class,
                        Name = member.Name,
                        Namespaces = null
                    };
                    preElem.Namespaces.Add(classElem);
                }
                else
                {
                    classElem.Members.Add(member);
                }

                preElem = root;
            }
            return root;
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

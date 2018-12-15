using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentExtensions.Extensions;
using XmlDocumentParser.XmlWrapper;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// Parse C# XML Document.
    /// </summary>
    public class CsXmlDocumentParser
    {
        #region Constants
        private const int ParameterLoopLimit = 10;
        #endregion

        #region Properties
        /// <summary>
        /// Parsed tree structure elements.
        /// </summary>
        public Element TreeElement{ get; private set; }

        /// <summary>
        /// Count of namespaces.
        /// </summary>
        public int NamespaceCount { get; private set; }

        /// <summary>
        /// Count of Classes.
        /// </summary>
        public int ClassCount { get; private set; }

        /// <summary>
        /// Path of the configured XML file.
        /// </summary>
        public string XmlPath { get; }
        #endregion

        /// <summary>
        /// Initialize C# XML Parser.
        /// </summary>
        /// <param name="xmlPath"></param>
        public CsXmlDocumentParser(string xmlPath)
        {
            XmlPath = xmlPath;
        }

        /// <summary>
        /// Parse C# XML Document
        /// </summary>
        /// <returns type="Element">Parsed tree structure elements.</returns>
        public Element Parse()
        {
            var members = FirstParse(XmlPath);
            TreeElement = SecondParse(members);
            return TreeElement;
        }

        private List<Member> FirstParse(string xmlPath)
        {
            var members = new List<Member>();
            using (var fs = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new Reader();
                reader.LoadFromStream(fs);
                var vals = reader.GetAttributes("name", "/doc/members/member");
                foreach (var val in vals)
                {
                    var value = reader.GetValue("/doc/members/member[@name=\"{0}\"]/summary".FormatString(val));
                    var ret = reader.GetValue("/doc/members/member[@name=\"{0}\"]/returns".FormatString(val));
                    value = RemoveFirstLastBreakLine(value);

                    var member = ConvertMemberNameToMember(val);
                    member.Value = value;
                    member.ReturnComment = ret;

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
            //Element classElem = root;

            var elemMap = new Dictionary<string, Element>();
            var classElemMap = new Dictionary<string, Element>();

            foreach (var member in members)
            {
                var nameSpace = member.Namespace;
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
                        Namespace = member.Namespace,
                        Name = firstName
                    };

                    if (!preElem.HasElement(elem.Name))
                    {
                        preElem.Namespaces.Add(elem);
                        preElem = elem;
                        NamespaceCount++;
                    }
                    else
                    {
                        preElem = preElem.Namespaces[preElem.Namespaces.Count - 1];
                    }

                    nameSpace = nameSpace.GetNamespaceWithoutFirst();
                }

                if (member.Type == MethodType.Class)
                {
                    var name = member.Name;

                    var classElem = new Element()
                    {
                        Id = member.Id,
                        Type = ElementType.Class,
                        Namespace = member.Namespace,
                        Name = name,
                        Value = member.Value,
						Namespaces = new List<Element>()
                    };

                    classElemMap.Put("{0}.{1}".FormatString(classElem.Namespace.ToString(), classElem.Name), classElem);
                    
                    preElem.Namespaces.Add(classElem);
                    ClassCount++;
                }
                else
                {
                    var classElem = classElemMap.Get(member.Namespace.ToString(), new Element());
                    if ("{0}.{1}".FormatString(classElem.Namespace.ToString(), classElem.Name).Equals(member.Namespace.ToString()))
                        classElem.Members.Add(member);
                }

                preElem = root;
            }
            return root;
        }
        
        private static string ConvertConstructorType(string baseType, string methodName)
        {
            if (methodName.Equals("#ctor"))
                return "C";
            return baseType;
        }

        private static (NamespaceItem nameSpaces, string methodName) SplitMethodName(string fullname)
        {
            var namespaceItem = new NamespaceItem(fullname);
            return (namespaceItem.GetParentNamespace(), namespaceItem.GetLastName());
        }

        private static MethodType ConvertMethodType(string text)
        {
            var map = new Dictionary<string, MethodType>
            {
                { "T", MethodType.Class },
				{ "E", MethodType.Event },
                { "P", MethodType.Property },
                { "C", MethodType.Constructor },
                { "M", MethodType.Method },
				{ "F", MethodType.Field }
            };
            return map[text];
        }

        private static string RemoveFirstLastBreakLine(string text)
        {
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\r", "\n");
            text = text.TrimStart('\n').TrimEnd('\n');
            return text;
        }

        /// <summary>
		/// Converts the name text to <see cref="Member"/>.
        /// </summary>
        /// <returns><see cref="Member"/>.</returns>
        /// <param name="text">The name text.</param>
		public static Member ConvertMemberNameToMember(string text)
        {
            string ResolveSplitParameter(string split)
            {
                for (int i = 0; i < ParameterLoopLimit; i++)
                {
                    var splitReg = new Regex("~(?<encoded>.*)~");
                    var splitMatch = splitReg.Match(split);
                    if (splitMatch.Success)
                    {
                        var full = splitMatch.ToString();
                        var encoded = splitMatch.Groups["encoded"].ToString();
                        var replacedText = split.Replace(full, Encoding.UTF8.GetString(Convert.FromBase64String(encoded)));
                        split = replacedText;
                    }
                    else
                    {
                        break;
                    }
                }
                return split;
            }
            string[] SplitParameter(string parameterText)
            {
                for (int i = 0; i < ParameterLoopLimit; i++)
                {
                    var splitReg = new Regex("(?<parameter>\\{(.*)\\}),|(?<parameter>\\{(.*)\\})");
                    var splitMatch = splitReg.Match(parameterText);
                    while (splitMatch.Success)
                    {
                        var full = splitMatch.Groups["parameter"].ToString();
                        var replacedText = parameterText.Replace(full, "~{0}~".FormatString(Convert.ToBase64String(Encoding.UTF8.GetBytes(full))));
                        parameterText = replacedText;

                        splitMatch = splitMatch.NextMatch();
                    }
                }

                var splits = parameterText.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    var systemType = MethodParameter.MethodParameterConverter.ResolveIdToGenericsType(ResolveSplitParameter(splits[i]));
                    splits[i] = MethodParameter.MethodParameterConverter.ResolveSystemType(systemType);
                }

                return splits;
            }

            var member = new Member();

            var reg = new Regex("(?<Type>.*):((?<MethodName>.*)\\((?<Parameters>.*)\\)|(?<MethodName>.*))");
            var match = reg.Match(text);
            if (match.Success)
            {
                var (nameSpace, methodName) = SplitMethodName(match.Groups["MethodName"].ToString());
                var parameters = SplitParameter(match.Groups["Parameters"].ToString());
                var strType = ConvertConstructorType(match.Groups["Type"].ToString(), methodName);
                var type = ConvertMethodType(strType);

                member.Type = type;
                member.Namespace = nameSpace;
                member.Name = methodName;
                member.ParameterTypes.AddRange(parameters);
            }

            member.Id = text;
            return member;
        }

        /// <summary>
        /// Parse multiple Files.
        /// </summary>
        /// <param name="files">Array of filepath</param>
        /// <param name="rootName">Root name.</param>
        /// <returns>Parsed element.</returns>
        public static Element ParseMultiFiles(string[] files, string rootName = "Root")
        {
            Element root = new Element()
            {
                Name = rootName,
                Type = ElementType.Root
            };
            foreach (var input in files)
            {
                var parser = new CsXmlDocumentParser(input);
                root.Namespaces.AddRange(parser.Parse().Namespaces);
            }
            return root;
        }
    }
}

using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentExtensions.Extensions;
using XmlDocumentParser.EasyCs;
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

        #region Events
        /// <summary>
        /// Parse type.
        /// </summary>
        public enum ParseType
        {
            /// <summary>
            /// The first.
            /// </summary>
            First,
            /// <summary>
            /// The second.
            /// </summary>
            Second
        }

        /// <summary>
        /// Xml document parse progress.
        /// </summary>
        public interface IXmlDocumentParseProgress
        {
            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>The type.</value>
            ParseType Type { get; }
        }

        /// <summary>
        /// Xml document parse progress event arguments.
        /// </summary>
		public class XmlDocumentParseProgressEventArgs : EventArgs, IXmlDocumentParseProgress
        {
            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>The type.</value>
            public ParseType Type { get; }

            /// <summary>
            /// Gets the max.
            /// </summary>
            /// <value>The max.</value>
			public int Max { get; }

            /// <summary>
            /// Gets the current.
            /// </summary>
            /// <value>The current.</value>
			public int Current { get; }

            /// <summary>
            /// Gets the percentage.
            /// </summary>
            /// <value>The percentage.</value>
			public int Percentage { get; }

            /// <summary>
            /// Gets the filename.
            /// </summary>
            /// <value>The filename.</value>
			public string Filename { get; }

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="T:XmlDocumentParser.CsXmlDocument.CsXmlDocumentParser.XmlDocumentParseProgressEventArgs"/> class.
            /// </summary>
            /// <param name="type">Type.</param>
            /// <param name="max">Max.</param>
            /// <param name="current">Current.</param>
            /// <param name="filename">Filename.</param>
			public XmlDocumentParseProgressEventArgs(ParseType type, int max, int current, string filename)
			{
                Type = type;
				Max = max;
				Current = current;
				Percentage = (int)((double)Current / Max * 100);
				Filename = filename;
			}
		}

        /// <summary>
        /// Xml document parse progress event handler.
        /// </summary>
        public delegate void XmlDocumentParseProgressEventHandler(object sender, XmlDocumentParseProgressEventArgs eventArgs);

        /// <summary>
        /// Xml document parse completed event handler.
        /// </summary>
        public delegate void XmlDocumentParseCompletedEventHandler(object sender, IXmlDocumentParseProgress eventArgs);

        /// <summary>
        /// Occurs when parse progress.
        /// </summary>
		public event XmlDocumentParseProgressEventHandler ParseProgress;

        /// <summary>
        /// Occurs when parse completed.
        /// </summary>
		public event XmlDocumentParseCompletedEventHandler ParseCompleted;
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
            ParseCompleted?.Invoke(this, new XmlDocumentParseProgressEventArgs(ParseType.First, 0, 0, null));
            var treeElement = SecondParse(members);
            ParseCompleted?.Invoke(this, new XmlDocumentParseProgressEventArgs(ParseType.Second, 0, 0, null));

            return treeElement;
		}

        /// <summary>
        /// Parses from text.
        /// </summary>
        /// <returns>The from text.</returns>
        public Element ParseFromText()
        {
            var reader = new Reader();
            reader.LoadFromText(XmlPath);
            var members = FirstParse(reader);

            ParseCompleted?.Invoke(this, new XmlDocumentParseProgressEventArgs(ParseType.First, 0, 0, null));
            var treeElement = SecondParse(members);
            ParseCompleted?.Invoke(this, new XmlDocumentParseProgressEventArgs(ParseType.Second, 0, 0, null));

            return treeElement;
        }

        private List<Member> FirstParse(string xmlPath)
        {
            using var fs = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var reader = new Reader();
            reader.LoadFromStream(fs);
            return FirstParse(reader);
        }

        private List<Member> FirstParse(Reader reader)
        {
            int eventIndex = 0;
            var members = new List<Member>();

            var vals = reader.GetAttributes("name", "/doc/members/member");
            foreach (var tuple in vals.Select((v, i) => new { Value = v, Index = i }))
            {
                var values = reader.GetValues("/doc/members/member[@name=\"{0}\"]/summary".FormatString(tuple.Value));
                var value = string.Join("\n", values.Select(s => s));
				var ret = reader.GetValue("/doc/members/member[@name=\"{0}\"]/returns".FormatString(tuple.Value), false);
                value = RemoveFirstLastBreakLine(value);

                var member = ConvertMemberNameToMember(tuple.Value);
                member.Value = value;
                member.ReturnComment = ret ?? string.Empty;

                var xparams = reader.GetAttributes("name", "/doc/members/member[@name=\"{0}\"]/param".FormatString(tuple.Value));
                foreach (var param in xparams)
                {
                    var path = "/doc/members/member[@name=\"{0}\"]/param[@name=\"{1}\"]".FormatString(tuple.Value, param);
                    var value2 = reader.GetValue(path);
                    value2 = RemoveFirstLastBreakLine(value2);

                    member.ParameterNames.Add(param, value2);
                }
                members.Add(member);

                ParseProgress?.Invoke(this, new XmlDocumentParseProgressEventArgs(
                    ParseType.First, vals.Count, ++eventIndex, tuple.Value));
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

            int eventIndex = 0;
			Element preElem = root;
			//Element classElem = root;

			var elemMap = new Dictionary<string, Element>();
			var classElemMap = new Dictionary<string, Element>();

			foreach (var tuple in members.Select((v, i) => new { Value = v, Index = i }))
			{
				var member = tuple.Value;
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

                ParseProgress?.Invoke(this, new XmlDocumentParseProgressEventArgs(
                    ParseType.Second, members.Count, ++eventIndex, tuple.Value.Id));
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
				{ "F", MethodType.Field },
				{ "!", MethodType.Unknown }
			};
			return map[text];
		}

		private static string RemoveFirstLastBreakLine(string text)
		{
            text = text.UnifiedNewLine();
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
            static string ResolveSplitParameter(string split)
			{
				for (var i = 0; i < ParameterLoopLimit; i++)
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

            static TypeInfo[] SplitParameter(string parameterText)
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

				if (string.IsNullOrEmpty(parameterText))
				{
					return new TypeInfo[0];
				}
				else
				{
					var splits = parameterText.Split(',');
					var list = new List<TypeInfo>();
					for (int i = 0; i < splits.Length; i++)
					{
						var systemType = MethodParameter.MethodParameterConverter.ResolveIdToGenericsType(ResolveSplitParameter(splits[i]));
						var fullName = MethodParameter.MethodParameterConverter.ResolveSystemType(systemType);
						var paramInfo = CSharpEasyAnalyzer.CreateParameterInfo(fullName);
						list.Add(paramInfo);
					}

					return list.ToArray();
				}
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
        /// <param name="parseProgressEventHandler"></param>
        /// <param name="completed"></param>
        /// <param name="startAct"></param>
        /// <returns>Parsed element.</returns>
        public static Element ParseMultiFiles(string[] files, string rootName = "Root",
            XmlDocumentParseProgressEventHandler parseProgressEventHandler = null, XmlDocumentParseCompletedEventHandler completed = null, Action<string> startAct = null)
        {
            Element root = new Element
            {
                Name = rootName,
                Type = ElementType.Root
            };
            foreach (var input in files)
            {
                startAct?.Invoke(input);
                var parser = new CsXmlDocumentParser(input);
                if (parseProgressEventHandler != null)
                    parser.ParseProgress += parseProgressEventHandler;
                if (completed != null)
                    parser.ParseCompleted = completed;
                var parseResult = parser.Parse();
                root.Namespaces.AddRange(parseResult.Namespaces);
            }
            return root;
        }

        /// <summary>
        /// Parses from text.
        /// </summary>
        /// <returns>The from text.</returns>
        /// <param name="xmlDocument">Xml document.</param>
        /// <param name="rootName">Root name.</param>
        /// <param name="parseProgressEventHandler">Parse progress event handler.</param>
        /// <param name="completed">Completed.</param>
        /// <param name="startAct">Start act.</param>
        public static Element ParseFromText(string xmlDocument, string rootName = "Root",
            XmlDocumentParseProgressEventHandler parseProgressEventHandler = null, XmlDocumentParseCompletedEventHandler completed = null, Action<string> startAct = null)
        {
            Element root = new Element
            {
                Name = rootName,
                Type = ElementType.Root
            };

            startAct?.Invoke("");
            var parser = new CsXmlDocumentParser(xmlDocument);
            if (parseProgressEventHandler != null)
                parser.ParseProgress += parseProgressEventHandler;
            if (completed != null)
                parser.ParseCompleted = completed;
            var parseResult = parser.ParseFromText();
            root.Namespaces.AddRange(parseResult.Namespaces);

            return root;
        }
    }
}

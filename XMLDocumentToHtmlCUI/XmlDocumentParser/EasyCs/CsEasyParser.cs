using CommonCoreLib.File;
using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentExtensions.Extensions;
using XmlDocumentParser.CsXmlDocument;

namespace XmlDocumentParser.EasyCs
{
    public class ClassInfo
    {
        public string Accessibility { get; set; }
        public string Modifier { get; set; }
        public string ClassType { get; set; }
        public string Name { get; set; }
        public string Inheritance { get; set; }
    }

    public class CsEasyParser
    {

        private Dictionary<string, ClassInfo> classMap = new Dictionary<string, ClassInfo>();
        private Dictionary<string, ClassInfo> methodMap = new Dictionary<string, ClassInfo>();

        public void Parse()
        {         
			var filepaths = GetCsFiles(@"/Users/aonasuzutsuki/Git/ImageManager/ImageManager/ImageManager");
            foreach (var filename in filepaths)
            {
				var text = File.ReadAllText(filename).Replace("\r\n", "\r").Replace("\r", "\n");
                text = RemoveComments(text);

                var namespaceItem = GetNamespace(text);

                ClassAnalyze(text, namespaceItem);
				MethodAnalyze(text, namespaceItem);
            }
        }

        public void ClassAnalyze(string code, NamespaceItem namespaceItem)
        {
			void test(string text)
			{
				var reg = "^( |\\t)*((?<accessibility>public|private|protected)?(\\s|\\t)*)?((?<modifier>static|abstract|sealed|partial)?(\\s|\\t)*)?((?<type>class|interface|enum|struct)(\\s|\\t)*)((?<class>[\\S]+)(\\s|\\t)*)(:(\\s|\\t)*(?<inheritance>[\\S]+))?";
                var regex = new Regex(reg, RegexOptions.Multiline);
                var match = regex.Match(text);
                while (match.Success)
                {
                    var accessibility = match.Groups["accessibility"].ToString();
                    var modifier = match.Groups["modifier"].ToString();
                    var type = match.Groups["type"].ToString();
                    var name = match.Groups["class"].ToString();
                    var inheritance = match.Groups["inheritance"].ToString();

                    var fullname = namespaceItem.NamespaceCount > 0 ? "{0}.{1}".FormatString(namespaceItem, name) : name;
                    classMap.Put(fullname, new ClassInfo()
                    {
                        Accessibility = accessibility,
                        Modifier = modifier,
                        ClassType = type,
                        Name = name,
                        Inheritance = inheritance
                    });

                    match = match.NextMatch();
                }
			}

			var codeArray = code.Split('\n');
			var splitter = new ArraySplitter(codeArray, 3);
			foreach (var elem in splitter)
			{
				test(elem);
			}
        }

        public void MethodAnalyze(string code, NamespaceItem namespaceItem)
        {
			void test(string text)
			{
				var reg = "^( |\\t)*(((?<accessibility>public|private|protected)?(\\s|\\t)*)?((?<modifier>static|virtual|override)?(\\s|\\t)*)?((?<type>[\\S]+)(\\s|\\t)+)((?<name>[\\S]+)(\\s|\\t)*)(\\(([ \\S]+)\\)))$";
                var regex = new Regex(reg, RegexOptions.Multiline);
				var match = regex.Match(text);
                while (match.Success)
                {
                    var accessibility = match.Groups["accessibility"].ToString();
                    var modifier = match.Groups["modifier"].ToString();
                    var type = match.Groups["type"].ToString();
                    var name = match.Groups["class"].ToString();
                    var inheritance = match.Groups["inheritance"].ToString();

                    var fullname = namespaceItem.NamespaceCount > 0 ? "{0}.{1}".FormatString(namespaceItem, name) : name;
                    methodMap.Put(fullname, new ClassInfo()
                    {
                        Accessibility = accessibility,
                        Modifier = modifier,
                        ClassType = type,
                        Name = name,
                        Inheritance = inheritance
                    });

                    match = match.NextMatch();
                }
			}

			var codeArray = code.Split('\n');
            var splitter = new ArraySplitter(codeArray, 10);
            foreach (var elem in splitter)
            {
                test(elem);
            }
        }

        private string[] GetCsFiles(string csprojParentPath)
        {
            var list = new List<string>();
            var filepaths = DirectorySearcher.GetAllFiles(csprojParentPath, "*.csproj");
            foreach (var file in filepaths)
            {
                var reader = new XmlWrapper.Reader();
                reader.LoadFromFile(file);
                reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                var parent = "{0}/".FormatString(Path.GetDirectoryName(file));
                var includes = MergeParentPath(reader.GetAttributes("Include", "/ns:Project/ns:ItemGroup/ns:Compile"), parent);
                list.AddRange(includes);
            }
            return list.ToArray();
        }

        private List<string> MergeParentPath(List<string> list, string parent)
        {
            var retList = new List<string>(list);
            for (int i = 0; i < retList.Count; i++)
            {
				retList[i] = CommonPath.PathUtils.ResolvePathSeparator(parent + retList[i]);
            }
            return retList;
        }

        private static string RemoveComments(string text)
        {
            var reg = "(\\s|\\t)*((\\/\\*([\\s\\S]*)\\*\\/)|([\\/]+(.*)))";
            return Regex.Replace(text, reg, "", RegexOptions.Multiline);
        }

        private static NamespaceItem GetNamespace(string text)
        {
            var reg = "^( |\\t)*((namespace)(\\s|\\t)+)(?<name>[\\S]+)";
            var regex = new Regex(reg, RegexOptions.Multiline);
            var match = regex.Match(text);
            if (match.Success)
            {
                var namespaceName = match.Groups["name"].ToString();
                return new NamespaceItem(namespaceName);
            }
            return new NamespaceItem();
        }
    }
}

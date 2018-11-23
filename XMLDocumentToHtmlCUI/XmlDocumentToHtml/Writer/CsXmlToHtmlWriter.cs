using CommonCoreLib.File;
using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentParser.CsXmlDocument;
using XmlDocumentToHtml.Crypto;
using XmlDocumentToHtml.Extensions;

namespace XmlDocumentToHtml.Writer
{
    public class CsXmlToHtmlWriter
    {

        private readonly Element root;

        public string TemplateDir { get; set; } = "BaseTemplate";
        public string BaseTemplatePath { get => "{0}/BaseTemplate.html".FormatString(TemplateDir); }
        public string BaseMethodTemplate { get => "{0}/BaseMethodTemplate.html".FormatString(TemplateDir); }
        public string BasePropertyTemplate { get => "{0}/BasePropertyTemplate.html".FormatString(TemplateDir); }
        public string ParameterTableTemplate { get => "{0}/BaseParameterTemplate.html".FormatString(TemplateDir); }

        public CsXmlToHtmlWriter(Element root)
        {
            this.root = root;
        }

        public void WriteToDisk()
        {
            //var menu = CreateMenu(root);
            CreateDirectory(root);
            CreateClassFile(root, root);
            CloneFiles(root.Name);
        }

        private void CreateDirectory(Element element, string suffix = "")
        {
            if (element != null)
            {
                if (element.Namespaces != null)
                {
                    var name = suffix + element.Name;
                    var di = new DirectoryInfo(name);
                    if (!di.Exists)
                        di.Create();
                    foreach (var elem in element.Namespaces)
                        CreateDirectory(elem, name + "/");
                }
            }
        }

        private void CreateClassFile(Element element, Element root, string suffix = "")
        {
            if (element != null)
            {
                if (element.Namespaces != null)
                {
                    var name = suffix + element.Name;
                    foreach (var elem in element.Namespaces)
                        CreateClassFile(elem, root, name + "/");
                }
                else
                {
                    var name = suffix + element.Name + ".html";
                    using (var fs = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        WriteHtml(fs, element.Members, element, root);
                    }
                }
            }
        }

        private void CloneFiles(string outPath)
        {
            var di = new DirectoryInfo("BaseTemplate/Clone");
            if (di.Exists)
            {
                var dirs = DirectorySearcher.GetAllDirectories(di.FullName);
                foreach (var dir in dirs)
                {
                    var relativeDir = ResolvePathSeparator(dir).Replace(ResolvePathSeparator(di.FullName) + "/", "");
                    relativeDir = "{0}/{1}".FormatString(outPath, relativeDir);
                    if (!Directory.Exists(relativeDir))
                        Directory.CreateDirectory(relativeDir);
                }

                var files = DirectorySearcher.GetAllFiles(di.FullName);
                foreach (var file in files)
                {
                    var relativeFile = ResolvePathSeparator(file).Replace(ResolvePathSeparator(di.FullName) + "/", "");
                    relativeFile = "{0}/{1}".FormatString(outPath, relativeFile);
                    if (!File.Exists(relativeFile))
                        File.Copy(file, relativeFile);
                }
            }
        }

        private string ResolvePathSeparator(string path)
        {
            return path.Replace("\\", "/");
        }

        private string CreateMenu(Element element, int link,string suffix = "")
        {
            if (element.Type == ElementType.Root)
            {
                var sb2 = new StringBuilder();
                sb2.AppendLine("<ul>");
                foreach (var elem in element.Namespaces)
                    sb2.Append(CreateMenu(elem, link, "    "));
                sb2.AppendLine("</ul>");
                return sb2.ToString();
            }

            if (element == null)
                return string.Empty;

            var sb = new StringBuilder();
            if (element.Namespaces != null)
            {
                var name = suffix + "<li>" + element.Name;
                sb.AppendLine(name);
                sb.AppendLine(suffix + "    <ul>");
                foreach (var elem in element.Namespaces)
                    sb.Append(CreateMenu(elem, link, suffix + "        "));
                sb.AppendLine(suffix + "    </ul>");
                sb.AppendLine(suffix + "</li>");
            }
            else
            {
                var namespacePath = element.Namespace.ToString().Replace(".", "/");
                var name = "{0}<li><a href=\"{1}{2}/{3}.html\">{3}</a></li>".FormatString(suffix, CreateRelativePath(link), namespacePath, element.Name); //suffix + "<li><a href=\"#\">" + element.Name + "</a></li>";
                sb.AppendLine(name);
            }
            return sb.ToString();
        }

        private void WriteHtml(Stream stream, List<Member> members, Element parent, Element root)
        {
            var loader = new Template.TemplateLoader(BaseTemplatePath);

            var constructors = new StringBuilder();
            var methods = new StringBuilder();
            var properties = new StringBuilder();
            foreach (var member in members)
            {
                if (member.Type == MethodType.Method || member.Type == MethodType.Constructor)
                {
                    var methodLoader = new Template.TemplateLoader(BaseMethodTemplate);
                    var parametersStr = ResolveMethodParameter(member);
                    var paramStr = ResolveParameterTable(member, ParameterTableTemplate);
                    var name = member.Type == MethodType.Constructor ? parent.Name : member.Name;
                    var hash = Sha256.GetSha256(name + parametersStr);
                    methodLoader.Assign("MethodHash", hash);
                    methodLoader.Assign("MethodName", name);
                    methodLoader.Assign("MethodParameters", parametersStr);
                    methodLoader.Assign("MethodComment", member.Value);

                    if (!string.IsNullOrEmpty(member.ReturnComment))
                    {
                        methodLoader.Assign("MethodReturnComment", member.ReturnComment);
                        methodLoader.Assign("HasReturn", true.ToString());
                    }
                    if (!string.IsNullOrEmpty(paramStr))
                    {
                        methodLoader.Assign("Parameters", paramStr, true);
                        methodLoader.Assign("HasParameter", true);
                    }

                    if (member.Type == MethodType.Method)
                    {
                        methods.Append(methodLoader.ToString());
                        loader.Assign("HasMethod", true);
                    }
                    else
                    {
                        constructors.Append(methodLoader.ToString());
                        loader.Assign("HasConstructor", true);
                    }
                }
                else if (member.Type == MethodType.Property)
                {
                    var propertyLoader = new Template.TemplateLoader(BasePropertyTemplate);
                    var hash = Sha256.GetSha256(member.Name);
                    propertyLoader.Assign("PropertyHash", hash);
                    propertyLoader.Assign("PropertyName", member.Name);
                    propertyLoader.Assign("PropertyComment", member.Value);
                    properties.Append(propertyLoader.ToString());
                    loader.Assign("HasProperty", true);
                }
            }

            var linkCount = parent.Namespace.NamespaceCount;
            loader.Assign("RelativePath", CreateRelativePath(linkCount));
            loader.Assign("ClassName", "{0} {1}".FormatString(parent.Name, parent.Type.ToString()));
            loader.Assign("ClassComment", "{0}".FormatString(parent.Value));
            loader.Assign("Title", "{0} {1}".FormatString(parent.Name, parent.Type.ToString()));
            loader.Assign("Namespace", parent.Namespace);
            loader.Assign("Menu", CreateMenu(root, linkCount), true);
            loader.Assign("Toc", CreateToc(members, parent), true);
            loader.Assign("ConstructorItems", constructors, true);
            loader.Assign("MethodItems", methods, true);
            loader.Assign("PropertyItems", properties, true);

            var template = loader.ToString();
            var templateBytes = Encoding.UTF8.GetBytes(template);
            stream.Write(templateBytes, 0, templateBytes.Length);
        }
        
        private string CreateToc(List<Member> members, Element parent)
        {
            var toc = new StringBuilder();

            string GetElement(MethodType type, Func<Member, string> func, string typeName)
            {
                var tocElement = new StringBuilder();
                var list = new List<string>();
                foreach (var member in members)
                {
                    if (member.Type == type)
                    {
                        var name = func(member);
                        var hash = Sha256.GetSha256(name);
                        list.Add("    <li><a href=\"#{0}\">{1}</a></li>".FormatString(hash, name));
                    }
                }
                if (list.Count > 0)
                {
					tocElement.AppendFormat("<h3>{0}</h3>\n", typeName);
                    tocElement.AppendLine("<ol>");
                    tocElement.AppendLine(list.GetString());
                    tocElement.AppendLine("</ol>");
                }
                return tocElement.ToString();
            }

            toc.Append(GetElement(MethodType.Constructor, (member) => parent.Name + ResolveMethodParameter(member), "Constructor"));
            toc.Append(GetElement(MethodType.Method, (member) => member.Name + ResolveMethodParameter(member), "Methods"));
            toc.Append(GetElement(MethodType.Property, (member) => member.Name, "Properties"));

            return toc.ToString();
        }

        private static string CreateRelativePath(int link)
        {
            string linkStr = "";
            for (int i = 0; i < link; i++)
                linkStr += "../";
            return linkStr;
        }

        private static string ResolveMethodParameter(Member member)
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

        private static string ResolveParameterTable(Member member, string templatePath)
        {
            var paramSb = new StringBuilder();
            var parameterLoader = new Template.TemplateLoader(templatePath);
            var p1 = member.MethodParameters.Zip(member.Parameters.Keys, (type, name) => new { Type = type, Name = name });
            var p2 = member.Parameters.Values.Zip(p1, (comment, parameter) => new { Comment = comment, Parameter = parameter });
            foreach (var parameter in p2)
            {
                parameterLoader.Assign("Type", ResolveType(parameter.Parameter.Type));
                parameterLoader.Assign("TypeName", parameter.Parameter.Name);
                parameterLoader.Assign("TypeComment", parameter.Comment);
                paramSb.Append(parameterLoader.ToString());
                parameterLoader.Reset();
            }
            return paramSb.ToString();
        }

        private static string ResolveType(string text)
        {
            text = text.Replace("System.Byte", "byte");
            text = text.Replace("System.Int32", "int");
            text = text.Replace("System.Int64", "long");
            text = text.Replace("System.Boolean", "bool");
            text = text.Replace("System.String", "string");

            //text = ResolveGenericsType(text);

            return text.Replace("{", "&lt;").Replace("}", "&gt;");
        }

        private static string ResolveGenericsType(string str, bool isMethod = false)
        {
            string format = isMethod ? "&lt;{0}{1}&gt;" : "{0}{1}";
            var reg = new Regex("``(?<number>[0-9]+)");
            var match = reg.Match(str);
            if (match.Success)
            {
                var num = match.Groups["number"].ToString();
                var full = match.ToString();
                str = str.Replace(full, format.FormatString("T", num));
                return ResolveGenericsType(str);
            }
            return str;
        }

        private void ShowElements(Element element, string suffix = "")
        {
            if (element != null)
            {
                if (element.Namespaces != null)
                {
                    var name = suffix + "> " + element.Name;
                    Console.WriteLine(name);
                    foreach (var elem in element.Namespaces)
                        ShowElements(elem, suffix + " ");
                }
                else
                {
                    var name = suffix + "> " + element.Name;
                    Console.WriteLine(name);
                }

                if (element.Members != null)
                {
                    foreach (var elem in element.Members)
                    {
                        string parametersStr = "";
                        if (elem.Type == MethodType.Method)
                        {
                            parametersStr += "(";
                            var parameters = elem.MethodParameters.Zip(elem.Parameters.Keys, (type, name) => new { Type = type, Name = name });
                            foreach (var tuple in parameters.Select((v, i) => new { Index = i, Value = v }))
                            {
                                if (tuple.Index < elem.Parameters.Count - 1)
                                    parametersStr += "{0} {1}, ".FormatString(tuple.Value.Type, tuple.Value.Name);
                                else
                                    parametersStr += "{0} {1})".FormatString(tuple.Value.Type, tuple.Value.Name);

                            }
                        }
                        Console.WriteLine("{0} > {1}: {2}{3}", suffix, elem.Type, elem.Name, parametersStr);
                    }
                }
            }
        }
    }
}

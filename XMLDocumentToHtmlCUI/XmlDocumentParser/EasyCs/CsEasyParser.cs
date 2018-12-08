using CommonCoreLib.File;
using CommonExtensionLib.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    public enum ClassType
    {
        Class,
        Interface,
        Enum,
        Struct,
        Method
    }

    public class ClassInfo
    {
        public Accessibility Accessibility { get; set; }
        public ClassType ClassType { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Inheritance { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CsEasyParser
    {

        private readonly Dictionary<string, ClassInfo> classMap = new Dictionary<string, ClassInfo>();
        private readonly Dictionary<string, ClassInfo> methodMap = new Dictionary<string, ClassInfo>();

        public void Parse()
        {
            var filepaths = GetCsFiles(@"D:\Develop\Git\XMLDocumentToHtml\XMLDocumentToHtmlCUI\XMLDocumentToHtmlCUI\bin\Debug\src");
            foreach (var filename in filepaths)
            {
                var text = File.ReadAllText(filename.Item2).Replace("\r\n", "\r").Replace("\r", "\n");
                text = RemoveComments(text);

                var namespaceItem = GetNamespace(text);

                RoslynAnalyze(text, filename.Item1);
            }
        }

        public void RoslynAnalyze(string code, string parent)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("sample",
                syntaxTrees: new[] { tree },
                references: new[] { MetadataReference.CreateFromFile(parent) });
            var semanticModel = compilation.GetSemanticModel(tree);
            var classSyntaxArray = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var inSyntaxArray = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var enumSyntaxArray = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();
            var structSyntaxArray = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>();
            var methodSyntaxArray = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
            var constructorSyntaxArray = tree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            var propertySyntaxArray = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>();
            var fieldSyntaxArray = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();
            
            void PutDeclaration(Dictionary<string, ClassInfo> dic, IEnumerable<SyntaxNode> syntaxNodes, ClassType classType)
            {
                foreach (var syntax in syntaxNodes)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    var key = ConvertGenerics(symbol.ToString()).Replace(" ", "");
                    var fullClassName = symbol.ToString();
                    var namespaceName = symbol.ContainingSymbol.ToString();
                    dic.Put(key, new ClassInfo()
                    {
                        FullName = fullClassName,
                        Name = fullClassName.Replace("{0}.".FormatString(namespaceName), ""),
                        Accessibility = symbol.DeclaredAccessibility,
                        ClassType = classType,
                        IsStatic = symbol.IsStatic,
                        IsSealed = symbol.IsSealed,
                        IsAbstract = symbol.IsAbstract,
                    });
                }
            }
            
                PutDeclaration(classMap, classSyntaxArray, ClassType.Class);
                PutDeclaration(classMap, inSyntaxArray, ClassType.Interface);
                PutDeclaration(classMap, enumSyntaxArray, ClassType.Enum);
                PutDeclaration(classMap, structSyntaxArray, ClassType.Struct);
                PutDeclaration(methodMap, methodSyntaxArray, ClassType.Method);
                PutDeclaration(methodMap, constructorSyntaxArray, ClassType.Method);
        }

        public void AddAttributesToElement(Element element)
        {
            if (element != null)
            {
                if (element.Namespaces.Count > 0)
                {
                    foreach (var elem in element.Namespaces)
                    {
                        if (elem.Type == ElementType.Namespace)
                        {
                            AddAttributesToElement(elem);
                        }
                        else
                        {
                            var fullname = elem.Namespace.IsRoot ? elem.Name : "{0}.{1}".FormatString(elem.Namespace, elem.Name);
                            var classInfo = classMap.Get(fullname);
                            if (classInfo != null)
                            {
                                if (classInfo.ClassType == ClassType.Interface)
                                    elem.Type = ElementType.Interface;
                                if (classInfo.ClassType == ClassType.Struct)
                                    elem.Type = ElementType.Struct;
                                if (classInfo.ClassType == ClassType.Enum)
                                    elem.Type = ElementType.Enum;
                                elem.IsAbstract = classInfo.IsAbstract;
                                elem.IsSealed = classInfo.IsSealed;
                                elem.IsStatic = classInfo.IsStatic;
                            }
                            
                            AddAttributesToElement(elem);
                        }
                    }
                }

                if (element.Members.Count > 0)
                {
                    foreach (var method in element.Members)
                    {
                        if (method.Type == MethodType.Method)
                        {
                            var sb = new StringBuilder();
                            foreach (var parameter in method.MethodParameters)
                            {
                                sb.AppendFormat("{0},", GetLastTypeWithoutNamespace(parameter).Replace("{", "<").Replace("}", ">"));
                            }
                            sb.Remove(sb.Length - 1, 1);
                            var fullname = method.Namespace.IsRoot ? method.Name : "{0}.{1}".FormatString(method.Namespace, method.Name);
                            fullname = "{0}({1})".FormatString(fullname, sb.ToString()).Replace(" ", "");
                            var item = methodMap.Get(fullname);
                            if (item != null)
                            {
                                var (methodName, parameterTypes) = SplitMethodNameAndParameter(item.Name);

                                int cnt = parameterTypes.Length > method.MethodParameters.Count ? method.MethodParameters.Count : parameterTypes.Length;
                                for (int i = 0; i < cnt; i++)
                                {
                                    method.MethodParameters[i] = parameterTypes[i].Replace("<", "{").Replace(">", "}");
                                }
                                method.Name = methodName.Replace("<", "{").Replace(">", "}");
                                method.Accessibility = item.Accessibility;
                                if (item.IsStatic)
                                    method.Type = MethodType.Function;
                            }
                            else
                            {
                                Console.WriteLine();
                            }
                        }
                        
                    }
                }
            }
        }

        private static (string methodName, string[] parameterTypes) SplitMethodNameAndParameter(string text)
        {
            var regex = new Regex("(?<name>[\\S ]+)\\((?<parameterStr>[\\S ]+)\\)");
            var match = regex.Match(text);
            if (match.Success)
            {
                var name = match.Groups["name"].ToString();
                var parameterStr = match.Groups["parameterStr"].ToString();
                var parameters = new List<string>();

                var paramRegex = new Regex("[a-zA-Z]+<[a-zA-Z, ]+>[.a-zA-Z]*|[a-zA-Z]+");
                var paramMatch = paramRegex.Match(parameterStr);
                while (paramMatch.Success)
                {
                    parameters.Add(paramMatch.ToString());
                    paramMatch = paramMatch.NextMatch();
                }

                return (name, parameters.ToArray());
            }
            return (text, new string[0]);
        }

        /// <summary>
        /// System.Int32 => Int32
        /// System.Action{System.Int32} => Action{Int32}
        /// </summary>
        private static string GetLastTypeWithoutNamespace(string text)
        {
            var regex = new Regex("[\\S]+\\.(?<genericType>([\\S]+)\\{[\\S]+\\}[\\S]*)|[\\S]+\\.(?<normalType>[\\S]+)");
            var match = regex.Match(text);
            if (match.Success)
            {
                var genericType = match.Groups["genericType"].ToString();
                var normalType = match.Groups["normalType"].ToString();

                if (!string.IsNullOrEmpty(genericType))
                {
                    var internalRegex = new Regex("{(?<internalType>[\\S]+)}");
                    var internalMatch = internalRegex.Match(genericType);
                    if (internalMatch.Success)
                    {
                        var internalType = internalMatch.Groups["internalType"].ToString();
                        var last = internalType.Split('.').Last();
                        return genericType.Replace(internalType, last);
                    }
                    return genericType;
                }
                return normalType;
            }
            return text.Split('.').Last();
        }

        /// <summary>
        /// Get{K, V}(Dictionary{K, V}, K, V) => Get``2(Dictionary{``0, ``1}, ``0, ``1)
        /// </summary>
        private static string ConvertGenerics(string text)
        {
            var regex = new Regex("(?<methodName>[\\S]+)<(?<genericsTypes>[a-zA-Z ,]+)>\\((?<arguments>[\\S ]+)\\)");
            var match = regex.Match(text);
            if (match.Success)
            {
                var sb = new StringBuilder();

                var methodName = match.Groups["methodName"].ToString();
                var genericsTypesStr = match.Groups["genericsTypes"].ToString().Replace(" ", "");
                var genericsTypes = genericsTypesStr.Split(',');

                sb.AppendFormat("{0}``{1}(", methodName, genericsTypes.Length);

                var genericsMap = new Dictionary<string, string>();
                var genericsSb = new StringBuilder();
                for (int i = 0; i < genericsTypes.Length; i++)
                {
                    genericsSb.AppendFormat("``{0},", i);
                    genericsMap.Put(genericsTypes[i], "``{0}".FormatString(i));
                }
                genericsSb.Remove(genericsSb.Length - 1, 1);

                var argumentsStr = match.Groups["arguments"].ToString().Replace(" ", "");
                var argumentRegex = new Regex("(?<notGenericsType>[\\S]+<[\\S ]+>[\\S]*)|(?<genericsType>[a-zA-Z]+)", RegexOptions.Multiline);
                var argMatch = argumentRegex.Match(argumentsStr);
                while (argMatch.Success)
                {
                    var notGenericsType = argMatch.Groups["notGenericsType"].ToString();
                    var genericsType = argMatch.Groups["genericsType"].ToString();

                    if (!string.IsNullOrEmpty(notGenericsType))
                    {
                        sb.AppendFormat("{0},", notGenericsType.Replace(genericsTypesStr, genericsSb.ToString()));
                    }
                    else if (!string.IsNullOrEmpty(genericsType))
                    {
                        var convertedType = genericsMap.Get(genericsType, genericsType);
                        sb.AppendFormat("{0},", convertedType);
                    }
                    argMatch = argMatch.NextMatch();
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(")");

                return sb.ToString();
            }
            return text;
        }

        private Tuple<string, string>[] GetCsFiles(string csprojParentPath)
        {
            var list = new List<Tuple<string, string>>();
            var filepaths = DirectorySearcher.GetAllFiles(csprojParentPath, "*.csproj");
            foreach (var file in filepaths)
            {
                var reader = new XmlWrapper.Reader();
                reader.LoadFromFile(file);
                reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                var parent = "{0}/".FormatString(Path.GetDirectoryName(file));
                var includes = MergeParentPath(reader.GetAttributes("Include", "/ns:Project/ns:ItemGroup/ns:Compile"), parent);
                foreach (var include in includes)
                {
                    list.Add(new Tuple<string, string>(file, include));
                }
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

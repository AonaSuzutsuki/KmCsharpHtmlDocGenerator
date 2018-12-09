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
        Delegate,
        Method
    }

    public class ClassInfo
    {
        public string Id { get; set; }
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

            IEnumerable<MetadataReference> references = new[]{
                //microlib.dll
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                //System.dll
                MetadataReference.CreateFromFile(typeof(System.Collections.ObjectModel.ObservableCollection<>).Assembly.Location),
                //System.Core.dll
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            };

            var compilation = CSharpCompilation.Create("sample",
                syntaxTrees: new[] { tree },
                references: references);
            var semanticModel = compilation.GetSemanticModel(tree);
            var classSyntaxArray = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var inSyntaxArray = tree.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var enumSyntaxArray = tree.GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>();
            var structSyntaxArray = tree.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>();
            var delegateSyntaxArray = tree.GetRoot().DescendantNodes().OfType<DelegateDeclarationSyntax>();
            var methodSyntaxArray = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
            var constructorSyntaxArray = tree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            var propertySyntaxArray = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>();
            var fieldSyntaxArray = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();
            
            void PutDeclaration(Dictionary<string, ClassInfo> dic, IEnumerable<SyntaxNode> syntaxNodes, ClassType classType)
            {
                foreach (var syntax in syntaxNodes)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    if (syntax is ClassDeclarationSyntax)
                    {
                        var baseSyntax = (syntax as ClassDeclarationSyntax).BaseList.Types.First();
                        var sym = semanticModel.GetDeclaredSymbol(baseSyntax.Type);
                        var baseId = baseSyntax.Type.ToString();
                    }
                    var id = symbol.GetDocumentationCommentId();
                    var fullClassName = symbol.ToString();
                    var namespaceName = symbol.ContainingSymbol.ToString();
                    dic.Put(id, new ClassInfo()
                    {
                        Id = id,
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
            PutDeclaration(classMap, delegateSyntaxArray, ClassType.Delegate);
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
                                else if (classInfo.ClassType == ClassType.Delegate)
                                    elem.Type = ElementType.Delegate;
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
                            var item = methodMap.Get(method.Id);
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

                var paramRegex = new Regex("[a-zA-Z.]+<[a-zA-Z, ]+>[.a-zA-Z]*|[a-zA-Z]+");
                var paramMatch = paramRegex.Match(parameterStr);
                while (paramMatch.Success)
                {
                    parameters.Add(paramMatch.ToString());
                    paramMatch = paramMatch.NextMatch();
                }

                return (name, parameters.ToArray());
            }
            return (text.Replace("(", "").Replace(")", ""), new string[0]);
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

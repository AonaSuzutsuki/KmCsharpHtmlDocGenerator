using CommonCoreLib.File;
using CommonExtensionLib.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentExtensions.Extensions;
using XmlDocumentParser.CsXmlDocument;

namespace XmlDocumentParser.EasyCs
{
    /// <summary>
    /// 
    /// </summary>
    public class CSharpEasyAnalyzer
    {

        private readonly Dictionary<string, ClassInfo> classMap = new Dictionary<string, ClassInfo>();
        private readonly Dictionary<string, ClassInfo> methodMap = new Dictionary<string, ClassInfo>();

        public void Parse()
        {
            var (csFilePathArray, referenceArray) = GetCsFiles(@"src");
            var syntaxTrees = new List<SyntaxTree>();
            foreach (var filename in csFilePathArray)
            {
                var text = File.ReadAllText(filename).Replace("\r\n", "\r").Replace("\r", "\n");
                text = RemoveComments(text);

                var namespaceItem = GetNamespace(text);

                syntaxTrees.Add(CSharpSyntaxTree.ParseText(text));
            }

            var metadataReferences = new List<MetadataReference>();
            foreach (var reference in referenceArray)
                metadataReferences.Add(MetadataReference.CreateFromFile(reference.Location));

            IEnumerable<MetadataReference> references = new[]{
                //microlib.dll
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                //System.dll
                MetadataReference.CreateFromFile(typeof(System.Collections.ObjectModel.ObservableCollection<>).Assembly.Location),
                //System.Core.dll
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            };
            var compilation = CSharpCompilation.Create("sample", syntaxTrees, metadataReferences);

            foreach (var tree in syntaxTrees)
            {
                RoslynAnalyze(tree, compilation);
            }
        }

        public void RoslynAnalyze(SyntaxTree tree, CSharpCompilation compilation)
        {
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
                    //if (syntax is ClassDeclarationSyntax)
                    //{
                    //    var baseSyntax = (syntax as ClassDeclarationSyntax).BaseList.Types.First();
                    //    var sym = semanticModel.GetDeclaredSymbol(baseSyntax.Type);
                    //    var baseId = baseSyntax.Type.ToString();
                    //}
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
                            var classInfo = classMap.Get(elem.Id);
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

        private (string[] csFilePathArray, Assembly[] referenceArray) GetCsFiles(string csprojParentPath)
        {
            var csFilePathList = new List<string>();
            var assemblyNameMap = new Dictionary<string, Assembly>();

            var filepaths = DirectorySearcher.GetAllFiles(csprojParentPath, "*.csproj");
            foreach (var file in filepaths)
            {
                var reader = new XmlWrapper.Reader();
                reader.LoadFromFile(file);
                reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                var parent = "{0}/".FormatString(Path.GetDirectoryName(file));
                var includes = MergeParentPath(reader.GetAttributes("Include", "/ns:Project/ns:ItemGroup/ns:Compile"), parent);
                csFilePathList.AddRange(includes);

                var targetFramework = GetTargetFramework(reader);
                var hintPaths = reader.GetValues("/ns:Project/ns:ItemGroup/ns:Reference/ns:HintPath", false);
                var references = reader.GetAttributes("Include", "/ns:Project/ns:ItemGroup/ns:Reference");
                foreach (var hintPath in hintPaths)
                {
                    var relativePath = Path.Combine(parent, CommonPath.PathUtils.ResolvePathSeparator(hintPath));
                    var absolutePath = Path.GetFullPath(relativePath);
                    if (File.Exists(absolutePath))
                    {
                        var assembly = Assembly.LoadFile(absolutePath);
                        assemblyNameMap.Put(assembly.GetName().Name, assembly);
                    }
                }
                foreach (var reference in references)
                {
                    var referenceName = reference.Split(',').First();
                    if (!assemblyNameMap.ContainsKey(referenceName))
                    {
                        //try
                        //{
                        //  var systemAssemblyDir = "{0}/{1}-api".FormatString("/Library/Frameworks/Mono.framework/Versions/5.10.1/lib/mono", targetFramework);
                        //  var assembly = Assembly.LoadFrom("{0}/{1}.dll".FormatString(systemAssemblyDir, referenceName));
                        //  assemblyNameMap.Add(reference, assembly);
                        //}
                        //catch (BadImageFormatException)
                        //{
                        //  var assemblyPath = DirectorySearcher.GetAllFiles("/Library/Frameworks/Mono.framework/Versions/5.10.1/lib/mono/gac", "{0}.dll".FormatString(reference)).Last();
                        //  var assembly = Assembly.LoadFile(assemblyPath);
                        //  assemblyNameMap.Add(reference, assembly);
                        //}
                        //catch
                        //{
                        //  Console.WriteLine();
                        //}

                        try
                        {
                            //C:\Windows\assembly\GAC_MSIL
                            var assemblyPath = GetSystemAssemblyPath(targetFramework, reference);
                            var assembly = Assembly.LoadFrom(assemblyPath);
                            assemblyNameMap.Add(reference, assembly);
                        }
                        catch
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }
            return (csFilePathList.ToArray(), assemblyNameMap.Values.ToArray());
        }

        private string GetSystemAssemblyPath(string targetFramework, string reference)
        {
            var systemAssemblyDirList = new List<string>
            {
                "{0}{1}".FormatString(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v", targetFramework),
                @"C:\Windows\assembly\GAC_MSIL",
                "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/gac",
                "{0}/{1}-api".FormatString("/Library/Frameworks/Mono.framework/Versions/Current/lib/mono", targetFramework),
            };

            foreach (var systemAssemblyDir in systemAssemblyDirList)
            {
                if (Directory.Exists(systemAssemblyDir))
                {
                    var assemblyPathArray = DirectorySearcher.GetAllFiles(systemAssemblyDir, "{0}.dll".FormatString(reference));
                    if (assemblyPathArray.Length > 0)
                        return assemblyPathArray.Last();
                }
            }
            return null;

            //string assemblyPath = null;
            //try
            //{
            //  var systemAssemblyDir = "{0}{1}".FormatString(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v", targetFramework);
            //  var assemblyPathArray = DirectorySearcher.GetAllFiles(systemAssemblyDir, "{0}.dll".FormatString(reference));
            //  assemblyPath = assemblyPathArray.Length > 0 ? assemblyPathArray.Last() : null;
            //  if (assemblyPath == null)
            //  {
            //      systemAssemblyDir = @"C:\Windows\assembly\GAC_MSIL";
            //      assemblyPathArray = DirectorySearcher.GetAllFiles(systemAssemblyDir, "{0}.dll".FormatString(reference));
            //      assemblyPath = assemblyPathArray.Length > 0 ? assemblyPathArray.Last() : null;
            //  }
            //}
            //catch { }

            //return assemblyPath;
        }

        private string GetTargetFramework(XmlWrapper.Reader reader)
        {
            var version = reader.GetValue("/ns:Project/ns:PropertyGroup/ns:TargetFrameworkVersion", false);
            var regex = new Regex("[0-9.]+");
            var match = regex.Match(version);
            if (match.Success)
            {
                return match.ToString();
            }
            return null;
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

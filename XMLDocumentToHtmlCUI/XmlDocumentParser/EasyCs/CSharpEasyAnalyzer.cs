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
using XmlDocumentParser.MethodParameter;

namespace XmlDocumentParser.EasyCs
{
    /// <summary>
    /// Be analyzed C# code by Roslyn and add attributes into elements of C# XML Document.
    /// </summary>
    public class CSharpEasyAnalyzer
    {

        private readonly Dictionary<string, ClassInfo> classMap = new Dictionary<string, ClassInfo>();
        private readonly Dictionary<string, ClassInfo> methodMap = new Dictionary<string, ClassInfo>();

        /// <summary>
        /// Parse C# codes in csproj files.
        /// </summary>
        /// <param name="csProjDirPath">Directory path included csproj file.</param>
        public void Parse(string csProjDirPath = "src")
        {
            if (!Directory.Exists(csProjDirPath))
                return;

            var (csFilePathArray, referenceArray) = GetCsFiles(csProjDirPath);
            var syntaxTrees = new List<SyntaxTree>();
            foreach (var filename in csFilePathArray)
            {
                var text = File.ReadAllText(filename).Replace("\r\n", "\r").Replace("\r", "\n");
                text = RemoveComments(text);

                var namespaceItem = GetNamespace(text);

                syntaxTrees.Add(CSharpSyntaxTree.ParseText(text));
            }

            var metadataReferences = new List<MetadataReference>
            {
                { referenceArray, (item) => MetadataReference.CreateFromFile(item.Location) }
            };

            //IEnumerable<MetadataReference> references = new[]{
            //             //microlib.dll
            //             MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            //             //System.dll
            //             MetadataReference.CreateFromFile(typeof(System.Collections.ObjectModel.ObservableCollection<>).Assembly.Location),
            //             //System.Core.dll
            //             MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            //};
            var compilation = CSharpCompilation.Create("sample", syntaxTrees, metadataReferences);

            foreach (var tree in syntaxTrees)
            {
                RoslynAnalyze(tree, compilation);
            }
        }

        /// <summary>
        /// Adds the attributes on C# codes to <see cref="Element"/>.
        /// </summary>
        /// <param name="element">Added <see cref="Element"/>.</param>
        public void AddAttributesToElement(Element element)
        {
            if (element == null)
                return;

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

                            elem.Name = classInfo.FullName;
                            elem.IsAbstract = classInfo.IsAbstract;
                            elem.IsSealed = classInfo.IsSealed;
                            elem.IsStatic = classInfo.IsStatic;

                            elem.InheritanceList.Add(classInfo.Inheritance, (item) => new Element()
                            {
                                Accessibility = item.Accessibility,
                                Id = item.Id,
                                Name = item.FullName,
                                Namespace = item.Namespace,
                                Type = ElementType.Class
                            });
                        }

                        AddAttributesToElement(elem);
                    }
                }
            }

            if (element.Members.Count > 0)
            {
                foreach (var method in element.Members)
                {
                    if (method.Type == MethodType.Method || method.Type == MethodType.Constructor)
                    {
                        var item = methodMap.Get(method.Id);
                        if (item != null)
                        {
                            method.ParameterTypes = new List<string>
                                {
                                    { item.ParameterTypes, (_item) => _item }
                                };

                            if (item.IsStatic)
                                method.Type = MethodType.Function;
                            if (item.IsExtensionMethod)
                                method.Type = MethodType.ExtensionMethod;

                            method.Difinition = ConvertToDefinition(item, method);
                            method.Name = item.Name;
                            method.Accessibility = item.Accessibility;
                            method.ReturnType = item.ReturnType;
                        }
                    }

                    if (method.Type == MethodType.Property)
                    {
                        var item = methodMap.Get(method.Id);
                        if (item != null)
                        {
                            method.Difinition = ConvertToDefinition(item, method);
                            method.Name = item.Name;
                            method.Accessibility = item.Accessibility;
                            method.ReturnType = item.ReturnType;
                        }
                    }

                }
            }
        }
        
		private static string ConvertToDefinition(ClassInfo classInfo, Member member)
		{
            var sb = new StringBuilder();

			if (classInfo.ClassType == ClassType.Method || classInfo.ClassType == ClassType.Constructor)
            {
                sb.AppendFormat("{0} ", classInfo.Accessibility.ToString().ToLower());

                if (classInfo.IsOverride)
                    sb.Append("override ");
                if (classInfo.IsVirtual)
                    sb.Append("virtual ");
                if (classInfo.IsStatic)
                    sb.Append("static ");
                if (classInfo.IsAsync)
                    sb.Append("async ");
                if (classInfo.IsExtern)
                    sb.Append("extern ");

                if (classInfo.ClassType == ClassType.Method)
                    sb.AppendFormat("{0} ", classInfo.ReturnType);
                sb.AppendFormat("{0}", classInfo.Name);
				sb.AppendFormat("{0};", MethodParameterConverter.CreateMethodParameterText(member, (item) => item));
            }
            else if (classInfo.ClassType == ClassType.Property)
            {
                sb.AppendFormat("{0} ", classInfo.Accessibility.ToString().ToLower());
                sb.AppendFormat("{0} ", classInfo.ReturnType);
                sb.AppendFormat("{0} {{ ", classInfo.Name);

                foreach (var accessors in classInfo.Accessors)
                {
                    if (accessors.Accessibility == Accessibility.Public)
                        sb.AppendFormat("{0}; ", accessors.Name);
                    else if (accessors.Accessibility != Accessibility.Private)
                        sb.AppendFormat("{0} {1}; ", accessors.Accessibility.ToString().ToLower(), accessors.Name);
                }

                sb.AppendFormat("}}");
            }

			var tree = CSharpSyntaxTree.ParseText(sb.ToString());
            
			return MethodParameterConverter.ResolveGenericsTypeToHtml(sb.ToString());
		}

		private static string ConvertSyntaxHighlightText(string defCode)
		{
			//(?<accessibility>[a-z ]+)[\s]+(?<returnType>[a-zA-Z0-9\[\]<>,\(\) ]+)[\s]+(?<methodName>[a-zA-Z0-9]+)\((?<arguments>[a-zA-Z0-9<>,. ]*)\);
			return null;
		}

        private void RoslynAnalyze(SyntaxTree tree, CSharpCompilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            var nodes = tree.GetRoot().DescendantNodes();
            var classSyntaxArray = nodes.OfType<ClassDeclarationSyntax>();
            var inSyntaxArray = nodes.OfType<InterfaceDeclarationSyntax>();
            var enumSyntaxArray = nodes.OfType<EnumDeclarationSyntax>();
            var structSyntaxArray = nodes.OfType<StructDeclarationSyntax>();
            var delegateSyntaxArray = nodes.OfType<DelegateDeclarationSyntax>();
            var methodSyntaxArray = nodes.OfType<MethodDeclarationSyntax>();
            var constructorSyntaxArray = nodes.OfType<ConstructorDeclarationSyntax>();
            var propertySyntaxArray = nodes.OfType<PropertyDeclarationSyntax>();
            var fieldSyntaxArray = nodes.OfType<FieldDeclarationSyntax>();
            
            void PutDeclaration(Dictionary<string, ClassInfo> dic, IEnumerable<SyntaxNode> syntaxNodes, ClassType classType)
            {
                foreach (var syntax in syntaxNodes)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(syntax);
                    var classInfo = CreateClassInfo(symbol, classType);

                    if (syntax is ClassDeclarationSyntax)
                    {
                        var classSyntax = (syntax as ClassDeclarationSyntax);
                        if (classSyntax.BaseList != null)
                        {
                            var baseTypes = classSyntax.BaseList.Types;
                            foreach (var baseSyntax in baseTypes)
                            {
                                var symbolInfo = semanticModel.GetSymbolInfo(baseSyntax.Type);
                                var sym = symbolInfo.Symbol;
                                var info = CreateClassInfo(sym, ClassType.Inheritance);
                                classInfo.Inheritance.Add(info);
                            }
                        }
                    }

                    if (syntax is PropertyDeclarationSyntax)
                    {
                        var propSyntax = (syntax as PropertyDeclarationSyntax);
                        var symbolInfo = semanticModel.GetSymbolInfo(propSyntax.Type);
                        var sym = symbolInfo.Symbol;
                        var accessors = propSyntax.AccessorList.Accessors;
                        classInfo.Accessors.Add(accessors, (item) =>
                        {
                            var accessibility = Accessibility.Public;
                            var keyword = item.Keyword.ToString();
                            if (item.Modifiers.Count > 0)
                            {
                                var msym = semanticModel.GetDeclaredSymbol(item);
                                accessibility = msym.DeclaredAccessibility;
                            }

                            return new ClassInfo()
                            {
                                Accessibility = accessibility,
                                Name = keyword
                            };
                        });
                        classInfo.ReturnType = sym.ToDisplayString();
                    }

                    dic.Put(classInfo.Id, classInfo);
                }
            }

            PutDeclaration(classMap, classSyntaxArray, ClassType.Class);
            PutDeclaration(classMap, inSyntaxArray, ClassType.Interface);
            PutDeclaration(classMap, enumSyntaxArray, ClassType.Enum);
            PutDeclaration(classMap, structSyntaxArray, ClassType.Struct);
            PutDeclaration(classMap, delegateSyntaxArray, ClassType.Delegate);
            PutDeclaration(methodMap, methodSyntaxArray, ClassType.Method);
            PutDeclaration(methodMap, constructorSyntaxArray, ClassType.Constructor);
            PutDeclaration(methodMap, propertySyntaxArray, ClassType.Property);
        }

        private static ClassInfo CreateClassInfo(ISymbol symbol, ClassType classType)
        {
            var id = symbol.GetDocumentationCommentId();
            var fullClassName = symbol.ToString();
            var namespaceName = symbol.ContainingSymbol.ToString();
            var nameWithParameter = fullClassName.Replace("{0}.".FormatString(namespaceName), "");
            (string methodName, string[] parameterTypes) = SplitMethodNameAndParameter(nameWithParameter);

            var classInfo = new ClassInfo
            {
                Id = id,
                FullName = nameWithParameter,
                Namespace = new NamespaceItem(namespaceName),
                Name = methodName,
                Accessibility = symbol.DeclaredAccessibility,
                ClassType = classType,
                IsStatic = symbol.IsStatic,
                IsSealed = symbol.IsSealed,
                IsAbstract = symbol.IsAbstract,
                IsExtern = symbol.IsExtern,
                IsVirtual = symbol.IsVirtual,
                IsOverride = symbol.IsOverride,
            };

            if (symbol is IMethodSymbol)
            {
                foreach (var type in ((IMethodSymbol)symbol).Parameters)
                {
                    classInfo.ParameterTypes.Add(type.ToString());
                }

                var returnType = ((IMethodSymbol)symbol).ReturnType;
                classInfo.IsAsync = ((IMethodSymbol)symbol).IsAsync;
                classInfo.ReturnType = returnType.ToString();
				classInfo.IsExtensionMethod = ((IMethodSymbol)symbol).IsExtensionMethod;
            }

            return classInfo;
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

				var paramRegex = new Regex("[a-zA-Z0-9.]+<[a-zA-Z0-9,. ]+>[.a-zA-Z0-9]*|[.a-zA-Z0-9]+");
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
                    var relativePath = Path.Combine(parent, CommonPath.PathUtils.UnifiedPathSeparator(hintPath));
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
                        try
                        {
                            var assemblyPath = GetSystemAssemblyPath(targetFramework, reference);
                            var assembly = Assembly.LoadFrom(assemblyPath);
                            assemblyNameMap.Add(reference, assembly);
                            Console.WriteLine("found \"{0}\" assembly.", reference);
                        }
                        catch
                        {
                            Console.WriteLine("not found \"{0}\" assembly.", reference);
                        }
                    }
                }
            }

            if (!assemblyNameMap.ContainsKey("mscorlib"))
                assemblyNameMap.Add("mscorlib", typeof(object).Assembly);

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
                "/usr/lib/mono/gac",
                "{0}/{1}-api".FormatString("/usr/lib/mono", targetFramework),
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
                retList[i] = CommonPath.PathUtils.UnifiedPathSeparator(parent + retList[i]);
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

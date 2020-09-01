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
using XmlDocumentParser.Csproj;
using XmlDocumentParser.CsXmlDocument;
using XmlDocumentParser.MethodParameter;

namespace XmlDocumentParser.EasyCs
{
    /// <summary>
    /// Be analyzed C# code by Roslyn and add attributes into elements of C# XML Document.
    /// </summary>
    public class CSharpEasyAnalyzer
    {

        #region Events
        /// <summary>
        /// CSharp parse progress event arguments.
        /// </summary>
        public class CSharpParseProgressEventArgs : EventArgs
        {
            /// <summary>
            /// Parse type.
            /// </summary>
            public enum ParseType
            {
                /// <summary>
                /// The syntactic analysis.
                /// </summary>
                SyntacticAnalysis,
                /// <summary>
                /// The code analysis.
                /// </summary>
                CodeAnalysis
            }

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
            /// <see cref="T:XmlDocumentParser.EasyCs.CSharpEasyAnalyzer.CSharpParseProgressEventArgs"/> class.
            /// </summary>
            /// <param name="type">Type.</param>
            /// <param name="max">Max.</param>
            /// <param name="current">Current.</param>
            /// <param name="filename">Filename.</param>
			public CSharpParseProgressEventArgs(ParseType type, int max, int current, string filename)
            {
                Type = type;
                Max = max;
                Current = current;
                Percentage = (int)((double)Current / Max * 100);
                Filename = filename;
            }
        }

        /// <summary>
        /// CSharp parse progress event handler.
        /// </summary>
		public delegate void CSharpParseProgressEventHandler(object sender, CSharpParseProgressEventArgs eventArgs);

        /// <summary>
        /// Occurs when analysis progress.
        /// </summary>
        public event CSharpParseProgressEventHandler AnalysisProgress;
        /// <summary>
        /// Occurs when code analysis completed.
        /// </summary>
		public event EventHandler CodeAnalysisCompleted;
        #endregion

        private readonly Dictionary<string, ClassInfo> classMap = new Dictionary<string, ClassInfo>();
        private readonly Dictionary<string, ClassInfo> methodMap = new Dictionary<string, ClassInfo>();

        /// <summary>
        /// Parse C# codes in csproj files.
        /// </summary>
        /// <param name="csProjDirPath">Directory path included csproj file.</param>
        /// <param name="compileType">Type of project.</param>
        public void Parse(CsprojAnalyzer csprojAnalyzer)
        {
            if (!Directory.Exists(csprojAnalyzer.CsprojParentPath))
                return;

            var csFilesInfo = csprojAnalyzer.GetCsFiles();
            var syntaxTrees = new List<SyntaxTree>();
            int index = 0;
            foreach (var tuple in csFilesInfo.SourceFiles.Select((v, i) => new { Value = v, Index = i }))
            {
                var text = File.ReadAllText(tuple.Value).UnifiedNewLine();

                var namespaceItem = GetNamespace(text);

				syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, CSharpParseOptions.Default, tuple.Value));

                AnalysisProgress?.Invoke(this, new CSharpParseProgressEventArgs(
                    CSharpParseProgressEventArgs.ParseType.SyntacticAnalysis, csFilesInfo.SourceFiles.Length * 2, ++index, tuple.Value));
            }

            var metadataReferences = new List<MetadataReference>
            {
                { csFilesInfo.References, (item) => MetadataReference.CreateFromFile(item.Location) }
            };

            var compilation = CSharpCompilation.Create("sample", syntaxTrees, metadataReferences);

            foreach (var tuple in syntaxTrees.Select((v, i) => new { Value = v, Index = i }))
            {
                RoslynAnalyze(tuple.Value, compilation);

                AnalysisProgress?.Invoke(this, new CSharpParseProgressEventArgs(
                    CSharpParseProgressEventArgs.ParseType.CodeAnalysis, csFilesInfo.SourceFiles.Length * 2, ++index, tuple.Value.FilePath));
            }

			CodeAnalysisCompleted?.Invoke(this, new EventArgs());
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
                            method.ParameterTypes = new List<TypeInfo>
                                {
                                    { item.ParameterTypes, (_item) => _item }
                                };

                            if (item.IsStatic)
                                method.Type = MethodType.Function;
                            if (item.IsExtensionMethod)
                                method.Type = MethodType.ExtensionMethod;

                            method.ClassInformation = item;
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
                            method.ClassInformation = item;
                            method.Name = item.Name;
                            method.Accessibility = item.Accessibility;
                            method.ReturnType = item.ReturnType;
                        }
                    }

                }
            }
        }

        public static TypeInfo CreateParameterInfo(string fullname)
        {
            var names = fullname.Split('.').ToList();
            if (names.Count > 0)
            {
                var name = names.Last();
                var nameSpace = string.Join(".", names.GetRange(0, names.Count - 1));
                return new TypeInfo
                {
                    Name = name,
                    Namespace = nameSpace
                };
            }

            return new TypeInfo();
        }

        private void RoslynAnalyze(SyntaxTree tree, CSharpCompilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            var nodes = tree.GetRoot().DescendantNodes().ToArray();
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

                    if (syntax is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        var classSyntax = classDeclarationSyntax;
                        if (classSyntax.BaseList != null)
                        {
                            var baseTypes = classSyntax.BaseList.Types;
                            foreach (var baseSyntax in baseTypes)
                            {
                                var symbolInfo = semanticModel.GetSymbolInfo(baseSyntax.Type);
                                var sym = symbolInfo.Symbol;
                                if (sym != null)
                                {
                                    var info = CreateClassInfo(sym, ClassType.Inheritance);
                                    classInfo.Inheritance.Add(info);
                                }
                            }
                        }
                    }

                    if (syntax is PropertyDeclarationSyntax declarationSyntax)
                    {
                        var propSyntax = declarationSyntax;
                        var symbolInfo = semanticModel.GetSymbolInfo(propSyntax.Type);
                        var sym = symbolInfo.Symbol;
                        if (propSyntax.AccessorList == null)
                        {
                            classInfo.Accessors.Add(new ClassInfo
                            {
                                Accessibility = Accessibility.Public,
                                Name = "get"
                            });
                        }
                        else
                        {
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

                                return new ClassInfo
                                {
                                    Accessibility = accessibility,
                                    Name = keyword
                                };
                            });
                        }
                        var returnFullname = sym == null ? propSyntax.Identifier.ToString() : sym.ToDisplayString();
                        classInfo.ReturnType = CreateParameterInfo(returnFullname);
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
            var (methodName, _) = SplitMethodNameAndParameter(nameWithParameter);

            //Microsoft.CodeAnalysis.CSharp.Symbols.SourceNamedTypeSymbol a;


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

            if (symbol is IMethodSymbol methodSymbol)
            {
                foreach (var type in methodSymbol.Parameters)
                {
                    var paramFullname = type.ToString();
                    classInfo.ParameterTypes.Add(CreateParameterInfo(paramFullname));
                }

                var returnType = methodSymbol.ReturnType;
                classInfo.IsAsync = methodSymbol.IsAsync;
                classInfo.ReturnType = CreateParameterInfo(returnType.ToString());
				classInfo.IsExtensionMethod = methodSymbol.IsExtensionMethod;
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

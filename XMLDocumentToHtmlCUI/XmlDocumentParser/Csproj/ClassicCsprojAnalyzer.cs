using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommonCoreLib.File;
using CommonExtensionLib.Extensions;
using XmlDocumentExtensions.Extensions;
using XmlDocumentParser.EasyCs;

namespace XmlDocumentParser.Csproj
{
    /// <summary>
    /// Analyzer of classic csproj file.
    /// </summary>
    public class ClassicCsprojAnalyzer : CsprojAnalyzer
    {
        public IEnumerable<string> IgnoreProjectNames;

        public ClassicCsprojAnalyzer(string csprojParentPath) : base(csprojParentPath)
        {

        }

        /// <summary>
        /// Get the C# source files and reference libraries from the classic csproj file.
        /// </summary>
        /// <param name="csprojParentPath">The parent directory where the csproj file is located. Search for the file by performing a recursion search.</param>
        /// <returns>The information about C# source files and reference libraries.</returns>
        public override CsFilesInfo GetCsFiles()
        {
            var csFilePathList = new List<string>();
            var assemblyNameMap = new Dictionary<string, Assembly>();

            var csprojArray = DirectorySearcher.GetAllFiles(CsprojParentPath, "*.csproj");
            csprojArray = RemoveIgnoreProject(csprojArray);
            foreach (var file in csprojArray)
            {
                var reader = new XmlWrapper.Reader();
                reader.LoadFromFile(file);
                reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                var parent = "{0}/".FormatString(Path.GetDirectoryName(file));
                var includes = from x in reader.GetAttributes("Include", "/ns:Project/ns:ItemGroup/ns:Compile")
                    select CommonPath.PathUtils.UnifiedPathSeparator($"{parent}{x}");
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

            return new CsFilesInfo(csFilePathList.ToArray(), assemblyNameMap.Values.ToArray());
        }

        private string[] RemoveIgnoreProject(string[] source)
        {
            if (IgnoreProjectNames == null || !IgnoreProjectNames.Any())
                return source;

            var ignoreSet = new HashSet<string>(IgnoreProjectNames);
            return (from x in source
                    let items = x.UnifiedSystemPathSeparator().Split('/')
                    let projName = Path.GetFileNameWithoutExtension(items.Last())
                    where !ignoreSet.Contains(projName)
                    select x).ToArray();
        }

        /// <summary>
        /// Get the path of the system assembly to match the .net framework version. It returns the most recently found assembly if it cannot be found.
        /// </summary>
        /// <param name="targetFramework">A version of the .net framework to explore.</param>
        /// <param name="reference">Name of the assembly to be searched for.</param>
        /// <returns>The assembly path found. Returns null if not found.</returns>
        protected string GetSystemAssemblyPath(string targetFramework, string reference)
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

        /// <summary>
        /// Get the target framework version.
        /// </summary>
        /// <param name="reader">The XML reader.</param>
        /// <returns>The target framework version</returns>
        protected string GetTargetFramework(XmlWrapper.Reader reader)
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
    }
}

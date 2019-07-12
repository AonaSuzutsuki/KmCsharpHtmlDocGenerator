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
    public class ClassicCsprojAnalyzer : CsprojAnalyzer
    {
        public override CsFilesInfo GetCsFiles(string csprojParentPath, CompileType compileType)
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

        protected override string GetSystemAssemblyPath(string targetFramework, string reference)
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

        protected override string GetTargetFramework(XmlWrapper.Reader reader)
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

        protected override List<string> MergeParentPath(List<string> list, string parent)
        {
            var retList = new List<string>(list);
            for (int i = 0; i < retList.Count; i++)
            {
                retList[i] = CommonPath.PathUtils.UnifiedPathSeparator(parent + retList[i]);
            }
            return retList;
        }
    }
}

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
    /// Provides information about C# source files.
    /// </summary>
    public readonly struct CsFilesInfo
    {
        /// <summary>
        /// C# source files.
        /// </summary>
        public string[] SourceFiles { get; }

        /// <summary>
        /// Assemblies referenced by the C# source.
        /// </summary>
        public Assembly[] References { get; }

        /// <summary>
        /// Initialize CsFilesInfo.
        /// </summary>
        /// <param name="csFilePathArray">C# source files.</param>
        /// <param name="referenceArray">Assemblies referenced by the C# source.</param>
        public CsFilesInfo(string[] csFilePathArray, Assembly[] referenceArray)
        {
            SourceFiles = csFilePathArray;
            References = referenceArray;
        }
    }

    public abstract class CsprojAnalyzer
    {
        public abstract CsFilesInfo GetCsFiles(string csprojParentPath, ProjectType compileType);

        protected abstract string GetSystemAssemblyPath(string targetFramework, string reference);

        protected abstract string GetTargetFramework(XmlWrapper.Reader reader);

        protected abstract List<string> MergeParentPath(List<string> list, string parent);



        public static CsFilesInfo Parse(string csprojParentPath, ProjectType compileType)
        {
            switch (compileType)
            {
                case ProjectType.Xamarin:
                    return new XamarinCsprojAnalyzer().GetCsFiles(csprojParentPath, compileType);
                default:
                    return new ClassicCsprojAnalyzer().GetCsFiles(csprojParentPath, compileType);
            }
        }
    }
}

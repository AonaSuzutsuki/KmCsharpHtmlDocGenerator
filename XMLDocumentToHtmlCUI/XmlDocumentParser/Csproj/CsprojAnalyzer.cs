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

    /// <summary>
    /// Analyzer of csproj file.
    /// </summary>
    public abstract class CsprojAnalyzer
    {
        public string CsprojParentPath { get; }

        public CsprojAnalyzer(string csprojParentPath)
        {
            CsprojParentPath = csprojParentPath;
        }

        /// <summary>
        /// Get the C# source files and reference libraries from the csproj file.
        /// </summary>
        /// <param name="csprojParentPath">The parent directory where the csproj file is located.</param>
        /// <returns></returns>
        public abstract CsFilesInfo GetCsFiles();


        /// <summary>
        /// Parse the csproj file.
        /// </summary>
        /// <param name="csprojParentPath">The parent directory where the csproj file is located.</param>
        /// <param name="compileType">Format of the csproj file to be analyzed. Currently, only classics are supported.</param>
        /// <returns></returns>
        public static CsFilesInfo Parse(string csprojParentPath, ProjectType compileType)
        {
            switch (compileType)
            {
                case ProjectType.Xamarin:
                    return new XamarinCsprojAnalyzer(csprojParentPath).GetCsFiles();
                default:
                    return new ClassicCsprojAnalyzer(csprojParentPath).GetCsFiles();
            }
        }
    }
}

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
    public struct CsFilesInfo
    {
        public string[] SourceFiles { get; }
        public Assembly[] References { get; }

        public CsFilesInfo(string[] csFilePathArray, Assembly[] referenceArray)
        {
            SourceFiles = csFilePathArray;
            References = referenceArray;
        }
    }

    public abstract class CsprojAnalyzer
    {
        public abstract CsFilesInfo GetCsFiles(string csprojParentPath, CompileType compileType);

        protected abstract string GetSystemAssemblyPath(string targetFramework, string reference);

        protected abstract string GetTargetFramework(XmlWrapper.Reader reader);

        protected abstract List<string> MergeParentPath(List<string> list, string parent);



        public static CsFilesInfo Parse(string csprojParentPath, CompileType compileType)
        {
            switch (compileType)
            {
                case CompileType.Xamarin:
                    return new XamarinCsprojAnalyzer().GetCsFiles(csprojParentPath, compileType);
                default:
                    return new ClassicCsprojAnalyzer().GetCsFiles(csprojParentPath, compileType);
            }
        }
    }
}

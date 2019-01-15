using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentParser;
using XmlDocumentParser.CsXmlDocument;
using CommonCoreLib.Crypto;
using XmlDocumentExtensions.Extensions;
using XmlDocumentToHtml.Writer;
using XmlDocumentParser.EasyCs;
using XmlDocumentParser.CommonPath;

namespace XMLDocumentToHtmlCUI
{
    class Program
    {      
        static void Main(string[] args)
        {
            var envParser = new Parser.EnvArgumentParser(args);
            envParser.AddOptionCount("-h", 0);
            envParser.AddOptionCount("-b", 1);
            envParser.AddOptionCount("-o", 1);
            envParser.AddOptionCount("-s", 1);

            envParser.Analyze();
            if (envParser.GetOption("-h") != null)
            {
                ShowHelp();
                return;
            }

            var baseTemplateDir = envParser.GetOption("-b") ?? "BaseTemplate";
            var sourceFilesDir = envParser.GetOption("-s") ?? "src"; // XmlDocumentExtensions.xml  XmlDocumentParser.xml XmlDocumentToHtml.xml
            var inputFiles = envParser.GetValues(); // GetXmlFiles(sourceFilesDir); // envParser.GetValues();
            if (inputFiles.Length < 1)
                inputFiles = GetXmlFiles(sourceFilesDir);
            var outputPath = envParser.GetOutputFilepath() ?? PathUtils.UnifiedPathSeparator("{0}/Root".FormatString(CommonCoreLib.AppInfo.GetAppPath()));

            var (singleDirectoryName, directoryName) = GetSingleDirectoryNameAndDirectoryName(outputPath);

            var root = CsXmlDocumentParser.ParseMultiFiles(inputFiles, singleDirectoryName);
            
            var parser = new CSharpEasyAnalyzer();
            parser.SyntacticAnalysisProgress += Parser_SyntacticAnalysisProgress;
            parser.CodeAnalysisProgress += Parser_CodeAnalysisProgress;
			parser.CodeAnalysisCompleted += Parser_CodeAnalysisCompleted;

			Console.WriteLine("Start C# code analysis.");
            parser.Parse(sourceFilesDir);
            parser.AddAttributesToElement(root);

            var converter = new CsXmlToHtmlWriter(root) { TemplateDir = baseTemplateDir };
            converter.WriteToDisk(directoryName);
        }

		private static void Parser_CodeAnalysisCompleted(object sender, EventArgs e)
		{
			Console.WriteLine("Completed C# code analysis.");
        }

        private static void Parser_CodeAnalysisProgress(object sender, CSharpEasyAnalyzer.CSharpParseProgressEventArgs eventArgs)
        {
			Console.WriteLine(" Code Analysis {0,3:d}%\t{1}", eventArgs.Percentage, eventArgs.Filename);
        }

        private static void Parser_SyntacticAnalysisProgress(object sender, CSharpEasyAnalyzer.CSharpParseProgressEventArgs eventArgs)
        {
			Console.WriteLine(" Syntactic Analysis {0,3:d}%\t{1}", eventArgs.Percentage, eventArgs.Filename);
        }

        static string[] GetXmlFiles(string sourceDir)
        {
            var files = Directory.GetFiles(sourceDir, "*.xml");
            return files;
        }

        static void ShowHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Convert C# XML Document to HTML.");
            sb.AppendLine("Copyright (C) 2018 - 2019 Aona Suzutsuki.");
            sb.AppendFormat("\n{0}\t{1}\n", "-h", "Show help.");
            sb.AppendFormat("{0}\t{1}\n", "-b", "Specify the directory where the template file is stored. If you specify this, you can output with your own template.");
            sb.AppendFormat("{0}\t{1}\n", "-o", "Change the directory path of the output destination.");
            sb.AppendFormat("{0}\t{1}\n", "-s", "csproj file and Source codes directory.");
            Console.WriteLine(sb);
        }

        /// <summary>
        /// Get the directory name and directory names string.
        /// </summary>
        /// <param name="path">Target path.</param>
        /// <returns>The directory name and directory names string.</returns>
        static (string singleDirectoryName, string directoryName) GetSingleDirectoryNameAndDirectoryName(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            var singleDirectoryName = PathUtils.GetSingleDirectoryName(path);
            var systemDirectoryName = Path.GetDirectoryName(path);
            string directoryName = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(systemDirectoryName))
                directoryName = "{0}{1}".FormatString(systemDirectoryName, Path.DirectorySeparatorChar);
            return (singleDirectoryName, directoryName);
        }
    }
}

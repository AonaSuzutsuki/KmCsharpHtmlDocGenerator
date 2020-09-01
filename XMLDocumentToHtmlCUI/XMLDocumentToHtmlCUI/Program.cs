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
using XmlDocumentParser.Csproj;

namespace XMLDocumentToHtmlCUI
{
    /// <summary>
    /// Generates an HTML document from C# source code.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var envParser = new Parser.EnvArgumentParser();
            envParser.AddOptionCount("-h", 0);
            envParser.AddOptionCount("-b", 1);
            envParser.AddOptionCount("-o", 1);
            envParser.AddOptionCount("-s", 1);
            envParser.AddOptionCount("-t", 1);
            envParser.AddOptionCount("-i", 1);

            envParser.Analyze(args);
            if (envParser.GetOption("-h") != null)
            {
                ShowHelp();
                return;
            }

            var type = ProjectTypeConverter.ToProjectType(envParser.GetOption("-t") ?? "Classic");
            var baseTemplateDir = envParser.GetOption("-b") ?? "BaseTemplate";
            var sourceFilesDir = envParser.GetOption("-s") ?? "src"; // XmlDocumentExtensions.xml  XmlDocumentParser.xml XmlDocumentToHtml.xml
            var inputFiles = envParser.GetValues(); // GetXmlFiles(sourceFilesDir); // envParser.GetValues();
            if (inputFiles.Length < 1)
                inputFiles = GetXmlFiles(sourceFilesDir);
            var outputPath = envParser.GetOutputFilepath() ?? PathUtils.UnifiedPathSeparator("{0}/Root".FormatString(CommonCoreLib.AppInfo.GetAppPath()));

            var ignorePathText = envParser.GetOption("-i");
            var ignorePathList = new List<string>();
            if (ignorePathText != null)
            {
                var lines = ignorePathText.Split(' ');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        ignorePathList.Add(line);
                }
            }

            var (singleDirectoryName, directoryName) = GetSingleDirectoryNameAndDirectoryName(outputPath);

            var csprojAnalyzer = new ClassicCsprojAnalyzer(sourceFilesDir)
            {
                IgnoreProjectNames = ignorePathList
            };
            Element root;
            if (inputFiles.Length < 1)
            {
                var generator = new CSharpDocumentGenerator(csprojAnalyzer);
                var xmlDocument = generator.ToString();

                root = CsXmlDocumentParser.ParseFromText(
                   xmlDocument: xmlDocument,
                   rootName: singleDirectoryName,
                   parseProgressEventHandler: XmlParser_ParserProgress,
                   completed: XmlParser_CodeAnalysisCompleted,
                   startAct: (name) => Console.WriteLine("Start XmlParse {0}", name)
                   );
            }
            else
            {
                root = CsXmlDocumentParser.ParseMultiFiles(
                   files: inputFiles,
                   rootName: singleDirectoryName,
                   parseProgressEventHandler: XmlParser_ParserProgress,
                   completed: XmlParser_CodeAnalysisCompleted,
                   startAct: (name) => Console.WriteLine("Start XmlParse {0}", name)
                   );
            }
            
            var parser = new CSharpEasyAnalyzer();
			parser.CodeAnalysisCompleted += Parser_CodeAnalysisCompleted;
            parser.AnalysisProgress += Parser_AnalysisProgress;

			Console.WriteLine("Start C# code analysis.");
            parser.Parse(csprojAnalyzer);
            parser.AddAttributesToElement(root);

            var converter = new CsXmlToHtmlWriter(root) { TemplateDir = baseTemplateDir };
            converter.WriteToDisk(directoryName);
        }

        private static void XmlParser_ParserProgress(object sender, CsXmlDocumentParser.XmlDocumentParseProgressEventArgs eventArgs)
        {
            if (eventArgs.Type == CsXmlDocumentParser.ParseType.First)
                Console.WriteLine(" {0,3:d}% Xml First Parse\t{1}", eventArgs.Percentage, eventArgs.Filename);
            else
                Console.WriteLine(" {0,3:d}% Xml Second Parse\t{1}", eventArgs.Percentage, eventArgs.Filename);
        }

        private static void Parser_AnalysisProgress(object sender, CSharpEasyAnalyzer.CSharpParseProgressEventArgs eventArgs)
        {
            if (eventArgs.Type == CSharpEasyAnalyzer.CSharpParseProgressEventArgs.ParseType.SyntacticAnalysis)
                Console.WriteLine(" {0,3:d}% Code Analysis\t{1}", eventArgs.Percentage, eventArgs.Filename);
            else
                Console.WriteLine(" {0,3:d}% Syntactic Analysis\t{1}", eventArgs.Percentage, eventArgs.Filename);
        }

        private static void XmlParser_CodeAnalysisCompleted(object sender, CsXmlDocumentParser.IXmlDocumentParseProgress e)
        {
            if (e.Type == CsXmlDocumentParser.ParseType.First)
                Console.WriteLine("Completed Xml First Parse.\n");
            else
                Console.WriteLine("Completed Xml Second Parse.\n");
        }

        private static void Parser_CodeAnalysisCompleted(object sender, EventArgs e)
		{
			Console.WriteLine("Completed C# code analysis.");
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
            sb.AppendLine("Copyright (C) 2018 - 2020 Aona Suzutsuki.");
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

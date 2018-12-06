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

            envParser.Analyze();
            if (envParser.GetOption("-h") != null)
            {
                ShowHelp();
                return;
            }

            var baseTemplateDir = envParser.GetOption("-b") ?? "BaseTemplate";
            var inputFiles = envParser.GetValues();
            var outputPath = envParser.GetOutputFilepath() ?? PathUtils.ResolvePathSeparator("{0}/Root".FormatString(CommonCoreLib.AppInfo.GetAppPath()));

            var (singleDirectoryName, directoryName) = PathUtils.GetSingleDirectoryNameAndDirectoryName(outputPath);

            Element root = CsXmlDocumentParser.ParseMultiFiles(inputFiles, singleDirectoryName);
            var converter = new CsXmlToHtmlWriter(root) { TemplateDir = baseTemplateDir };
            converter.WriteToDisk(directoryName);
        }

        static void ShowHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine(" Convert C# XML Document to HTML.");
            sb.AppendLine(" Copyright (C) 2018 Aona Suzutsuki.");
            sb.AppendFormat("\n\t{0}\t{1}\n", "-h", "Show help.");
            sb.AppendFormat("\t{0}\t{1}\n", "-b", "Specify the directory where the template file is stored. If you specify this, you can output with your own template.");
            sb.AppendFormat("\t{0}\t{1}\n", "-o", "Change the directory path of the output destination.");
            Console.WriteLine(sb);
        }
    }
}

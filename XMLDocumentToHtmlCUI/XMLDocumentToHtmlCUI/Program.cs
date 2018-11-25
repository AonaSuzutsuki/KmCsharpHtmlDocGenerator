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
using XmlDocumentToHtml.Extensions;
using XmlDocumentToHtml.Writer;
using XmlDocumentToHtml.CommonPath;

namespace XMLDocumentToHtmlCUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var envParser = new Parser.EnvArgumentParser(args);
            var baseTemplateDir = envParser.GetOption("-b") ?? "BaseTemplate";
            var inputFiles = envParser.GetValues();
            var outputPath = envParser.GetOutputFilepath() ?? PathUtils.ResolvePathSeparator("{0}/Root".FormatString(CommonCoreLib.AppInfo.GetAppPath()));

            var (singleDirectoryName, directoryName) = PathUtils.GetSingleDirectoryNameAndDirectoryName(outputPath);

            Element root = CsXmlDocumentParser.ParseMultiFiles(inputFiles, singleDirectoryName);
            var converter = new CsXmlToHtmlWriter(root) { TemplateDir = baseTemplateDir };
            converter.WriteToDisk(directoryName);
        }
    }
}

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

namespace XMLDocumentToHtmlCUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var envParser = new Parser.EnvArgumentParser(args);
            var baseTemplateDir = envParser.GetOption("-b") ?? "BaseTemplate";
            var inputFiles = envParser.GetValues();
            var outputPath = envParser.GetOutputFilepath();

            Element root = CsXmlDocumentParser.ParseMultiFiles(inputFiles, outputPath);
            var converter = new CsXmlToHtmlWriter(root) { TemplateDir = baseTemplateDir };
            converter.WriteToDisk("{0}/".FormatString(CommonCoreLib.AppInfo.GetAppPath()));
        }
    }
}

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
using XMLDocumentToHtmlCUI.Crypto;
using XMLDocumentToHtmlCUI.Extensions;
using XMLDocumentToHtmlCUI.Writer;

namespace XMLDocumentToHtmlCUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var envParser = new Parser.EnvArgumentParser(args);
            var inputFiles = envParser.GetValues();
            var outputPath = envParser.GetOutputFilepath();

            Element root = new Element()
            {
                Name = outputPath,
                Type = ElementType.Root
            };
            foreach (var input in inputFiles)
            {
                var parser = new CsXmlDocumentParser(input);
                root.Namespaces.AddRange(parser.TreeElement.Namespaces);
            }
            var converter = new CsXmlToHtmlWriter(root);
            converter.WriteToDisk();
        }
    }
}

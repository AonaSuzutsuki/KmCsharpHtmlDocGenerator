using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmlDocumentParser;

namespace XMLDocumentToHtmlCUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new XmlDocumentParser.CsXmlDocument.CsXmlDocumentParser("base.xml");
            var members = parser.Members;

            foreach (var member in members)
            {
                Console.WriteLine(member);
            }

            Console.ReadLine();
        }
    }
}

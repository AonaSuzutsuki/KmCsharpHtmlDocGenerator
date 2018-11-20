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

namespace XMLDocumentToHtmlCUI
{
    class Program
    {
        static void Main(string[] args)
        {

			//var loader = new Template.TemplateLoader("base_template.html");
			//loader.Assign("title", "test");
   //         loader.Assign("body", "test");
			//Console.WriteLine(loader);

            var parser = new CsXmlDocumentParser("base.xml");
			var root = parser.TreeElement;
			//         foreach (var member in root.Namespaces)
			//             ShowElements(member);
			//CreateDirectory(root);
			//CreateClassFile(root);
			Console.WriteLine(CreateMenu(root));

            Console.ReadLine();
        }

		static void CreateDirectory(Element element, string suffix = "")
		{
			if (element != null)
            {
                if (element.Namespaces != null)
                {
					var name = suffix + element.Name;
					var di = new DirectoryInfo(name);
					if (!di.Exists)
						di.Create();
                    foreach (var elem in element.Namespaces)
						CreateDirectory(elem, name + "/");
                }
            }
		}

		static void CreateClassFile(Element element, string suffix = "")
		{
			if (element != null)
            {
                if (element.Namespaces != null)
                {
                    var name = suffix + element.Name;
                    foreach (var elem in element.Namespaces)
						CreateClassFile(elem, name + "/");
                }
                else
                {
                    var name = suffix + element.Name + ".html";
					var fs = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.Read);
					//element.Members: elements
					WriteHtml(fs, element.Members, element);
					fs.Dispose();
                }
            }
		}

		static string CreateMenu(Element element, string suffix = "")
        {
			if (element.Type == ElementType.Root)
			{            
                var sb2 = new StringBuilder();
				sb2.AppendLine("<ul>");
                foreach (var elem in element.Namespaces)
					sb2.AppendLine(CreateMenu(elem, "\t"));
                sb2.AppendLine("</ul>");
				return sb2.ToString();
			}

			if (element == null)
				return string.Empty;

			var sb = new StringBuilder();
			if (element.Namespaces != null)
			{
				var name = suffix + "<li>" + element.Name;
				sb.AppendLine(name);
				sb.AppendLine(suffix + "<ul>");
				//Console.WriteLine(name);
				//Console.WriteLine("<ul>");
				foreach (var elem in element.Namespaces)
					sb.AppendLine(CreateMenu(elem, suffix + "\t\t"));
				sb.AppendLine(suffix + "</ul>");
                sb.AppendLine(suffix + "</li>");
                //Console.WriteLine("</ul></li>");
			}
			else
			{
				var name = suffix + "<li><a href=\"#\">" + element.Name + "</a></li>";
                sb.AppendLine(name);
				//Console.WriteLine(name);
			}
			return sb.ToString();
		}

		static void WriteHtml(Stream stream, List<Member> members, Element parent)
		{
			var classByte = Encoding.UTF8.GetBytes(parent.Name + "\n");
			stream.Write(classByte, 0, classByte.Length);
			foreach (var member in members)
			{
				var data = Encoding.UTF8.GetBytes(" {0}: {1}\n".FormatString(member.Type.ToString(), member.Name));
				stream.Write(data, 0, data.Length);
			}
		}

        static void ShowElements(Element element, string suffix = "")
        {
            if (element != null)
            {
                if (element.Namespaces != null)
                {
                    var name = suffix + "> " + element.Name;
                    Console.WriteLine(name);
                    foreach (var elem in element.Namespaces)
                        ShowElements(elem, suffix + " ");
                }
                else
                {
                    var name = suffix + "> " + element.Name;
                    Console.WriteLine(name);
                }

                if (element.Members != null)
                {
                    foreach (var elem in element.Members)
                    {
                        string parametersStr = "";
                        if (elem.Type == MethodType.Method)
                        {
                            parametersStr += "(";
                            var parameters = elem.MethodParameters.Zip(elem.Parameters.Keys, (type, name) => new { Type = type, Name = name });
                            foreach (var tuple in parameters.Select((v, i) => new { Index = i, Value = v }))
                            {
                                if (tuple.Index < elem.Parameters.Count - 1)
                                    parametersStr += "{0} {1}, ".FormatString(tuple.Value.Type, tuple.Value.Name);
                                else
                                    parametersStr += "{0} {1})".FormatString(tuple.Value.Type, tuple.Value.Name);

                            }
                        }
                        Console.WriteLine("{0} > {1}: {2}{3}", suffix, elem.Type, elem.Name, parametersStr);
                    }
                }
            }
        }

    }
}

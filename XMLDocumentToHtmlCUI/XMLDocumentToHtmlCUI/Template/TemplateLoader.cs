using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XMLDocumentToHtmlCUI.Extensions;

namespace XMLDocumentToHtmlCUI.Template
{
	public class TemplateLoader
	{

		private readonly string templatePath;

		private readonly Dictionary<string, string> map = new Dictionary<string, string>();

		public TemplateLoader(string templatePath)
		{
			this.templatePath = templatePath;
		}

		public void Assign(string key, string value)
		{
			if (map.ContainsKey(key))
				map[key] = value;
			else
				map.Add(key, value);
		}

		private string Analyze()
		{
			var converted = new List<string>();
			var lines = File.ReadAllLines(templatePath);
			foreach (var line in lines)
			{
				var reg = new Regex("\\{\\$(?<key>.*?)\\}");
				var match = reg.Match(line);
				if (match.Success)
				{
					var key = match.Groups["key"].ToString();
					var value = map.Get(key);
					converted.Add(line.Replace("{$" + key + "}", value));
				}
				else
                    converted.Add(line);
			}
			return converted.GetString();
		}

		public override string ToString()
		{         
			return Analyze();
		}
	}
}

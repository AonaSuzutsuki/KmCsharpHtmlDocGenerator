using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XMLDocumentToHtmlCUI.Extensions;
using System.Text;
using System.Linq;

namespace XMLDocumentToHtmlCUI.Template
{
	public class TemplateLoader
	{

        class ValueIndentPair
        {
            public string Value { get; set; }
            public bool IsIndent { get; set; }
        }

		private readonly string templatePath;

        private readonly Dictionary<string, ValueIndentPair> map = new Dictionary<string, ValueIndentPair>();

        public TemplateLoader(string templatePath)
		{
			this.templatePath = templatePath;
		}

		public void Assign(string key, string value, bool isIndent = false)
		{
            if (map.ContainsKey(key))
                map[key] = new ValueIndentPair() { Value = value, IsIndent = isIndent };
            else
                map.Add(key, new ValueIndentPair() { Value = value, IsIndent = isIndent });
		}

		private string Analyze()
		{
			var converted = new List<string>();
			var lines = File.ReadAllLines(templatePath);

            string preLine;
			foreach (var line in lines)
			{
                preLine = line;
                var reg = new Regex("\\{\\$(?<key>.*?)\\}+");
                var indent = GetIndent(preLine);
                while (true)
                {
                    var match = reg.Match(preLine);
                    if (match.Success)
                    {
                        var key = match.Groups["key"].ToString();
                        var pair = map.Get(key);
                        if (pair != null)
                        {
                            var value = pair.Value;
                            if (pair.IsIndent)
                                value = ResolveIndent(value, indent);
                            preLine = preLine.Replace("{$" + key + "}", value);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        converted.Add(preLine);
                        break;
                    }
                }
			}
			return converted.GetString();
		}

        private string ResolveIndent(string text, string indent)
        {
            var sb = new StringBuilder();
            var textArray = new List<string>(ResolveNewLine(text).Split('\n'));
            foreach (var tuple in textArray.Select((v, i) => new { Index = i, Value = v }))
            {
                if (!string.IsNullOrEmpty(tuple.Value))
                {
                    if (tuple.Index > 0)
                        sb.AppendFormat("{0}{1}\r\n", indent, tuple.Value);
                    else
                        sb.AppendFormat("{0}\r\n", tuple.Value);
                }
            }
            return sb.ToString();
        }

        private string ResolveNewLine(string text)
        {
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\r", "\n");
            return text;
        }

        private string GetIndent(string text)
        {
            var reg = new Regex("(?<spaces> +)(.*)");
            var match = reg.Match(text);
            if (match.Success)
            {
                var spaces = match.Groups["spaces"].ToString();
                return spaces;
            }
            return string.Empty;
        }

        public override string ToString()
		{         
			return Analyze();
		}
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XmlDocumentToHtml.Extensions;
using System.Text;
using System.Linq;

namespace XmlDocumentToHtml.Template
{
	public class TemplateLoader
	{

        class ValueIndentPair
        {
            public string Value { get; set; }
            public bool IsIndent { get; set; }
        }

		private readonly string templatePath;

        private Dictionary<string, ValueIndentPair> map = new Dictionary<string, ValueIndentPair>();

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

        public void Reset()
        {
            map = new Dictionary<string, ValueIndentPair>();
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
                            preLine = preLine.Replace("{$" + key + "}", "");
                        }
                    }
                    else
                    {
                        converted.Add(preLine);
                        break;
                    }
                }
			}
			return Analyze2(converted.GetString());
		}

        private string Analyze2(string text)
        {
            var reg = new Regex("(?<startStatement>{(?<start>.*) +\\$(?<key>.*) == (?<statementValue>.*)})(?<value>[\\s\\S]*)(?<endStatement>{\\/(?<end>.*)})");
            var match = reg.Match(text);
            if (match.Success)
            {
                var statementId = match.Groups["start"].ToString();
                var key = match.Groups["key"].ToString();
                var pair = map.Get(key);

                if (statementId.Equals("if"))
                {
                    bool.TryParse(match.Groups["statementValue"].ToString(), out bool statementValue);
                    var valueStr = pair != null ? pair.Value : false.ToString();
                    bool.TryParse(valueStr, out bool val);
                    if (val != statementValue)
                        text = text.Replace(match.Groups["value"].ToString(), "");
                }

                var endReg = new Regex("(?<endStatement>{\\/if \\$" + key + "})");
                var endMatch = endReg.Match(text);
                if (endMatch.Success)
                {
                    var v1 = match.Groups["startStatement"].ToString();
                    var v2 = endMatch.Groups["endStatement"].ToString();
                    text = text.Replace(v1, "").Replace(v2, "");
                    return Analyze2(text);
                }
            }
            return text;
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

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XmlDocumentExtensions.Extensions;
using System.Text;
using System.Linq;

namespace XmlDocumentToHtml.Template
{
    /// <summary>
    /// It is a template engine that generates character strings from correspondence between keys and values.
    /// </summary>
	public class TemplateLoader
	{

        private class ValueIndentPair
        {
            public string Value { get; set; }
            public bool IsIndent { get; set; }
        }

        #region Fields
        private readonly string templatePath;

        private Dictionary<string, ValueIndentPair> map = new Dictionary<string, ValueIndentPair>();
        #endregion

        /// <summary>
        /// Initialize TemplateLoader with path of template file.
        /// </summary>
        /// <param name="templatePath">Path of template file.</param>
        public TemplateLoader(string templatePath)
		{
			this.templatePath = templatePath;
		}

        /// <summary>
        /// Assign value to key.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Assigned value.</param>
        /// <param name="isIndent">Whether or not to indent.</param>
        public void Assign<T>(string key, T value, bool isIndent = false)
        {
            if (map.ContainsKey(key))
                map[key] = new ValueIndentPair() { Value = value.ToString(), IsIndent = isIndent };
            else
                map.Add(key, new ValueIndentPair() { Value = value.ToString(), IsIndent = isIndent });
        }

        /// <summary>
        /// Reset key value map on TemplateLoader.
        /// </summary>
        public void Reset()
        {
            map = new Dictionary<string, ValueIndentPair>();
        }

        /// <summary>
        /// Returns a string with a value added to the key.
        /// </summary>
        /// <returns>Completed string.</returns>
        public override string ToString()
        {
            return Analyze();
        }

        private string Analyze()
		{
			var converted = new List<string>();
			var lines = File.ReadAllLines(templatePath);

            string preLine;
			foreach (var line in lines)
			{
                preLine = line;
                var reg = new Regex("\\{\\$(?<key>.*?)\\}");
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
			return SecondAnalyze(converted.GetString());
		}

        private string SecondAnalyze(string text)
        {
            var reg = new Regex("(?<startStatement>{(?<start>.*) +\\$(?<key>.*) == (?<statementValue>.*)})(?<value>[\\s\\S]*)(?<endStatement>{\\/(?<end>.*)})");
            var match = reg.Match(text);
            if (match.Success)
            {
                var key = match.Groups["key"].ToString();
                var endReg = new Regex("(?<startStatement>{(?<start>.*) +\\$(?<key>.*) == (?<statementValue>.*)})(?<value>[\\s\\S]*)(?<endStatement>{\\/if \\$" + key + "})"); //(?<endStatement>{\\/if \\$" + key + "})
                var endMatch = endReg.Match(text);
                if (endMatch.Success)
                {
                    var v1 = endMatch.Groups["startStatement"].ToString();
                    var v2 = endMatch.Groups["endStatement"].ToString();
                    text = text.Replace(v1, "").Replace(v2, "");

                    var statementId = endMatch.Groups["start"].ToString();
                    var pair = map.Get(key);
                    if (statementId.Equals("if"))
                    {
                        bool.TryParse(endMatch.Groups["statementValue"].ToString(), out bool statementValue);
                        var valueStr = pair != null ? pair.Value : false.ToString();
                        bool.TryParse(valueStr, out bool val);

                        if (val != statementValue)
                        {
                            var _value = endMatch.Groups["value"].ToString();
                            text = text.Replace(_value, "");
                        }
                    }

                    return SecondAnalyze(text);
                }
            }
            return text;
        }

        private static string ResolveIndent(string text, string indent)
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

        private static string ResolveNewLine(string text)
        {
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\r", "\n");
            return text;
        }

        private static string GetIndent(string text)
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
    }
}

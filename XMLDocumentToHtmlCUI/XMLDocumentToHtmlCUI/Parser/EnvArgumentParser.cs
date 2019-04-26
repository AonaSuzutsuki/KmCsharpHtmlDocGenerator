using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentExtensions.Extensions;

namespace XMLDocumentToHtmlCUI.Parser
{
    public class EnvArgumentParser
    {

        protected readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        protected readonly List<string> values = new List<string>();

        protected Dictionary<string, int> optionCountMap = new Dictionary<string, int>();

        public void AddOptionCount(string key, int count)
        {
            optionCountMap.Put(key, count);
        }

        public void Analyze(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("-", StringComparison.Ordinal))
                {
                    var count = optionCountMap.Get(arg, 1);
                    if (count > 0)
                    {
                        if (args.Length > i + 1)
                        {
                            var value = args[++i];
                            parameters.Add(arg, value);
                        }
                    }
                    else
                    {
                        parameters.Add(arg, "");
                    }
                }
                else
                {
                    values.Add(arg);
                }
            }
        }

        public string GetOutputFilepath(string option = "-o")
        {
            return GetOption(option);
        }
        
        public string GetOption(string option)
        {
            if (parameters.ContainsKey(option))
                return parameters[option];
            return null;
        }

        public string[] GetValues()
        {
            return values.ToArray();
        }

        public string GetValue()
        {
            if (values.Count > 0)
                return values[0];
            return null;
        }
    }
}

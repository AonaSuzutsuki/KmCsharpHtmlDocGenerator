using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    public class Member
    {
        public MethodType Type { get; set; }
        public NamespaceItem NameSpace { get; set; }
        public string Name { get; set; }
        public List<string> MethodParameters { get; set; } = new List<string>();
        public string Value { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[{0}: {1},", "Type", Type.ToString());
            sb.AppendFormat("{0}: {1},", "NameSpace", NameSpace.ToString());
            sb.AppendFormat("{0}: {1},", "Name", Name.ToString());
            sb.AppendFormat("{0}: {1},", "MethodParameters", MethodParameters.ToString());
            sb.AppendFormat("{0}: {1},", "Value", Value.ToString());
            sb.AppendFormat("{0}: {1}]\n", "Parameters", Parameters.ToString());
            return sb.ToString();
        }
    }
}

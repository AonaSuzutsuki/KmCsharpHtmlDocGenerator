using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    public class Element
    {
        public ElementType Type { get; set; }
        public string Name { get; set; }
        public List<Element> Namespaces { get; set; } = new List<Element>();
        public List<Member> Members { get; set; } = new List<Member>();

        public bool HasElement(string name)
        {
            foreach (var elem in Namespaces)
            {
                if (elem.Name.Equals(name))
                    return true;
            }
            return false;
        }

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendFormat("[{0}: {1}, ", "Type", Type.ToString());
        //    sb.AppendFormat("{0}: {1}\n", "Name", Name.ToString());

        //    foreach (var elem in Namespaces.Select((v, i) => new { Index = i, Value = v }))
        //    {
        //        sb.AppendFormat("\t{0}\n", elem.Value.ToString());
        //    }

        //    foreach (var elem in Members.Select((v, i) => new { Index = i, Value = v }))
        //    {
        //        sb.AppendFormat("\t{0}\n", elem.Value.ToString());
        //    }

        //    sb.AppendLine("]");
        //    return sb.ToString();
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser
{
    public class NamespaceItem
    {
        private readonly List<string> items;

        public NamespaceItem(string text)
        {
            var array = text.Split('.');
            items = new List<string>(array);
        }

        public NamespaceItem(string[] list)
        {
            items = new List<string>(list);
        }

        public NamespaceItem GetNamespace()
        {
            if (items.Count > 0)
                return new NamespaceItem(items.Take(items.Count - 1).ToArray());
            return null;
        }

        public string GetFirstName()
        {
            if (items.Count > 0)
                return items.First();
            return null;
        }

        public string GetLastName()
        {
            if (items.Count > 0)
                return items.Last();
            return null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (i < items.Count - 1)
                    sb.AppendFormat("{0}.", item);
                else
                    sb.AppendFormat("{0}", item);
            }
            return sb.ToString();
        }
    }
}

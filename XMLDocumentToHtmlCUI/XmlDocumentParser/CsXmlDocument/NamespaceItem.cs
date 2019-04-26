using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// Namespace Item for <see cref="Element"/>.
    /// </summary>
    public class NamespaceItem
    {
        private readonly List<string> items;

        #region Properties
        /// <summary>
        /// Count of namespaces.
        /// </summary>
        public int NamespaceCount { get => items.Count; }

        /// <summary>
        /// Returns whether it is Root or not.
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return NamespaceCount == 0;
            }
        }
        #endregion

        /// <summary>
        /// Initialize NamespaceItem without default namespace.
        /// </summary>
        public NamespaceItem()
        {
            items = new List<string>();
        }

        /// <summary>
        /// Initialize NamespaceItem with a namespace text.
        /// </summary>
        /// <param name="text">A namespace text.</param>
        public NamespaceItem(string text)
        {
            var array = text.Split('.');
            items = new List<string>(array);
        }

        /// <summary>
        /// Initialize NamespaceItem with namespace array.
        /// </summary>
        /// <param name="list">Namespace array</param>
        public NamespaceItem(string[] list)
        {
            items = new List<string>(list);
        }

        /// <summary>
        /// Get parent namespace.
        /// </summary>
        /// <returns>Parent namespace array.</returns>
        public NamespaceItem GetParentNamespace()
        {
            if (items.Count > 0)
                return new NamespaceItem(items.Take(items.Count - 1).ToArray());
            return null;
        }

        /// <summary>
        /// Get namespace item without first item.
        /// </summary>
        /// <returns>NamespaceItem without first item.</returns>
        public NamespaceItem GetNamespaceWithoutFirst()
        {
            if (items.Count > 0)
            {
                var item = new NamespaceItem(items.Skip(1).ToArray());
                return item;
            }
            return null;
        }

        /// <summary>
        /// Get a first name in namespaces.
        /// </summary>
        /// <returns>A first name.</returns>
        public string GetFirstName()
        {
            if (items.Count > 0)
                return items.First();
            return null;
        }

        /// <summary>
        /// Get a last name in namespaces.
        /// </summary>
        /// <returns>A last name.</returns>
        public string GetLastName()
        {
            if (items.Count > 0)
                return items.Last();
            return null;
        }

        /// <summary>
        /// Get a namespace string.
        /// </summary>
        /// <returns>A namespace string.</returns>
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


        /// <summary>
        /// Object.GetHashCode()
        /// </summary>
        /// <returns>The hash value.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Check the equivalence of this object and the argument object.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <returns>It returns True if equivalent, False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            var namespaceItem = (NamespaceItem)obj;
            return items.SequenceEqual(namespaceItem.items);
        }
    }
}

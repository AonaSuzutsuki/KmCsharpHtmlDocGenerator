using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// Type of tree structure element such as namespace.
    /// </summary>
    public enum ElementType
    {
        /// <summary>
        /// Root.
        /// </summary>
        Root,

        /// <summary>
        /// Namespace.
        /// </summary>
        Namespace,

        /// <summary>
        /// Class.
        /// </summary>
        Class,

        /// <summary>
        /// Interface.
        /// </summary>
        Interface,

        /// <summary>
        /// Enum.
        /// </summary>
        Enum,
        
        /// <summary>
        /// Struct.
        /// </summary>
        Struct,

        /// <summary>
        /// Delegate.
        /// </summary>
        Delegate,
    }
}

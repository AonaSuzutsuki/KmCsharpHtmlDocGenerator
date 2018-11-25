using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// Method type of <c>Member</c>.
    /// </summary>
    public enum MethodType
    {
        /// <summary>
        /// Class.
        /// </summary>
        Class,

        /// <summary>
        /// Constructor.
        /// </summary>
        Constructor,

        /// <summary>
        /// Method.
        /// </summary>
        Method,

        /// <summary>
        /// Field.
        /// </summary>
        Field,

        /// <summary>
        /// Property.
        /// </summary>
        Property,

        /// <summary>
        /// Enum item.
        /// </summary>
        EnumItem,
        
    }
}

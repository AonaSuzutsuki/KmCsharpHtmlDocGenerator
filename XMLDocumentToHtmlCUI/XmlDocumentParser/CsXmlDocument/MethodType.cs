using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument
{
    /// <summary>
    /// Method type of <c><see cref="Member"/></c>.
    /// </summary>
    public enum MethodType
    {
        /// <summary>
        /// Class.
        /// </summary>
        Class,

        /// <summary>
        /// The event.
        /// </summary>
        Event,

        /// <summary>
        /// Constructor.
        /// </summary>
        Constructor,

        /// <summary>
        /// Static method.
        /// </summary>
        Function,

        /// <summary>
        /// Method.
        /// </summary>
        Method,

        /// <summary>
        /// Extension method.
        /// </summary>
        ExtensionMethod,

        /// <summary>
        /// Field.
        /// </summary>
        Field,

        /// <summary>
        /// Property.
        /// </summary>
        Property,
        
    }
}

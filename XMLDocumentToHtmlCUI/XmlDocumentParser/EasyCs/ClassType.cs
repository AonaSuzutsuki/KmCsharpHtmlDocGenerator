using System;
namespace XmlDocumentParser.EasyCs
{
	/// <summary>
    /// Class type for ClassInfo.
    /// </summary>
    public enum ClassType
    {
        /// <summary>
        /// The class.
        /// </summary>
        Class,

        /// <summary>
        /// The interface.
        /// </summary>
        Interface,

        /// <summary>
        /// The enum.
        /// </summary>
        Enum,

        /// <summary>
        /// The struct.
        /// </summary>
        Struct,

        /// <summary>
        /// The delegate.
        /// </summary>
        Delegate,

        /// <summary>
        /// The method.
        /// </summary>
        Method,

        /// <summary>
        /// The property.
        /// </summary>
        Property
    }
}

using System;
namespace XmlDocumentParser.EasyCs
{
	/// <summary>
    /// Class type for ClassInfo.
    /// </summary>
    public enum ClassType
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown,

        /// <summary>
        /// For inheritance
        /// </summary>
        Inheritance,

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
        /// The constructor.
        /// </summary>
        Constructor,

        /// <summary>
        /// The property.
        /// </summary>
        Property
    }
}

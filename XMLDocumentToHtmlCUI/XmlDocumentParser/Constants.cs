using System;
using XmlDocumentParser.EasyCs;

namespace XmlDocumentParser
{
    /// <summary>
    /// Constants.
    /// </summary>
	public static class Constants
	{
        /// <summary>
        /// The system void.
        /// </summary>
		public const string SystemVoid = "void";

        public static readonly TypeInfo SystemVoidTypeInfo = new TypeInfo
        {
            Name = SystemVoid
        };
	}
}

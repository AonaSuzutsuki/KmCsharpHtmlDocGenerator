using System;
using System.IO;

namespace XmlDocumentExtensions.Extensions
{
    /// <summary>
    /// String extension.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Unifieds the new line.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>The new line.</returns>
        public static string UnifiedNewLine(this string text)
        {
            return text.Replace("\r\n", "\r").Replace("\r", "\n");
        }

        /// <summary>
        /// Unifieds the path separator to its system.
        /// </summary>
        /// <param name="path">Target path.</param>
        /// <returns>Unified path.</returns>
        public static string UnifiedSystemPathSeparator(this string path)
        {
            return path.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        }
    }
}

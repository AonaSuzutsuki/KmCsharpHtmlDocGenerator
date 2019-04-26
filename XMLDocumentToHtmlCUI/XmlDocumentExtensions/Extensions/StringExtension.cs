using System;
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
        /// <returns>The new line.</returns>
        /// <param name="text">Text.</param>
        public static string UnifiedNewLine(this string text)
        {
            return text.Replace("\r\n", "\r").Replace("\r", "\n");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace XmlDocumentExtensions.Extensions
{
    /// <summary>
    /// Extension functions of <see cref="IList"/>.
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        /// Convert <c>List&lt;string&gt;</c> to string.
        /// </summary>
        /// <param name="array">Target <c>List&lt;string&gt;</c>.</param>
        /// <returns>Converted string.</returns>
		public static string GetString(this List<string> array)
		{
			var sb = new StringBuilder();
			foreach (var text in array)
			{
				sb.AppendLine(text);
			}
			return sb.ToString();
		}
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace XMLDocumentToHtmlCUI.Extensions
{
    public static class ArrayExtension
    {
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

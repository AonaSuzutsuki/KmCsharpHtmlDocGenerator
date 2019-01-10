using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace XmlDocumentExtensions.Extensions
{
    /// <summary>
    /// Extension functions of <see cref="List{T}"/>.
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

        /// <summary>
        /// Processes elements of the specified collection and adds them to the list.
        /// </summary>
        /// <typeparam name="V1">Type of destination List element.</typeparam>
        /// <typeparam name="V2">Type of source collection element.</typeparam>
        /// <param name="destArray">Destination List.</param>
        /// <param name="sourceArray">Source collection.</param>
        /// <param name="func">Function for elements.</param>
        public static void Add<V1, V2>(this List<V1> destArray, IEnumerable<V2> sourceArray, Func<V2, V1> func)
        {
            foreach (var item in sourceArray)
                destArray.Add(func(item));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Test(out string a, ref string b)
        {
            a = "";
        }
    }
}

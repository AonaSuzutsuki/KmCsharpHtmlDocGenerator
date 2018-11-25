using System;
using System.Collections.Generic;

namespace XmlDocumentToHtml.Extensions
{
    /// <summary>
    /// Extension functions of <c>Dictionary</c>.
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// It checks for the existence of the element and returns the element of the key if it exists.
        /// If it does not exist, it returns the default value.
        /// </summary>
        /// <typeparam name="K">Key type.</typeparam>
        /// <typeparam name="V">Value type.</typeparam>
        /// <param name="dict">Target dictionary.</param>
        /// <param name="key">The key you want to search</param>
        /// <returns></returns>
		public static V Get<K, V>(this Dictionary<K, V> dict, K key)
		{
			if (dict.ContainsKey(key))
				return dict[key];
			return default;
		}
    }
}

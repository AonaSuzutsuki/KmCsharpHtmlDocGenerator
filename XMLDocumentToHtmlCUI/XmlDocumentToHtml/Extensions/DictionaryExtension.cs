using System;
using System.Collections.Generic;

namespace XmlDocumentToHtml.Extensions
{
    public static class DictionaryExtension
    {
		public static V Get<K, V>(this Dictionary<K, V> dict, K key)
		{
			if (dict.ContainsKey(key))
				return dict[key];
			return default;
		}
    }
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace XmlDocumentExtensions.Extensions
{
    /// <summary>
    /// Extension functions of <see cref="Dictionary{TKey, TValue}"/>.
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
        /// <param name="defaultValue">Default value.</param>
        /// <returns></returns>
        public static V Get<K, V>(this Dictionary<K, V> dict, K key, V defaultValue = default)
		{
			if (dict.ContainsKey(key))
				return dict[key];
			return defaultValue;
		}

        /// <summary>
        /// Add only if the specified element does not exist in the dictionary.
        /// </summary>
        /// <typeparam name="K">Key type.</typeparam>
        /// <typeparam name="V">Value type.</typeparam>
        /// <param name="dict">Target dictionary.</param>
        /// <param name="key">The key you want to add.</param>
        /// <param name="value">The value you want to add.</param>
        public static void Put<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
            else
                dict[key] = value;
        }
    }
}

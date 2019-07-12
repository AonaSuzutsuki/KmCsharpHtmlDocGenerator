using System;
using System.Collections.Generic;

namespace XmlDocumentParser.EasyCs
{
    /// <summary>
    /// Converter of Project type.
    /// </summary>
    public static class ProjectTypeConverter
    {
        private static Dictionary<string, ProjectType> TypeMap;

        static ProjectTypeConverter()
        {
            TypeMap = new Dictionary<string, ProjectType>
            {
                { "Classic", ProjectType.Classic },
                { "Xamarin", ProjectType.Xamarin }
            };
        }

        /// <summary>
        /// Convert string to ProjectType.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ProjectType ToProjectType(string text)
        {
            if (TypeMap.ContainsKey(text))
                return TypeMap[text];
            return ProjectType.Classic;
        }
    }
}

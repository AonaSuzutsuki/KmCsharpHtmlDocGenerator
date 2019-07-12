using System;
using System.Collections.Generic;

namespace XmlDocumentParser.EasyCs
{
    public static class CompileTypeConverter
    {
        public static Dictionary<string, CompileType> TypeMap;

        static CompileTypeConverter()
        {
            TypeMap = new Dictionary<string, CompileType>
            {
                { "Classic", CompileType.Classic },
                { "Xamarin", CompileType.Xamarin }
            };
        }

        public static CompileType ToCompileType(string text)
        {
            if (TypeMap.ContainsKey(text))
                return TypeMap[text];
            return CompileType.Classic;
        }
    }
}

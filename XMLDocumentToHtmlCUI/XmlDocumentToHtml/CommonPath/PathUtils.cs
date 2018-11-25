using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentToHtml.CommonPath
{
    /// <summary>
    /// 
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ResolvePathSeparator(string path)
        {
            return path.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetSingleDirectoryName(string path)
        {
            var list = new List<string>(path.Split(Path.DirectorySeparatorChar));
            list.Remove("");
            if (list.Count > 0)
                return list[list.Count - 1];
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static (string singleDirectoryName, string directoryName) GetSingleDirectoryNameAndDirectoryName(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            var singleDirectoryName = GetSingleDirectoryName(path);
            var directoryName = "{0}{1}".FormatString(Path.GetDirectoryName(path), Path.DirectorySeparatorChar);
            return (singleDirectoryName, directoryName);
        }
    }
}

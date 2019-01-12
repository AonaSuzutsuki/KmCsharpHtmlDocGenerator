using CommonExtensionLib.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CommonPath
{
    /// <summary>
    /// Path related utility class.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// Unified path to system path character.
        /// </summary>
        /// <param name="path">Target path.</param>
        /// <returns>Unified path.</returns>
        public static string UnifiedPathSeparator(string path)
        {
            return path.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Get the directory name.
        /// </summary>
        /// <param name="path">Target path.</param>
        /// <returns>The directory name</returns>
        public static string GetSingleDirectoryName(string path)
        {
            var list = new List<string>(path.Split(Path.DirectorySeparatorChar));
            list.Remove("");
            if (list.Count > 0)
                return list[list.Count - 1];
            return null;
        }
    }
}

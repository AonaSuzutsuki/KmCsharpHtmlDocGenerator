using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentParser.EasyCs;
using XmlDocumentParser.XmlWrapper;

namespace XmlDocumentParser.Csproj
{
    /// <summary>
    /// Analyzer of Xamarin csproj file. *not supported at this time.
    /// </summary>
    public class XamarinCsprojAnalyzer : CsprojAnalyzer
    {
        public XamarinCsprojAnalyzer(string csprojParentPath) : base(csprojParentPath)
        {

        }

        /// <summary>
        /// not supported at this time.
        /// </summary>
        /// <param name="csprojParentPath">The parent directory where the csproj file is located. Search for the file by performing a recursion search.</param>
        /// <returns>The information about C# source files and reference libraries.</returns>
        public override CsFilesInfo GetCsFiles()
        {
            throw new NotImplementedException();
        }
    }
}

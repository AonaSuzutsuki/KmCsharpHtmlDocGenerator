using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentParser.EasyCs;
using XmlDocumentParser.XmlWrapper;

namespace XmlDocumentParser.Csproj
{
    public class XamarinCsprojAnalyzer : CsprojAnalyzer
    {
        public override CsFilesInfo GetCsFiles(string csprojParentPath, ProjectType compileType)
        {
            throw new NotImplementedException();
        }

        protected override string GetSystemAssemblyPath(string targetFramework, string reference)
        {
            throw new NotImplementedException();
        }

        protected override string GetTargetFramework(Reader reader)
        {
            throw new NotImplementedException();
        }

        protected override List<string> MergeParentPath(List<string> list, string parent)
        {
            throw new NotImplementedException();
        }
    }
}

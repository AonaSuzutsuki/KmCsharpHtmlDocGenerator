using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentParser.CommonPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XmlDocumentParser.CommonPath.Tests
{
    [TestClass()]
    public class PathUtilsTests
    {
        [TestMethod()]
        public void UnifiedPathSeparatorTest()
        {
            var test1 = "system\\io\\test.html";
            var test2 = "system/io/test.html";
            var exp = string.Format("system{0}io{0}test.html", Path.DirectorySeparatorChar);

            var value1 = PathUtils.UnifiedPathSeparator(test1);
            var value2 = PathUtils.UnifiedPathSeparator(test2);

            Assert.AreEqual(exp, value1);
            Assert.AreEqual(exp, value2);
        }

        [TestMethod()]
        public void GetSingleDirectoryNameTest()
        {
            var test = string.Format("system{0}io{0}test{0}", Path.DirectorySeparatorChar);
            var exp = "test";

            var value = PathUtils.GetSingleDirectoryName(test);
            var value2 = PathUtils.GetSingleDirectoryName("");

            Assert.AreEqual(exp, value);
            Assert.AreEqual(null, value2);
        }
    }
}
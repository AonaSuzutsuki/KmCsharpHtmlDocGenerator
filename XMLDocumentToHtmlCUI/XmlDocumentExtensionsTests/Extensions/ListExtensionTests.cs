using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentExtensions.Extensions.Tests
{
    [TestClass()]
    public class ListExtensionTests
    {
        [TestMethod()]
        public void GetStringTest()
        {
            var exp = new StringBuilder();
            exp.AppendLine("aaa");
            exp.AppendLine("bbb");
            exp.AppendLine("");
            exp.AppendLine("ccc");

            var list = new List<string>
            {
                "aaa",
                "bbb",
                "",
                "ccc"
            };

            Assert.AreEqual(exp.ToString(), list.GetString());
        }

        [TestMethod()]
        public void AddTest()
        {
            var exp = new List<int> { 2, 4, 6 };

            var source = new List<int> { 1, 2, 3 };
            var dist = new List<int>
            {
                { source, (item) => item + item }
            };

            CollectionAssert.AreEqual(exp, dist);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentParser.XmlWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XmlDocumentParser.XmlWrapper.Tests
{
    [TestClass()]
    public class ReaderTests
    {

        private const string XmlFilePath = "TestData/XmlTestFile.xml";

        private static string GetXmlText()
        {
            return File.ReadAllText("TestData/XmlTestFile.xml");
        }

        private static Stream GetXmlStream()
        {
            return null;
        }

        [TestMethod()]
        public void GetAttributesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAttributeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetValuesTest()
        {
            var exp = new List<string>()
            {
                "",
                "test2-value",
                "",
            };

            var xmlText = GetXmlText();
            var reader = new Reader();
            reader.LoadFromText(xmlText);

            var values = reader.GetValues("/root/members/member", false);

            CollectionAssert.AreEqual(exp.ToArray(), values);
        }

        [TestMethod()]
        public void GetValueTest()
        {
            var exp = "test2-value";

            var xmlText = GetXmlText();
            var reader = new Reader();
            reader.LoadFromText(xmlText);

            var value = reader.GetValue("/root/members/member[@name='test2']", false);

            Assert.AreEqual(exp, value);
        }
    }
}
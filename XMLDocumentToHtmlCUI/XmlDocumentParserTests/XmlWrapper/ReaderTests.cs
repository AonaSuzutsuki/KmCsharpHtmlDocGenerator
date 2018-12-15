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
            var exp = new List<string>()
            {
                "test",
                "test2",
                "test3",
                "test4"
            };

            var xmlText = GetXmlText();
            var reader = new Reader();
            reader.LoadFromText(xmlText);
            reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            var attributes = reader.GetAttributes("name", "/ns:root/ns:members/ns:member");

            CollectionAssert.AreEqual(exp.ToArray(), attributes);
        }

        [TestMethod()]
        public void GetAttributeTest()
        {
            var exp = "sub-test1";

            var xmlText = GetXmlText();
            var reader = new Reader();
            reader.LoadFromText(xmlText);
            reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            var attribute = reader.GetAttribute("name", "/ns:root/ns:members/ns:member[@name='test4']/ns:submember");

            Assert.AreEqual(exp, attribute);
        }

        [TestMethod()]
        public void GetValuesTest()
        {
            var exp = new List<string>()
            {
                "",
                "test2-value",
                "",
                "<submember name=\"sub-test1\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" />"
            };

            var xmlText = GetXmlText();
            var reader = new Reader();
            reader.LoadFromText(xmlText);
            reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            var values = reader.GetValues("/ns:root/ns:members/ns:member", false);

            CollectionAssert.AreEqual(exp.ToArray(), values);
        }

        [TestMethod()]
        public void GetValueTest()
        {
            var exp = "test2-value";
            var exp3 = "\r\ntest2-value\r\n\r\n";

            var xmlText = GetXmlText();
            var reader = new Reader();
            reader.LoadFromText(xmlText);
            reader.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            var value = reader.GetValue("/ns:root/ns:members/ns:member[@name='test2']", false);
            var value2 = reader.GetValue("/ns:root/ns:members/ns:mmm", false);
            var value3 = reader.GetValue("/ns:root/ns:members/ns:member[@name='test2']", true);

            Assert.AreEqual(exp, value);
            Assert.AreEqual(default, value2);
            Assert.AreEqual(exp3, value3);
        }
    }
}
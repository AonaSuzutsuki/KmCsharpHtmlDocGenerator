using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentParser.CsXmlDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XmlDocumentParser.CsXmlDocument.Tests
{
    [TestClass()]
    public class CsXmlDocumentParserTests
    {
        private static Element CreateExceptedElement()
        {
            var members = new List<Member>()
            {
                new Member()
                {
                    Id = "P:Test.TestClass.Value",
                    Type = MethodType.Property,
                    Name = "Value",
                    Namespace = new NamespaceItem("Test.TestClass"),
                    Value = "Value.",
                },
                new Member()
                {
                    Id = "M:Test.TestClass.#ctor(System.Int32,System.String)",
                    Type = MethodType.Constructor,
                    Name = "#ctor",
                    Namespace = new NamespaceItem("Test.TestClass"),
                    Value = "Test constructor.",
                    ParameterTypes = new List<EasyCs.TypeInfo>
                    {
                        new EasyCs.TypeInfo { Name = "int" },
                        new EasyCs.TypeInfo { Name = "string" }
                    },
                    ParameterNames = new Dictionary<string, string>
                    {
                        { "ivalue", "Int value." },
                        { "svalue", "String value." }
                    }
                },
                new Member()
                {
                    Id = "M:Test.TestClass.Method",
                    Type = MethodType.Method,
                    Name = "Method",
                    Namespace = new NamespaceItem("Test.TestClass"),
                    Value = "Test method.",
                    ReturnComment = "Return int.",
                }
            };

            var TestClassElement = new Element()
            {
                Id = "T:Test.TestClass",
                Name = "TestClass",
                Namespace = new NamespaceItem("Test"),
                Type = ElementType.Class,
                Value = "Test class.",
                Members = members,
            };

            var testElement = new Element()
            {
                Type = ElementType.Namespace,
                Namespace = new NamespaceItem("Test"),
                Name = "Test",
                Namespaces = new List<Element> { TestClassElement }
            };

            var exp = new Element()
            {
                Type = ElementType.Root,
                Name = "Root",
                Namespaces = new List<Element> { testElement },
            };

            return exp;
        }

        [TestMethod()]
        public void ConvertMemberNameToMemberTest()
        {
            var exp = new Member()
            {
                Id = "M:Test.TestClass.#ctor(System.Int32,System.String)",
                Type = MethodType.Constructor,
                Name = "#ctor",
                Namespace = new NamespaceItem("Test.TestClass"),
                ParameterTypes = new List<EasyCs.TypeInfo>
                {
                    new EasyCs.TypeInfo { Name = "int" },
                    new EasyCs.TypeInfo { Name = "string" }
                }
            };
            var value = CsXmlDocumentParser.ConvertMemberNameToMember("M:Test.TestClass.#ctor(System.Int32,System.String)");
            Assert.AreEqual(exp, value);
        }

        [TestMethod()]
        public void ParseMultiFilesTest()
        {
            var exp = CreateExceptedElement();
            var element = CsXmlDocumentParser.ParseMultiFiles(new string[]
            {
                string.Format("TestData{0}TestXmlDoc1.xml", Path.DirectorySeparatorChar)
            });

            Assert.AreEqual(exp, element);
        }
    }
}
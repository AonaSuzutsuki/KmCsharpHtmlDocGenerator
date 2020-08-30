using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentParser.EasyCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentParser.CsXmlDocument;
using Microsoft.CodeAnalysis;

namespace XmlDocumentParser.EasyCs.Tests
{
    [TestClass()]
    public class CSharpEasyAnalyzerTests
    {

        private Element CreateExceptedElement()
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
                    Accessibility = Accessibility.Public,
                    ReturnType = new TypeInfo { Name = "int" }
                },
                new Member()
                {
                    Id = "M:Test.TestClass.#ctor(System.Int32,System.String)",
                    Type = MethodType.Constructor,
                    Name = "TestClass",
                    Namespace = new NamespaceItem("Test.TestClass"),
                    Value = "Test constructor.",
                    ParameterTypes = new List<TypeInfo>
                    {
                        new TypeInfo { Name = "int" },
                        new TypeInfo { Name = "string" }
                    },
                    ParameterNames = new Dictionary<string, string>
                    {
                        { "ivalue", "Int value." },
                        { "svalue", "String value." }
                    },
                    Accessibility = Accessibility.Public
                },
                new Member()
                {
                    Id = "M:Test.TestClass.Method",
                    Type = MethodType.Method,
                    Name = "Method",
                    Namespace = new NamespaceItem("Test.TestClass"),
                    Value = "Test method.",
                    ReturnComment = "Return int.",
                    Accessibility = Accessibility.Public,
                    ReturnType = new TypeInfo { Name = "int" }
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
        public void AddAttributesToElementTest()
        {
            var exp = CreateExceptedElement();

            var element = CsXmlDocumentParser.ParseMultiFiles(new string[] { "TestData/TestXmlDoc1.xml" });
            var csParser = new CSharpEasyAnalyzer();
            csParser.Parse("TestData/Test");
            csParser.AddAttributesToElement(element);

            Assert.AreEqual(exp, element);
        }
    }
}
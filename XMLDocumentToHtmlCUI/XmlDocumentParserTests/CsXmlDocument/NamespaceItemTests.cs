using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentParser.CsXmlDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentParser.CsXmlDocument.Tests
{
    [TestClass()]
    public class NamespaceItemTests
    {
        private static NamespaceItem GetNamespaceItemFromString()
        {
            return new NamespaceItem("System.Collections.Generic.Dictionary<K, V>");
        }

        private static NamespaceItem GetNamespaceItemFromArray()
        {
            return new NamespaceItem(new string[] {
                "System", "Collections", "Generic", "Dictionary<K, V>"
            });
        }

        [TestMethod()]
        public void GetParentNamespaceTest()
        {
            var exp = new NamespaceItem("System.Collections.Generic");
            var namespaceItem = GetNamespaceItemFromString();
            var parent = namespaceItem.GetParentNamespace();
            var parent2 = new NamespaceItem().GetParentNamespace();

            Assert.AreEqual(exp, parent);
            Assert.AreEqual(null, parent2);
        }

        [TestMethod()]
        public void GetNamespaceWithoutFirstTest()
        {
            var exp = new NamespaceItem("Collections.Generic.Dictionary<K, V>");
            var namespaceItem = GetNamespaceItemFromString();
            var parent = namespaceItem.GetNamespaceWithoutFirst();
            var parent2 = new NamespaceItem().GetNamespaceWithoutFirst();

            Assert.AreEqual(exp, parent);
            Assert.AreEqual(null, parent2);
        }

        [TestMethod()]
        public void GetFirstNameTest()
        {
            var exp = "System";
            var namespaceItem = GetNamespaceItemFromString();
            var first = namespaceItem.GetFirstName();
            var first2 = new NamespaceItem().GetFirstName();

            Assert.AreEqual(exp, first);
            Assert.AreEqual(null, first2);
        }

        [TestMethod()]
        public void GetLastNameTest()
        {
            var exp = "Dictionary<K, V>";
            var namespaceItem = GetNamespaceItemFromString();
            var last = namespaceItem.GetLastName();
            var last2 = new NamespaceItem().GetLastName();

            Assert.AreEqual(exp, last);
            Assert.AreEqual(null, last2);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            var exp = "System.Collections.Generic.Dictionary<K, V>";
            var namespaceItem = GetNamespaceItemFromString();
            var namespaceItem2 = new NamespaceItem();

            Assert.AreEqual(exp, namespaceItem.ToString());
            Assert.AreEqual(string.Empty, namespaceItem2.ToString());
        }
    }
}
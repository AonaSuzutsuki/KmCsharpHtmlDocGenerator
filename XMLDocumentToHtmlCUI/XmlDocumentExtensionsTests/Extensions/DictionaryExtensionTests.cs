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
    public class DictionaryExtensionTests
    {
        [TestMethod()]
        public void GetTest()
        {
            var exp1 = new KeyValuePair<string, string>("key1", "value1");
            var exp2 = new KeyValuePair<string, string>("key2", "value2");
            var dict = new Dictionary<string, string>
            {
                { "key1", "value1" }
            };

            var value1 = dict.Get(exp1.Key, null);
            var value2 = dict.Get(exp2.Key, null);

            Assert.AreEqual(value1, exp1.Value);
            Assert.AreNotEqual(value2, exp2.Value);
            Assert.AreEqual(value2, null);
        }

        [TestMethod()]
        public void PutTest()
        {
            var exp1 = new KeyValuePair<string, string>("key1", "value1");
            var exp2 = new KeyValuePair<string, string>("key2", "value2");

            var dict = new Dictionary<string, string>();
            dict.Put(exp1.Key, exp1.Value);

            Assert.AreEqual(dict[exp1.Key], exp1.Value);

            dict.Put(exp1.Key, exp2.Value);

            Assert.AreEqual(dict[exp1.Key], exp2.Value);
        }
    }
}
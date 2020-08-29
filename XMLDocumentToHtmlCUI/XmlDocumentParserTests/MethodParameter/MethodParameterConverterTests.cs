using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlDocumentParser.MethodParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlDocumentParser.CsXmlDocument;
using XmlDocumentParser.EasyCs;

namespace XmlDocumentParser.MethodParameter.Tests
{
    [TestClass()]
    public class MethodParameterConverterTests
    {
        [TestMethod()]
        public void CreateMethodParameterTextTest()
        {
            var exp = "(this System.Collections.Generic.Dictionary&lt;K, V&gt; dict, K key, V defaultValue)";
            var member = new Member()
            {
                Type = MethodType.ExtensionMethod,
                ParameterTypes = new List<TypeInfo>
                {
                    new TypeInfo
                    {
                        Name = "Dictionary<K, V>",
                        Namespace = "System.Collections.Generic"
                    },
                    new TypeInfo
                    {
                        Name = "K"
                    },
                    new TypeInfo
                    {
                        Name = "V"
                    }
                },
                ParameterNames = new Dictionary<string, string>
                {
                     { "dict", "" },
                     { "key", "" },
                     { "defaultValue", "" }
                }
            };

            var value = MethodParameterConverter.CreateMethodParameterText(member, true);

            Assert.AreEqual(exp, value);
        }

        [TestMethod()]
        public void ResolveGenericsTypeToHtmlTest()
        {
            var exp = "System.Collections.Generic.Dictionary&lt;K, V&gt;";
            var value = MethodParameterConverter.ResolveGenericsTypeToHtml("System.Collections.Generic.Dictionary<K, V>");
            Assert.AreEqual(exp, value);
        }
    }
}
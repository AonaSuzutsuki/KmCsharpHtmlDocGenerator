using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// Test class.
    /// </summary>
    public class TestClass
    {

        /// <summary>
        /// Test value.
        /// </summary>
        /// <value>Value.</value>
        public int Value { get; }

        /// <summary>
        /// Test constructor.
        /// </summary>
        /// <param name="text"></param>
        public TestClass(int ivalue, string svalue)
        {
            int.TryParse(svalue, out var value);
            Value = value;
        }

        /// <summary>
        /// Test method.
        /// </summary>
        /// <returns>Return int.</returns>
        public int Method()
        {
            return 0;
        }
    }
}

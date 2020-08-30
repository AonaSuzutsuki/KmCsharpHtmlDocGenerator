using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLDocumentToHtmlCUI.Parser
{
    /// <summary>
    /// Partial1
    /// </summary>
    public partial class PartialTest
    {
        /// <summary>
        /// 
        /// </summary>
        public int Test { get; set; }
    }

    /// <summary>
    /// Partial2
    /// </summary>
    public partial class PartialTest
    {
        /// <summary>
        /// 
        /// </summary>
        public int Test2 => Test;
    }
}

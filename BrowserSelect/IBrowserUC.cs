using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrowserSelect
{
    internal interface IBrowserUC
    {
        bool Always { get; set; }
        
        Browser Browser { get; }
    }
}

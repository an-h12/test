using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkBackAutoTest
{
    class UIAppInfo
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string WindowTitle { get; set; }
        public string ExecutablePath { get; set; }

        public string DisplayText 
        { 
            get { return ProcessName + "-" + WindowTitle; } 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkBackAutoTest
{
    class DeviceInfo
    {
        public string modelName { get; set; }
        public string binaryName { get; set; }
        public string serial { get; set; }
        public string type { get; set; }
        public string branch { get; set; }
        public string os { get; set; }

        public DeviceInfo()
        {

        }
        public DeviceInfo(string _modelName, string _binaryName, string _serial, string _type, string _branch,string _os)
        {
            modelName = _modelName;
            binaryName = _binaryName;
            serial = _serial;
            type = _type;
            branch = _branch;
            os = _os;
        }

       
    }
}

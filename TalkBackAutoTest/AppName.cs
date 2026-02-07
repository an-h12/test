using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkBackAutoTest
{
    class AppName
    {
        public string appName { get; set; }
        public string pkgName { get; set; }
        public bool supportMonkey { get; set; }

        public AppName()
        {
        }
        public AppName(string _appName, string _pkgName, bool _supportMonkey)
        {
            appName = _appName;
            pkgName = _pkgName;
            supportMonkey = _supportMonkey;
        }

    }
}

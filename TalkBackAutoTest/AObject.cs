using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TalkBackAutoTest
{
    class AObject
    {
        public int no { get; set; }
        public bool isShow { get; set; }
        public bool isChecked { get; set; }
        public string activityName { get; set; }
        public AObject()
        {
            no = 0;
        }
        
        public AObject(int _no, bool _isShow, bool _isChecked, string _activityName)
        {
            no = _no;
            isShow = _isShow;
            isChecked = _isChecked;
            activityName = _activityName;
        }
    }
}

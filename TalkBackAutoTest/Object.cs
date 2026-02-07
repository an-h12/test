using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TalkBackAutoTest
{
    class Object
    {
        public int no { get; set; }
        public string focusedObject { get; set; }
        public string screen { get; set; }
        public string package { get; set; }
        public string pkgVersion { get; set; }
        public string objectInformation { get; set; }
        public string talkbackText { get; set; }
        public string result { get; set; }
        public string testingtime { get; set; }
        public string remark { get; set; }

        public string testingmode { get; set; }
        public string deviceInfo { get; set; }

        public string fullContent { get; set; }
        public string errortype { get; set; }

        public Object()
        {
            no = 0;
        }
        //public Object(int _no,string _focusedObject, string _screen, string _package, string _pkgVersion, string _objectInformation, string _talkbackText, string _result,string _remark)
        //{
        //    no = _no;
        //    focusedObject = _focusedObject;
        //    screen = _screen;
        //    package = _package;
        //    pkgVersion = _pkgVersion;
        //    objectInformation = _objectInformation;
        //    talkbackText = _talkbackText;
        //    result = _result;

        //    testingtime = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        //    remark = _remark;
        //}

        //public Object(int _no, string _focusedObject, string _screen, string _package, string _pkgVersion, string _objectInformation, string _talkbackText, string _result, string _testtingtime,string _remark)
        //{
        //    no = _no;
        //    focusedObject = _focusedObject;
        //    screen = _screen;
        //    package = _package;
        //    pkgVersion = _pkgVersion;
        //    objectInformation = _objectInformation;
        //    talkbackText = _talkbackText;
        //    result = _result;

        //    testingtime = _testtingtime;
        //    remark = _remark;
        //}

        public Object(int _no, string _focusedObject, string _screen, string _package, string _pkgVersion, string _objectInformation, string _talkbackText, string _result, string _testingtime, string _remark, string _testingmode, string _deviceinfo, string _errortype)
        {
            no = _no;
            focusedObject = _focusedObject;
            screen = encodeText(_screen);
            package = _package;
            pkgVersion = _pkgVersion;
            objectInformation = EscapeXml(_objectInformation);
            talkbackText = encodeText(_talkbackText);
            result = _result;

            testingtime = _testingtime;
            testingmode = _testingmode;
            remark = encodeText(_remark);
            //remark = _remark;
            deviceInfo = _deviceinfo;

            errortype = _errortype;
        }

        private string encodeText(string s)
        {      
            return s.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("&","&amp;");
        }

        private string xmlEncodeName(string s)
        {
            return XmlConvert.EncodeName(s);
        }

        private string EscapeXml(string s)
        {
            string toxml = s;
            if (!string.IsNullOrEmpty(toxml))
            {
                // replace literal values with entities
                toxml = toxml.Replace("&", "&amp;");
                toxml = toxml.Replace("'", "&apos;");
                toxml = toxml.Replace("\"", "&quot;");
                toxml = toxml.Replace(">", "&gt;");
                toxml = toxml.Replace("<", "&lt;");
            }
            return toxml;
        }

        

    }
}

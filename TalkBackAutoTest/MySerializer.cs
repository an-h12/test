using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TalkBackAutoTest
{
    public class MySerializer<T> where T : class
    {
        public static string Serialize(List<T> objs)
        {
            string s = "";
            foreach (T obj in objs)
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
                using (var sww = new StringWriter())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented })
                    {
                        xsSubmit.Serialize(writer, obj);
                        s += sww.ToString() + "\r\n";
                        //return sww.ToString();
                    }
                }
            }

            return s;
        }
    }
}

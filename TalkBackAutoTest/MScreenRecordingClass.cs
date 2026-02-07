using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TalkBackAutoTest
{
    class MScreenRecordingClass
    {

        Process pA = new Process();
        int pId = 0;
        //Android
        public void startRecordScreenAndroid(string path, string fileName, string serial)
        {
            pA.StartInfo.RedirectStandardOutput = true;
            pA.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            string s = "/c adb -s " + serial + " shell screenrecord --bit-rate 1000000 --time-limit 180 --verbose /data/local/tmp/" + fileName;
            pA.StartInfo.Arguments = s;
            pA.StartInfo.CreateNoWindow = true;
            pA.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pA.StartInfo.UseShellExecute = false;
            pA.Start();
            Thread.Sleep(1000);

            //foreach (var adb in Process.GetProcessesByName("adb"))
            //{
            //    pId = adb.Id;
            //    break;
            //}

            //gte process id lasted

            //while (pA.MainWindowHandle == IntPtr.Zero)
            //{
            //    pA.Refresh();
            //}
            //Minimize(pA);


        }

        public void stopRecordScreenAndroid(string path, string fileName, string serial, bool isDeleteFile = false)
        {
            Thread.Sleep(1000);
            if (pA != null)
            {
                ////pA.CloseMainWindow();
                ////pA.CloseMainWindow();

                //foreach (var adb in Process.GetProcessesByName("adb"))
                //{
                //    //if (adb.Id == pId)
                //    //{
                //    adb.Kill();
                //    break;
                //    //}
                //}
                //Thread.Sleep(2000);

                //// pA.Kill();
                ////pA.WaitForExit();
                ////Thread.Sleep(30000);//optimize

                //Thread.Sleep(2000);//optimize
                Process pS = new Process();
                pS.StartInfo.RedirectStandardOutput = true;
                pS.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                string s1 = "/c adb -s " + serial + " shell pkill -2 screenrecord";
                pS.StartInfo.Arguments = s1;
                pS.StartInfo.CreateNoWindow = true;
                pS.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pS.StartInfo.UseShellExecute = false;
                pS.Start();
                Thread.Sleep(3000);//optimize


                Process pB = new Process();
                pB.StartInfo.RedirectStandardOutput = true;
                pB.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                string s = "";
                if (isDeleteFile == false)
                {
                    s = "/c adb -s " + serial + " pull /data/local/tmp/" + fileName + " " + path + "\\" + fileName;
                }
                else
                {
                    s = "/c adb -s " + serial + " pull /data/local/tmp/" + fileName + " " + path + "\\" + fileName +" && "+"adb -s " + serial + " shell rm /data/local/tmp/" + fileName;;
                }
                pB.StartInfo.Arguments = s;
                pB.StartInfo.CreateNoWindow = true;
                pB.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pB.StartInfo.UseShellExecute = false;
                pB.Start();
                pB.WaitForExit();



            }
        }


        string FFMPEG = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\ffmpeg.exe";
        Process p0 = new Process();





        public void startRecordScreen(string path, string fileName)
        {

            p0.StartInfo.RedirectStandardOutput = true;
            p0.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            //string s = "/c \"" + FFMPEG + "\" -f gdigrab -framerate 30 -i desktop -vcodec mpeg4 " + path + "\\" + fileName;
            string s = "/c \"" + FFMPEG + "\" -f gdigrab -framerate 30 -i desktop -preset ultrafast -pix_fmt yuv420p " + path + "\\" + fileName;
            p0.StartInfo.Arguments = s;
            p0.StartInfo.CreateNoWindow = false;

            p0.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p0.StartInfo.UseShellExecute = false;
            p0.Start();
            while (p0.MainWindowHandle == IntPtr.Zero)
            {
                p0.Refresh();
            }
            Minimize(p0);

        }


        public void stopRecordScreen()
        {
            if (p0 != null)
            {
                sendkeys("q");
            }
        }

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lp1, string lp2);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public Process telnet = new Process();

        public void sendkeys(string data)
        {
            bool send = true;
            Process s = Process.GetProcessById(p0.Id);
            while (s.ProcessName == p0.ProcessName && send)
            {
                while (send)
                {
                    IntPtr h = s.MainWindowHandle;
                    SetForegroundWindow(h);
                    SendKeys.SendWait(data);
                    Thread.Sleep(1000);
                    send = false;
                }
            }
        }

        private const int SW_HIDE = 0;
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        // more here: http://www.pinvoke.net/default.aspx/user32.showwindow

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void Minimize(Process p)
        {
            bool send = true;
            Process s = Process.GetProcessById(p.Id);
            while (s.ProcessName == p.ProcessName && send)
            {
                while (send)
                {
                    IntPtr h = s.MainWindowHandle;
                    //SetForegroundWindow(h);
                    ShowWindow(h, SW_MINIMIZE);
                    Thread.Sleep(1000);
                    send = false;
                }
            }
        }




    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation;


namespace TalkBackAutoTest
{
    class PCTB
    {

        // Import các Windows API cần thiết
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetActiveWindow(IntPtr hWnd);
        // Các hằng số cho ShowWindow
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int SW_MAXIMIZE = 3;


        public bool ActivateWindowByPID(int pid)
        {
            try
            {
                // Lấy process từ PID
                Process process = Process.GetProcessById(pid);
            
                // Kiểm tra xem process có cửa sổ chính không
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    Console.WriteLine("Process {pid} không có cửa sổ chính.");
                    return false;
                }
            
                IntPtr hWnd = process.MainWindowHandle;
            
                // Nếu cửa sổ đang minimized, khôi phục lại
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
                else
                {
                    // Hiển thị cửa sổ
                    ShowWindow(hWnd, SW_SHOW);
                }
            
                // Đưa cửa sổ lên foreground
                SetForegroundWindow(hWnd);
            
                // Set làm cửa sổ active
                SetActiveWindow(hWnd);
            
                Console.WriteLine("Đã kích hoạt cửa sổ cho PID: {pid}");
                return true;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Không tìm thấy process với PID: {pid}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi kích hoạt cửa sổ: {ex.Message}");
                return false;
            }
        }





        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int VK_TAB = 0x09;
        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_LWIN = 0x5B;
        private const byte VK_RETURN = 0x0D;

        private const string NarratorProcessName = "narrator";

        #region Narattor

        private void senHotKey()
        {
            // Nhấn Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_EXTENDEDKEY, 0);

            // Nhấn Win
            keybd_event(VK_LWIN, 0, KEYEVENTF_EXTENDEDKEY, 0);

            // Nhấn Enter
            keybd_event(VK_RETURN, 0, KEYEVENTF_EXTENDEDKEY, 0);

            // Thả Enter
            keybd_event(VK_RETURN, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            // Thả Win
            keybd_event(VK_LWIN, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            // Thả Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public void StartNarratorNVDA()
        {

            if (TalkBackAutoTest.Properties.Settings.Default.pcmethodouput == 1)//narrator
            {
                //UIAutomationHelper.GetUIDumpForAppById(26164);
                if (haveNarratorProcess() == false)
                {
                    System.Console.Write("Ongoing Start Narrator");
                    System.Threading.Thread.Sleep(5000);
                    senHotKey();
                    System.Threading.Thread.Sleep(20000);
                }
                else
                {
                    System.Console.Write("Existed Narrator");
                }
            }
            else
            {
                System.Console.Write("Ongoing Start NVDA");
            }
        }

        public void StopNarratorNVDA()
        {
            if (TalkBackAutoTest.Properties.Settings.Default.pcmethodouput == 1)//narrator
            {
                if (haveNarratorProcess() == true)
                {
                    System.Console.Write("Ongoing Stop Narrator");
                    System.Threading.Thread.Sleep(3000);
                    senHotKey();
                    System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    System.Console.Write("Stopped Narrator");
                }
            }
            else
            {
                System.Console.Write("Ongoing Stop NVDA");
            }
        }

        public bool IsNarratorRunning()
        {
            System.Console.Write("Ongoing Check status of Narrator");
            return true;
        }

        public void getNarratorOutput()
        {
            System.Console.Write("Ongoing getNarratorOutput");
        }
        #endregion narrator

        #region nvda

        public void StartNVDA()
        {
            System.Console.Write("Ongoing Start Narrator");
        }

        public void StopNVDA()
        {
            System.Console.Write("Ongoing Stop Narrator");
        }

        public bool IsNVDARunning()
        {
            System.Console.Write("Ongoing Check status of Narrator");
            return true;
        }
        public void getNVDAOutput()
        {
            System.Console.Write("Ongoing getNVDAOutput");
        }

        #endregion nvda


        #region Execution
        public void gotoScreen(string appName)
        {
            System.Console.Write("Ongoing gotoScreen");
            //Process.Start("SamsungNotes.exe");
        }

        public void changeFocus()
        {
            System.Console.Write("Ongoing changeFocus");

            keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY, 0);
            // Thả Ctrl
            keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public void dumpScreen()
        {
            System.Threading.Thread.Sleep(3000);
            System.Console.Write("Ongoing dumpScreen");
            //    Process[] notepadProcesses = Process.GetProcessesByName("SamsungNotes");

            //        if (notepadProcesses.Length > 0)
            //        {
            //            IntPtr notepadHandle = notepadProcesses[0].MainWindowHandle;

            //            Console.WriteLine("=== UI Dump của Notepad ===");
            //            string uiDump = UIAutomationHelper.GetUIDump(notepadHandle);
            //            Console.WriteLine(uiDump);

            //        }

            //    // Ví dụ 2: Lấy UI dump theo process name
            //    Console.WriteLine("\n=== UI Dump của Calculator ===");
            //    string calcDump = UIAutomationHelper.GetUIDumpForApp("Calculator");
            //    Console.WriteLine(calcDump);
        }

        public void dumpFocusedObject()
        {
            System.Console.Write("Ongoing dumpFocusedObject");
        }

        public void checkIssue(string objectName, string textOutput)
        {
            System.Console.Write("Ongoing checkIssue");
        }

        #endregion Execution


        #region ouput

        public void getScreenShot()
        {
            System.Console.Write("Ongoing getScreenShot");
        }

        public void StartRecordVideo(string videoPath)
        {
            System.Console.Write("Ongoing StartRecordVideo");
        }

        public void StopRecordVideo()
        {
            System.Console.Write("Ongoing StopRecordVideo");
        }

        public void cleanLogPC()
        {
            System.Console.Write("Ògoing cleanLogPC");
        }

        public void getObjectScreenShot()
        {
            System.Console.Write("Ongoing getObjectScreenShot");
        }

        public void getLogPC()
        {
            System.Console.Write("Ongoing getLogPC");
        }

        #endregion ouput


        #region format
        public void generatePLMFormat()
        {
            System.Console.Write("Ongoing generatePLMFormat");
        }

        public void generateJirraFormat()
        {
            System.Console.Write("Ongoing generateJirraFormat");
        }

        #endregion format


        #region other

        public UIAppInfo startUWPApp(string uwpP = "SAMSUNGELECTRONICSCoLtd.SamsungNotes_wyx1vj98g3asy!App")
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-command \"Start-Process 'shell:AppsFolder\\" + uwpP + "'\"",
                //Arguments = "-command \"Start-Process 'narrator.exe'\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            
            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Không thể mở . Mã lỗi: " + process.ExitCode);
                    return null;
                }
                else
                {
                    Console.WriteLine("Đã mở ứng dụng  thành công!");
                    System.Threading.Thread.Sleep(3000);
                    List<UIAppInfo> lists = GetRunningUIApplications();
                    //update cbbox voi selectted later
                    foreach(UIAppInfo x in lists)
                    {
                        if (x.WindowTitle.Contains("Notes"))
                        {
                           return x;
                        }
                    }
                    return null;
                }
            }
        }

        public string checkPCEnvironemnet()
        {
            //return "Ongoing checkPCEnvironemnet";
            return "";
        }

        public bool haveNarratorProcess()
        {
            var uiApps = GetRunningUIApplications();
            foreach (UIAppInfo x in uiApps)
            {
                if (x.ProcessName.ToString().ToLower().Contains("narrator"))
                {
                    return true;
                }
            }
            return false;
        }

        public List<UIAppInfo> GetRunningUIApplications()
        {
            var uiApps = new List<UIAppInfo>();

            // Lấy tất cả các process đang chạy
            var processes = Process.GetProcesses();

            try
            {
                // Lọc các process có cửa sổ chính
                foreach (var process in processes.Where(p => p.MainWindowHandle != IntPtr.Zero))
                {
                    try
                    {
                        uiApps.Add(new UIAppInfo
                        {
                            ProcessName = process.ProcessName,
                            ProcessId = process.Id,
                            WindowTitle = process.MainWindowTitle,
                            ExecutablePath = GetProcessPath(process)
                        });
                    }
                    catch
                    {
                        // Bỏ qua process không thể truy cập
                        continue;
                    }
                }
            }
            finally
            {
                // Giải phóng tài nguyên
                foreach (var process in processes)
                {
                    process.Dispose();
                }
            }

            return uiApps;
        }

        private static string GetProcessPath(Process process)
        {
            try
            {
                return process.MainModule.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion other


        public Object getFocusedObjectFake(int objIdx)
        {
            AutomationElement focusedElement = AutomationElement.FocusedElement;

            string[] values = { "Pass", "Pass", "Pass", "Pass", "Fail", "Consider" };
            Random random = new Random();
            int index = random.Next(values.Length);
            string randomResult = values[index];
            string ErrorType = randomResult;

            return new Object(objIdx, "OBJID_" + objIdx, "currentScreen_" + objIdx, "package_" + objIdx, "packageVersion_" + objIdx, "objectInformation", "talkbackText", randomResult, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "allTextDes_optimize", "random_mode", "NA", ErrorType);
        }
    }


    #region automator
    public class UIAutomationDumper
    {
        public static string GetFullUIDump(IntPtr windowHandle)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                AutomationElement rootElement = AutomationElement.FromHandle(windowHandle);
                int a = 5;

            }
            catch (Exception ex)
            {
                sb.AppendLine("Lỗi: {ex.Message}");
            }

            return sb.ToString();
        }
    }

    public class UIAutomationHelper
    {
        public static string GetUIDump(IntPtr windowHandle)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                AutomationElement rootElement = AutomationElement.FromHandle(windowHandle);
                DumpElementTree(rootElement, sb, 0);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Lỗi: {ex.Message}");
            }

            return sb.ToString();
        }

        public static string GetUIDumpForApp(string processName)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                // Tìm process
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    return "Không tìm thấy process: {processName}";
                }

                // Lấy main window
                AutomationElement rootElement = AutomationElement.FromHandle(processes[0].MainWindowHandle);
                DumpElementTree(rootElement, sb, 0);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Lỗi: {ex.Message}");
            }

            return sb.ToString();
        }


        public static string GetUIDumpForAppById(int processId = 26164)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                // Tìm process
                System.Diagnostics.Process pro = System.Diagnostics.Process.GetProcessById(processId);

                if (pro == null)
                {
                    return "Không tìm thấy process: {processName}";
                }

                // Lấy main window
                AutomationElement rootElement = AutomationElement.FromHandle(pro.MainWindowHandle);
                DumpElementTree(rootElement, sb, 0);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Lỗi: {ex.Message}");
            }

            return sb.ToString();
        }

        private static void DumpElementTree(AutomationElement element, StringBuilder sb, int level)
        {
            string indent = new string(' ', level * 2);

            // Lấy thông tin element
            string name = element.Current.Name;
            string className = element.Current.ClassName;
            string automationId = element.Current.AutomationId;
            string controlType = element.Current.ControlType.ProgrammaticName;
            bool isEnabled = element.Current.IsEnabled;
            bool isVisible = element.Current.IsOffscreen;

            sb.AppendLine("{indent}[{controlType}]");
            sb.AppendLine("{indent}  Name: {name}");
            sb.AppendLine("{indent}  ClassName: {className}");
            sb.AppendLine("{indent}  AutomationId: {automationId}");
            sb.AppendLine("{indent}  Enabled: {isEnabled}");
            sb.AppendLine();

            // Đệ quy qua các con
            foreach (AutomationElement child in element.FindAll(TreeScope.Children, Condition.TrueCondition))
            {
                DumpElementTree(child, sb, level + 1);
            }
        }
    #endregion uiautomator





//        ## Lệnh CMD để start UWP app qua Package ID

//**Quan trọng:** UWP app không start trực tiếp bằng Package ID, mà cần dùng **AUMID** (Application User Model ID).

//### 1. Cách tìm AUMID từ Package ID

//```cmd
//# Liệt kê tất cả UWP app với AUMID
//powershell "Get-AppxPackage | Select-Object Name, PackageFamilyName"

//# Tìm AUMID cụ thể cho một Package Name
//powershell "Get-AppxPackage 'Microsoft.WindowsCalculator' | Select-Object Name, PackageFamilyName"
//```

//### 2. Lệnh CMD start UWP app bằng AUMID

//```cmd
//# Cách 1: Dùng shell:AppsFolder
//start shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb3d8bbwe!App

//# Cách 2: Dùng PowerShell
//powershell -command "Start-Process 'shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb3d8bbwe!App'"

//# Cách 3: Dùng explorer
//explorer shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb

//        ## Lệnh CMD để start UWP app qua Package ID

//**Quan trọng:** UWP app không start trực tiếp bằng Package ID, mà cần dùng **AUMID** (Application User Model ID).

//### 1. Cách tìm AUMID từ Package ID

//```cmd
//# Liệt kê tất cả UWP app với AUMID
//powershell "Get-AppxPackage | Select-Object Name, PackageFamilyName"

//# Tìm AUMID cụ thể cho một Package Name
//powershell "Get-AppxPackage 'Microsoft.WindowsCalculator' | Select-Object Name, PackageFamilyName"
//```

//### 2. Lệnh CMD start UWP app bằng AUMID

//```cmd
//# Cách 1: Dùng shell:AppsFolder
//start shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb3d8bbwe!App

//# Cách 2: Dùng PowerShell
//powershell -command "Start-Process 'shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb3d8bbwe!App'"

//# Cách 3: Dùng explorer
//explorer shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb


    }





}
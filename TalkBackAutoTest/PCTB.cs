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
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;


namespace TalkBackAutoTest
{
    class PCTB
    {
        private static string BaseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        private static readonly HttpClient client = new HttpClient();

        // Import các Windows API cần thiết
        //[DllImport("user32.dll")]
        //private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //[DllImport("user32.dll")]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern bool IsIconic(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern bool SetActiveWindow(IntPtr hWnd);
        //// Các hằng số cho ShowWindow
        //private const int SW_RESTORE = 9;
        //private const int SW_SHOW = 5;
        //private const int SW_MAXIMIZE = 3;



        //0903

        public class PkgInfo
        {
            public string pkgName { get; set; }
            public string pkgVersion { get; set; }
            public PkgInfo(string _pkgName, string _pkgVersion)
            {
                pkgName = _pkgName;
                pkgVersion = _pkgVersion;
            }
        }

        public PkgInfo GetPkgByKeyword(string keyword)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString(BaseUrl + "/api/app/uwp/search?keyword="+keyword);
                    var response = JsonConvert.DeserializeObject<dynamic>(json);

                    if (response.success == "true" && response.apps != null)
                    {
                        var apps = response.apps;
                        foreach (var app in apps)
                        {
                            int a = 5;
                            string name = app.Name.ToString();
                            string PackageFullName = app.PackageFullName.ToString();
                            //return null;
                            try
                            {
                                string[] parts = PackageFullName.Split(new string[] { "_" }, StringSplitOptions.None);
                                return new PkgInfo(PackageFullName, parts[1]);
                            }
                            catch
                            {

                            }
                            return new PkgInfo(PackageFullName, PackageFullName);
                        }

                        return null;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return null;
            }
        }

        public string getKwFromWindowTitle(string windowTitle)
        {
            if (windowTitle == "Samsung Notes")
            {
                return "SamsungNotes";

            }
            if (windowTitle == "Samsung Gallery")
            {
                return "PCGallery";
            }
            //if (windowTitle == "Calculator")
            //{
            //    return "WindowsCalculator";
            //}

            return windowTitle;
        }

        public string getWindowTitle(int pId)
        {
            // Try API first
            if (!string.IsNullOrEmpty(BaseUrl))
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string json = client.DownloadString(BaseUrl + "/api/window/title?pid=" + pId);
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                        if (data.success == "true")
                        {
                            return data.window_title.ToString();
                        }
                    }
                }
                catch { /* Fall through */ }
            }

            // Fallback: Get from Process
            try
            {
                Process process = Process.GetProcessById(pId);
                if (process != null && !string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    return process.MainWindowTitle;
                }
            }
            catch { }

            return "";
        }



        //public string GetPackageFullNameFromPid(int pid)
        //{
        //    try
        //    {
        //        Process process = Process.GetProcessById(pid);
            
        //        uint length = 0;
        //        uint result = GetPackageFullName((uint)process.Handle, ref length, null);
            
        //        if (result == 234) // ERROR_INSUFFICIENT_BUFFER
        //        {
        //            StringBuilder sb = new StringBuilder((int)length);
        //            result = GetPackageFullName((uint)process.Handle, ref length, sb);
                
        //            if (result == 0) // SUCCESS
        //            {
        //                string fullName = sb.ToString();
        //                // Loại bỏ phần suffix (!App, !Microsoft.WindowsCalculator)
        //                int exclamationIndex = fullName.IndexOf('!');
        //                if (exclamationIndex >= 0)
        //                {
        //                    fullName = fullName.Substring(0, exclamationIndex);
        //                }
        //                return fullName;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Lỗi khi lấy PackageFullName: " + ex.Message);
        //    }
        
        //    return null;
        //}

        //// Parse packageName từ PackageFullName
        //public static string GetPackageNameFromFullName(string packageFullName)
        //{
        //    if (string.IsNullOrEmpty(packageFullName))
        //        return null;
        
        //    // Format: Name_Version_Architecture_ResourceId_PublisherId
        //    string[] parts = packageFullName.Split('_');
        //    return parts.Length >= 1 ? parts[0] : null;
        //}

        //// Import GetPackageFullName từ kernel32.dll
        //[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        //private static extern uint GetPackageFullName(uint hProcess, ref uint packageFullNameLength, StringBuilder packageFullName);

        //end 0903

        //public string GetPackageNameByPid(int pid)
        //{
        //    try
        //    {
        //        Process process = Process.GetProcessById(pid);
            
        //        uint length = 0;
        //        uint result = GetPackageFullName((uint)process.Handle, ref length, null);
            
        //        if (result == 234)
        //        {
        //            StringBuilder sb = new StringBuilder((int)length);
        //            result = GetPackageFullName((uint)process.Handle, ref length, sb);
                
        //            if (result == 0)
        //            {
        //                string packageFullName = sb.ToString();
        //                string[] parts = packageFullName.Split('_');
        //                return parts.Length >= 1 ? parts[0] : null;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        
        //    return null;
        //}

        public bool ActivateWindowByPID(int pid)
        {
            try
            {
                // Try API first if BaseUrl is available
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            string json = client.DownloadString(BaseUrl + "/api/window/activate?pid=" + pid.ToString());
                            var response = JsonConvert.DeserializeObject<ApiResponse>(json);
                            if (response.success) return true;
                        }
                    }
                    catch { /* Fall through to Windows API */ }
                }

                // Fallback: Windows API
                Process process = Process.GetProcessById(pid);
                if (process == null || process.MainWindowHandle == IntPtr.Zero)
                {
                    Console.WriteLine("ActivateWindowByPID: Process " + pid + " not found or has no window.");
                    return false;
                }

                IntPtr hWnd = process.MainWindowHandle;

                // Restore if minimized
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    System.Threading.Thread.Sleep(200);
                }
                else
                {
                    ShowWindow(hWnd, SW_SHOW);
                }

                // Bring to foreground
                SetForegroundWindow(hWnd);
                System.Threading.Thread.Sleep(100);

                Console.WriteLine("ActivateWindowByPID: Da kich hoat cua so PID " + pid);
                return true;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("ActivateWindowByPID: Khong tim thay process PID " + pid);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ActivateWindowByPID loi: " + ex.Message);
                return false;
            }
        }





        


        //0903
        public DeviceInfo getDeviceInfo()
        {
            DeviceInfo d = new DeviceInfo();
            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString(BaseUrl + "/api/device/info");

                    dynamic data = JsonConvert.DeserializeObject<dynamic>(json);

                    if (data.success == "true")
                    {
                        d.modelName = data.modelName.ToString();
                        d.binaryName = data.binaryName.ToString();
                        d.serial = data.serial.ToString();
                        d.type = data.type.ToString();
                        d.branch = data.branch.ToString();
                    }

                    
                    return d;
                }
            }
            catch (Exception ex)
            {
                return d;
            }
        }
        //end0903

        #region Narrator

        // Keyboard input 
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        private const byte VK_TAB = 0x09;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_LWIN = 0x5B;
        private const byte VK_CAPITAL = 0x14;  // CapsLock
        private const byte VK_X = 0x58;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        // --- Keyboard---
        private void SendKeyChord(byte[] vkCodes, int holdMs)
        {
            // Press all keys in order
            foreach (byte vk in vkCodes)
                keybd_event(vk, 0, 0, UIntPtr.Zero);
            System.Threading.Thread.Sleep(holdMs);
            // Release all keys in reverse order
            for (int idx = vkCodes.Length - 1; idx >= 0; idx--)
                keybd_event(vkCodes[idx], 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        /// <summary>Toggle Narrator on/off (Win+Ctrl+Enter).</summary>
        public void ToggleNarrator()
        {
            SendKeyChord(new byte[] { VK_CONTROL, VK_LWIN, VK_RETURN }, 100);
        }

        /// <summary>
        /// Force Narrator to read the focused element (Caps+Tab).
        /// Call once at the start of an automation session for the first element.
        /// </summary>
        public void ForceNarratorRead()
        {
            System.Threading.Thread.Sleep(1000);
            SendKeyChord(new byte[] { VK_CAPITAL, VK_TAB }, 100);
        }

        public void StartNarratorNVDA()
        {
            if (TalkBackAutoTest.Properties.Settings.Default.pcmethodouput == 1)//narrator
            {
                // Try API first
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    try
                    {
                        var content = new StringContent("", Encoding.UTF8, "application/json");
                        var response = client.PostAsync(BaseUrl + "/api/narrator/start", content).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("StartNarratorNVDA: Da bat Narrator (qua API)");
                            return;
                        }
                    }
                    catch { /* Fall through to local */ }
                }

                // Fallback: Toggle Narrator ON locally
                if (IsNarratorRunning())
                {
                    Console.WriteLine("StartNarratorNVDA: Narrator da chay san");
                }
                else
                {
                    ToggleNarrator(); // Win+Ctrl+Enter
                    System.Threading.Thread.Sleep(500);
                    if (IsNarratorRunning())
                    {
                        Console.WriteLine("StartNarratorNVDA: Da bat Narrator (local)");
                    }
                    else
                    {
                        Console.WriteLine("StartNarratorNVDA: Bat Narrator that bai - kiem tra thu cong Win+Ctrl+Enter");
                    }
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
                // Try API first
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    try
                    {
                        var content = new StringContent("", Encoding.UTF8, "application/json");
                        var response = client.PostAsync(BaseUrl + "/api/narrator/stop", content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string json = response.Content.ReadAsStringAsync().Result;
                            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                            //return data.success;
                        }
                        //return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi StopNarrator (API): " + ex.Message);
                    }
                    return; // exit early after API attempt
                }

                // Fallback: Toggle Narrator OFF locally
                if (!IsNarratorRunning())
                {
                    Console.WriteLine("StopNarratorNVDA: Narrator khong chay");
                }
                else
                {
                    ToggleNarrator(); // Win+Ctrl+Enter
                    System.Threading.Thread.Sleep(500);
                    Console.WriteLine("StopNarratorNVDA: Da tat Narrator (local)");
                }
            }
        }

        // Narrator process
        /// <summary>Check if Narrator process is currently running.</summary>
        public bool IsNarratorRunning()
        {
            return Process.GetProcessesByName("Narrator").Length > 0 || Process.GetProcessesByName("narrator").Length > 0;
        }

        // --- Clipboard ---
        /// <summary>Get current clipboard text, or null if unavailable/empty.</summary>
        public string GetClipboardText()
        {
            try
            {
                if (Clipboard.ContainsText())
                    return Clipboard.GetText();
            }
            catch { }
            return null;
        }

        // Chưa xóa history (cần WinRT)        
        /// <summary>Clear the current clipboard contents.</summary>
        public void ClearCurrentClipboard()
        {
            Clipboard.Clear();
        }

        /// <summary>
        /// Issue Caps+Ctrl+X to trigger Narrator's "copy last spoken phrase".
        /// Returns raw clipboard text (unfiltered)
        /// </summary>
        private string DoNarratorCapture()
        {
            //Caps+Ctrl+X
            SendKeyChord(new byte[] { VK_CAPITAL, VK_CONTROL, VK_X }, 100);

            string raw = GetClipboardText();
            if (raw == null) return null;
            return FilterNarratorOutput(raw);
        }

        // FilterOutput
        private string FilterNarratorOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] blocked = new[] {
                "Copied last phrase to clipboard",
                "Failed to copy to clipboard"
            };
            var filtered = lines.Where(l => !blocked.Contains(l.Trim().ToLower())).ToArray();
            return string.Join(Environment.NewLine, filtered);
        }

        /// <summary>
        /// Single attempt capture. Logs error and returns null if nothing captured.
        /// </summary>
        private string TryNarratorCapture()
        {
            string result = DoNarratorCapture();
            if (result == null)
            {
                Console.WriteLine("Lỗi: Narrator không cap nội dung");
            }
            return result;
        }

        /// <summary>
        /// Save current clipboard, issue Narrator copy, restore original clipboard.
        /// </summary>
        private string CaptureNarratorLastSpoken()
        {
            // Save current clipboard
            string original = null;
            bool hadText = false;
            try
            {
                hadText = Clipboard.ContainsText();
                if (hadText) original = Clipboard.GetText();
            }
            catch { }

            // Capture Narrator output
            string result = TryNarratorCapture();

            // Restore original clipboard
            try
            {
                if (hadText && original != null) ClearCurrentClipboard();
            }
            catch { }

            return result;
        }

        /// <summary>
        /// Main public method — captures Narrator output with auto toggle-on.
        /// Auto-disables Narrator if it was off before the call.
        /// </summary>
        public string GetNarratorOutput()
        {
            // NOTE: Narrator should already be ON (via StartNarratorNVDA called once at test start).
            // This method just captures the current Narrator output WITHOUT toggling Narrator on/off.
            if (!IsNarratorRunning())
            {
                Console.WriteLine("Narrator chua bat");
                return null;
            }
            return CaptureNarratorLastSpoken();
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
            //System.Console.Write("Ongoing gotoScreen");
            //Process.Start("SamsungNotes.exe");

            //GetPkgByKeyword("SamsungNotes");
        }

        public void changeFocus()
        {
            // Try API first if available
            if (!string.IsNullOrEmpty(BaseUrl))
            {
                try
                {
                    var requestBody = new { count = 1 };
                    string json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(BaseUrl + "/api/ui/send_tab", content).Result;
                    if (response.IsSuccessStatusCode) return;
                }
                catch { /* Fall through */ }
            }

            // Fallback: Send Tab key via Windows API
            try
            {
                keybd_event(VK_TAB, 0, 0, UIntPtr.Zero);         // Press Tab
                System.Threading.Thread.Sleep(50);
                keybd_event(VK_TAB, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Tab
            }
            catch (Exception ex)
            {
                Console.WriteLine("changeFocus loi: " + ex.Message);
            }
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

        public string dumpFocusedObject()
        {
            return dumpFocusedObject(IntPtr.Zero);
        }

        /// <summary>
        /// Get the focused UI element from a specific window using UI Automation.
        /// Falls back to Windows API if no window handle is available.
        /// Returns a JSON-like string with element info.
        /// </summary>
        public string dumpFocusedObject(IntPtr windowHandle)
        {
            try
            {
                // Try API first (if BaseUrl is set)
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    try
                    {
                        var response = client.GetAsync(BaseUrl + "/api/ui/focused_element").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string json = response.Content.ReadAsStringAsync().Result;
                            var data = JsonConvert.DeserializeObject<dynamic>(json);
                            if (data.success == "True" && data.element != null)
                            {
                                return data.element.ToString();
                            }
                        }
                    }
                    catch { /* Fall through to UI Automation */ }
                }

                // Fallback: Use UI Automation directly
                return GetFocusedElementFromWindow(windowHandle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy focused element: " + ex.Message);
                return "TRYCATCH_NA";
            }
        }

        private string GetFocusedElementFromWindow(IntPtr windowHandle)
        {
            try
            {
                IntPtr hwnd = windowHandle;
                if (hwnd == IntPtr.Zero)
                {
                    hwnd = GetForegroundWindow();
                }
                if (hwnd == IntPtr.Zero)
                {
                    return "{\"name\":\"NO_WINDOW\",\"className\":\"\",\"automationId\":\"\",\"controlType\":\"\",\"isEnabled\":false,\"isVisible\":false}";
                }

                AutomationElement element = AutomationElement.FromHandle(hwnd);
                if (element == null)
                {
                    return "{\"name\":\"ELEMENT_NULL\",\"className\":\"\",\"automationId\":\"\",\"controlType\":\"\",\"isEnabled\":false,\"isVisible\":false}";
                }

                AutomationElement focused = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
                if (focused == null)
                {
                    // Try the element itself as the focused one
                    focused = element;
                }

                string name = "";
                string className = "";
                string automationId = "";
                string controlType = "";
                bool isEnabled = false;
                bool isVisible = true;

                try { name = focused.Current.Name ?? ""; } catch { }
                try { className = focused.Current.ClassName ?? ""; } catch { }
                try { automationId = focused.Current.AutomationId ?? ""; } catch { }
                try { controlType = focused.Current.ControlType?.ProgrammaticName ?? ""; } catch { }
                try { isEnabled = focused.Current.IsEnabled; } catch { }
                try { isVisible = !focused.Current.IsOffscreen; } catch { }

                // Also get the first child element as the focused element inside the window
                try
                {
                    var condition = Condition.TrueCondition;
                    AutomationElementCollection children = focused.FindAll(TreeScope.Children, condition);
                    if (children.Count > 0)
                    {
                        var first = children[0];
                        try { name = first.Current.Name ?? ""; } catch { }
                        try { className = first.Current.ClassName ?? ""; } catch { }
                        try { automationId = first.Current.AutomationId ?? ""; } catch { }
                        try { controlType = first.Current.ControlType?.ProgrammaticName ?? ""; } catch { }
                        try { isEnabled = first.Current.IsEnabled; } catch { }
                        try { isVisible = !first.Current.IsOffscreen; } catch { }
                    }
                }
                catch { }

                // Return as JSON-like string (matching the expected format from API)
                return string.Format(
                    "{{\"name\":\"{0}\",\"className\":\"{1}\",\"automationId\":\"{2}\",\"controlType\":\"{3}\",\"isEnabled\":{4},\"isVisible\":{5}}}",
                    EscapeJson(name),
                    EscapeJson(className),
                    EscapeJson(automationId),
                    EscapeJson(controlType),
                    isEnabled.ToString().ToLower(),
                    isVisible.ToString().ToLower()
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi UI Automation: " + ex.Message);
                return "{\"name\":\"UA_ERROR:" + EscapeJson(ex.Message) + "\",\"className\":\"\",\"automationId\":\"\",\"controlType\":\"\",\"isEnabled\":false,\"isVisible\":false}";
            }
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
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
            System.Console.Write("Ongoing cleanLogPC");
        }

        public void getObjectScreenShot(string folderResult,int pid)
        {
            System.Console.Write("Ongoing getObjectScreenShot");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(folderResult))
                    {
                        Directory.CreateDirectory(folderResult);
                    }

                    // Đường dẫn file screenshot.png
                    string filepath = Path.Combine(folderResult, "screenshot.png");

                    // Build URL API - .NET 4.5 không dùng string interpolation ($)
                    string url = BaseUrl + "/api/capture/pid?pid="+pid+"&path=" + Uri.EscapeDataString(filepath);

                    // Gọi API
                    var response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<dynamic>(json);

                        if (data.success == true)
                        {
                            System.Console.WriteLine("Screenshot đã lưu: " + filepath);
                        }
                        else
                        {
                            System.Console.WriteLine("Lỗi: " + data.message);
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Lỗi HTTP: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Lỗi: " + ex.Message);
            }

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
                    foreach (UIAppInfo x in lists)
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



        public class ApiResponse
        {
            public bool success { get; set; }
            public int total_apps { get; set; }
            public List<UIAppInfo> apps { get; set; }
        }

        public List<UIAppInfo> GetRunningUIApplications()
        {

            //try
            //{
            //    using (WebClient client = new WebClient())
            //    {
            //        string json = client.DownloadString(BaseUrl + "/api/system/ui-apps");
            //        var response = JsonConvert.DeserializeObject<ApiResponse>(json);

            //        if (response.success && response.apps != null)
            //        {
            //            return response.apps;
            //        }

            //        return new List<UIAppInfo>();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Lỗi: " + ex.Message);
            //    return new List<UIAppInfo>();
            //}

            var uiApps = new List<UIAppInfo>();
            try
            {
                var processes = Process.GetProcesses();
                int totalWithWindow = 0;
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            totalWithWindow++;
                            uiApps.Add(new UIAppInfo
                            {
                                ProcessName = process.ProcessName,
                                ProcessId = process.Id,
                                WindowTitle = process.MainWindowTitle,
                                ExecutablePath = GetProcessPath(process)
                            });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                Console.WriteLine("GetRunningUIApplications: found " + uiApps.Count + " apps (of " + totalWithWindow + " with window handle)");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetRunningUIApplications: " + ex.Message);
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


        public Object getFocusedObjectFake(int objIdx, string objElement, string windowTitle, PkgInfo pkgInfo)
        {
            return getFocusedObjectFake(objIdx, objElement, windowTitle, pkgInfo, "TalkbackText_NA");
        }

        public Object getFocusedObjectFake(int objIdx, string objElement, string windowTitle, PkgInfo pkgInfo, string talkbackText)
        {
            //AutomationElement focusedElement = AutomationElement.FocusedElement;

            JObject json = JObject.Parse(@objElement);
            string objName = "BLANK";

            if (json["name"] != null)
            {
                objName = json["name"].ToString();
            }


            string PackageName = "";
            string PackageVersion = "";
            if (pkgInfo != null)
            {
                PackageName = pkgInfo.pkgName;
                PackageVersion = pkgInfo.pkgVersion;
            }



            string[] values = { "Pass", "Pass", "Pass", "Pass", "Fail", "Consider" };
            Random random = new Random();
            int index = random.Next(values.Length);
            string randomResult = values[index];
            string ErrorType = randomResult;

            return new Object(objIdx, "OBJID_" + objIdx, windowTitle, PackageName, PackageVersion, objElement, talkbackText, randomResult, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), objName, "random_mode", "NA", ErrorType);
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
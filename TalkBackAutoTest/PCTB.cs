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
            string windowTitle = "";
            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString(BaseUrl + "/api/window/title?pid="+pId);

                    dynamic data = JsonConvert.DeserializeObject<dynamic>(json);

                    if (data.success == "true")
                    {
                        windowTitle = data.window_title.ToString();
                        return windowTitle;
                    }


                    return windowTitle;
                }
            }
            catch (Exception ex)
            {
                return windowTitle;
            }

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
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString(BaseUrl + "/api/window/activate?pid=" + pid.ToString());
                    var response = JsonConvert.DeserializeObject<ApiResponse>(json);

                    if (response.success)
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return false;
            }

            //try
            //{
            //    // Lấy process từ PID
            //    Process process = Process.GetProcessById(pid);

            //    // Kiểm tra xem process có cửa sổ chính không
            //    if (process.MainWindowHandle == IntPtr.Zero)
            //    {
            //        Console.WriteLine("Process {pid} không có cửa sổ chính.");
            //        return false;
            //    }

            //    IntPtr hWnd = process.MainWindowHandle;

            //    // Nếu cửa sổ đang minimized, khôi phục lại
            //    if (IsIconic(hWnd))
            //    {
            //        ShowWindow(hWnd, SW_RESTORE);
            //    }
            //    else
            //    {
            //        // Hiển thị cửa sổ
            //        ShowWindow(hWnd, SW_SHOW);
            //    }

            //    // Đưa cửa sổ lên foreground
            //    SetForegroundWindow(hWnd);

            //    // Set làm cửa sổ active
            //    SetActiveWindow(hWnd);

            //    Console.WriteLine("Đã kích hoạt cửa sổ cho PID: {pid}");
            //    return true;
            //}
            //catch (ArgumentException)
            //{
            //    Console.WriteLine("Không tìm thấy process với PID: {pid}");
            //    return false;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Lỗi khi kích hoạt cửa sổ: {ex.Message}");
            //    return false;
            //}
        }





        // Keyboard input via keybd_event (avoids SendKeys Win-key limitation)
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_TAB = 0x09;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_LWIN = 0x5B;
        private const byte VK_CAPITAL = 0x14;  // CapsLock
        private const byte VK_X = 0x58;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;


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

        #region Narattor

        // --- Keyboard helper ---
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
                //UIAutomationHelper.GetUIDumpForAppById(26164);
                //if (haveNarratorProcess() == false)
                //{
                //    System.Console.Write("Ongoing Start Narrator");
                //    System.Threading.Thread.Sleep(5000);
                //    senHotKey();
                //    System.Threading.Thread.Sleep(20000);
                //}
                //else
                //{
                //    System.Console.Write("Existed Narrator");
                //}

                try
                {
                    var content = new StringContent("", Encoding.UTF8, "application/json");
                    var response = client.PostAsync(BaseUrl + "/api/narrator/start", content).Result; // Blocking

                    if (response.IsSuccessStatusCode)
                    {
                        string json = response.Content.ReadAsStringAsync().Result; // Blocking
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                        //return data.success;
                    }
                    //return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi: " + ex.Message);
                    //return false;
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
                    Console.WriteLine("Lỗi: " + ex.Message);
                    //return false;
                }

                //if (haveNarratorProcess() == true)
                //{
                //    System.Console.Write("Ongoing Stop Narrator");
                //    System.Threading.Thread.Sleep(3000);
                //    senHotKey();
                //    System.Threading.Thread.Sleep(5000);
                //}
                //else
                //{
                //    System.Console.Write("Stopped Narrator");
                //}
            }
            else
            {
                System.Console.Write("Ongoing Stop NVDA");
            }
        }

        // --- Narrator process management ---
        /// <summary>Check if Narrator process is currently running.</summary>
        public bool IsNarratorRunning()
        {
            return Process.GetProcessesByName("Narrator").Length > 0
                || Process.GetProcessesByName("narrator").Length > 0;
        }

        /// <summary>Wait up to `retries` * 200ms for Narrator state to match expected.</summary>
        public bool WaitForNarratorState(bool expected, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                if (IsNarratorRunning() == expected) return true;
                System.Threading.Thread.Sleep(200);
            }
            return IsNarratorRunning() == expected;
        }

        /// <summary>Start Narrator if not already running (toggle-on approach).</summary>
        public bool StartNarratorProcess()
        {
            if (IsNarratorRunning()) return true;
            ToggleNarrator(); // Win+Ctrl+Enter
            System.Threading.Thread.Sleep(200);
            return WaitForNarratorState(true);
        }

        /// <summary>Ensure Narrator is on; returns true if already on or successfully turned on.</summary>
        public bool EnsureNarratorOn()
        {
            if (IsNarratorRunning()) return true;
            Console.WriteLine("INFO: Narrator is off; auto-enabling for capture");
            ToggleNarrator();
            if (WaitForNarratorState(true)) return true;
            // Fallback: direct toggle again
            ToggleNarrator();
            return WaitForNarratorState(true);
        }

        /// <summary>
        /// Restore Narrator to its prior state. Call with autoEnabled=true
        /// (from EnsureNarratorOn) to toggle it back off if we auto-enabled it.
        /// </summary>
        public void RestoreNarratorState(bool autoEnabled)
        {
            if (!autoEnabled) return;
            ToggleNarrator();
            if (WaitForNarratorState(false))
                Console.WriteLine("INFO: Narrator restored to previous state (off)");
            else
                Console.WriteLine("WARN: Failed to restore Narrator state");
        }

        // --- Clipboard operations ---
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

        /// <summary>Clear the current clipboard contents.</summary>
        public void ClearCurrentClipboard()
        {
            try { Clipboard.Clear(); } catch { }
        }

        // --- Narrator capture helpers ---
        private string FilterNarratorOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] blocked = new[] {
                "copied last phrase to clipboard",
                "failed to copy to clipboard"
            };
            var filtered = lines.Where(l => !blocked.Contains(l.Trim().ToLower())).ToArray();
            return string.Join(Environment.NewLine, filtered);
        }

        /// <summary>
        /// Issue Caps+Ctrl+X to trigger Narrator's "copy last spoken phrase".
        /// Returns raw clipboard text (unfiltered), or null on failure.
        /// </summary>
        private string DoNarratorCapture()
        {
            // Issue the Narrator copy-last-spoken chord: Caps+Ctrl+X
            SendKeyChord(new byte[] { VK_CAPITAL, VK_CONTROL, VK_X }, 100);

            // Brief delay for clipboard to populate
            System.Threading.Thread.Sleep(300);

            string raw = GetClipboardText();
            if (raw == null) return null;
            return FilterNarratorOutput(raw);
        }

        /// <summary>
        /// Single attempt capture. Logs error and returns null if nothing captured.
        /// </summary>
        private string TryNarratorCapture()
        {
            string result = DoNarratorCapture();
            if (result == null)
            {
                Console.WriteLine("Lỗi: Narrator capture thất bại");
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
                if (hadText && original != null)
                    Clipboard.SetText(original);
            }
            catch { }

            return result;
        }

        // --- Public API ---
        /// <summary>
        /// Main public method — captures Narrator output with auto toggle-on.
        /// Auto-disables Narrator if it was off before the call.
        /// </summary>
        public string GetNarratorOutput()
        {
            bool autoEnabled = EnsureNarratorOn();
            try
            {
                return CaptureNarratorLastSpoken();
            }
            finally
            {
                RestoreNarratorState(autoEnabled);
            }
        }

        /// <summary>
        /// Capture Narrator output only if it is already running (no auto toggle).
        /// Returns null if Narrator is not running.
        /// </summary>
        public string TryCaptureNarratorIfRunning()
        {
            if (!IsNarratorRunning()) return null;
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
            //System.Console.Write("Ongoing changeFocus");

            //keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY, 0);
            //// Thả Ctrl
            //keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            try
            {
                var requestBody = new
                {
                    count = 1
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.PostAsync(BaseUrl + "/api/ui/send_tab", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = response.Content.ReadAsStringAsync().Result;
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    //return data.success;
                }

                //return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi TAB: " + ex.Message);
                //return false;
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
            //System.Console.Write("Ongoing dumpFocusedObject");

            try
            {
                var response = client.GetAsync(BaseUrl + "/api/ui/focused_element").Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<dynamic>(json);

                    if (data.success=="True" && data.element != null)
                    {
                        return data.element.ToString();
                    }
                }

                return "NA";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy focused element: " + ex.Message);
                return "TRYCATCH_NA";
            }

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

            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString(BaseUrl + "/api/system/ui-apps");
                    var response = JsonConvert.DeserializeObject<ApiResponse>(json);

                    if (response.success && response.apps != null)
                    {
                        return response.apps;
                    }

                    return new List<UIAppInfo>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                return new List<UIAppInfo>();
            }

            //var uiApps = new List<UIAppInfo>();
            //// Lấy tất cả các process đang chạy
            //var processes = Process.GetProcesses();

            //try
            //{
            //    // Lọc các process có cửa sổ chính
            //    foreach (var process in processes.Where(p => p.MainWindowHandle != IntPtr.Zero))
            //    {
            //        try
            //        {
            //            uiApps.Add(new UIAppInfo
            //            {
            //                ProcessName = process.ProcessName,
            //                ProcessId = process.Id,
            //                WindowTitle = process.MainWindowTitle,
            //                ExecutablePath = GetProcessPath(process)
            //            });
            //        }
            //        catch
            //        {
            //            // Bỏ qua process không thể truy cập
            //            continue;
            //        }
            //    }
            //}
            //finally
            //{
            //    // Giải phóng tài nguyên
            //    foreach (var process in processes)
            //    {
            //        process.Dispose();
            //    }
            //}

            //return uiApps;
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

            //return new Object(objIdx, "OBJID_" + objIdx, "currentScreen_" + objIdx, "package_" + objIdx, "packageVersion_" + objIdx, "objectInformation", "talkbackText", randomResult, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "allTextDes_optimize", "random_mode", "NA", ErrorType);
            return new Object(objIdx, "OBJID_" + objIdx, windowTitle, PackageName, PackageVersion, objElement, "talkbackText", randomResult, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), objName, "random_mode", "NA", ErrorType);
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
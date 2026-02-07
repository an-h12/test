using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


//using System.Linq;
//using System.Text.RegularExpressions;
using WK.Libraries.BetterFolderBrowserNS;
namespace TalkBackAutoTest
{
    public partial class MainForm : Form
    {

        //
        public string WORKSPACE
        {
            get { return txtWS.Text; }
        }

        int COUNT_IGNORE_MYFILES = 0;
        int COUNT_IGNORE_NOTES = 0;
        int COUNT_IGNORE_GALLERY = 0;
        int COUNT_IGNORE_VOICE = 0;
        int COUNT_IGNORE_CLOCK = 0;

        string GLOBAL_FAIL = "Fail";
        string GLOBAL_PASS = "Pass";
        string GLOBAL_CONSIDER = "Consider";


        bool renderDone = false;

        List<AppName> listAppName = new List<AppName>();
        //intit
        bool HAVECOMMA2 = false;
        int TIMES_DUP = 0;
        //const int TIMEOUT_COMMAND = 110 * 1000;
        //const int TIMEOUT_COMMAND = 110;
        const int TIMEOUT_COMMAND = 500;

        string ALL_APPS = "All Apps";

        string GLOBAL_LANGUAGE = "";

        int GLOBAL_DPI = 0;

        const string TRYCATCH_NA = "TRYCAT_NA";
        int MAX_SCREENSHOT = 5;

        public static string[] listSerial = null;

        Dictionary<string, string> dictPkgVersion = new Dictionary<string, string>();

        //init

        //string PY_OCR_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\py_ocr.py";
        //string PY_OCR_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\py_easyocr.py";
        string PY_OCR_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\py_easyocr_enhance_opencv.py";
        //string PY_LANGID_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\py_langid.py";
        string AI_MODEL_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\AI_Model";


        string AI_MODEL_FILES = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\AI_Model\\craft_mlt_25k.pth";

        string PY_RS_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\rs.txt";

        string ENG_WORDS_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\en_word_list.txt";
        string BLACKLIST_ENG_WORDS_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\blacklist_en_word_list.txt";
        string BLACKLIST_ELEMENT_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\blacklist_element.txt";

        Thread threadRunProject;
        Thread threadRunProject1;
        XDocument doc;
        DeviceInfo device;
        //int numberOfObject = 0;
        List<Object> listObject = new List<Object>();

        //optimize hashmap
        Dictionary<string, Object> hashMap = new Dictionary<string, Object>();


        public static string SetValueForTitle = "";
        public static string SetValueForBody = "";
        public string stressTestPkg = "";


        List<XElement> ScriptsAll = new List<XElement>();

        public MainForm()
        {
            InitializeComponent();
            device = new DeviceInfo();
        }


        private void initProject()
        {
            //update here
            txtWS.Text = TalkBackAutoTest.Properties.Settings.Default.txtWS;

            if (TalkBackAutoTest.Properties.Settings.Default.txtPkg == "")
            {
                TalkBackAutoTest.Properties.Settings.Default.txtPkg = ALL_APPS;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            stressTestPkg = TalkBackAutoTest.Properties.Settings.Default.txtPkg;
            //txtPkg.Text = stressTestPkg;

            //txtScriptFolder.Text = TalkBackAutoTest.Properties.Settings.Default.txtScriptFolder;

            txtEventNumberMax1screen.Text = TalkBackAutoTest.Properties.Settings.Default.txtEventNumberMax1screen;
            txtEventNumberMaxAll.Text = TalkBackAutoTest.Properties.Settings.Default.txtEventNumberMaxAll;

            txtMaximumScreenShot.Text = TalkBackAutoTest.Properties.Settings.Default.txtMaximumScreenShot.ToString() == "" || TalkBackAutoTest.Properties.Settings.Default.txtMaximumScreenShot.ToString() == "0" ? TalkBackAutoTest.Properties.Settings.Default.txtMaximumScreenShot.ToString() : "5";

            cbDisableSysKey.Checked = TalkBackAutoTest.Properties.Settings.Default.cbDisableSysKey;


            int modeTesting = TalkBackAutoTest.Properties.Settings.Default.modeTesting;

            


            txtList_language_word.Text = TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word;
            txtList_repeat_text.Text = TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text;
            txtList_dup_word.Text = TalkBackAutoTest.Properties.Settings.Default.blacklist_dup_word;
            txtList_unlabelled.Text = TalkBackAutoTest.Properties.Settings.Default.blacklist_unlabelled;

            txtList_screen.Text = TalkBackAutoTest.Properties.Settings.Default.blacklist_screen;


            string currentLocale = getCurrentLocale();
            cbLanguage.Text = currentLocale;

            int envMode = TalkBackAutoTest.Properties.Settings.Default.envMode;
            if (envMode == 1)
            {
                rbAndroidMode.Checked = true;
            }
            else
            {
                rbWindowMode.Checked = true;
            }


            if (modeTesting == 1)
            {
                rbStressTestMode.Checked = true;
            }
            else if (modeTesting == 2)
            {
                rbManualMode.Checked = true;
            }
            else if (modeTesting == 3)
            {
                rbActivityMode.Checked = true;
            }

            initKindOfIssue();

            string xmlPath = txtWS.Text + "\\result.xml";


            readXml(@xmlPath);

            readLog();
        }

        private void initKindOfIssue()
        {
            cb_Unlabelled.Checked = TalkBackAutoTest.Properties.Settings.Default.set_Unlabelled;
            cb_NAF.Checked = TalkBackAutoTest.Properties.Settings.Default.set_NAF;
            cb_NoOutput.Checked = TalkBackAutoTest.Properties.Settings.Default.set_NoOutput;
            cb_DupWord.Checked = TalkBackAutoTest.Properties.Settings.Default.set_DupWord;
            cb_RepeatPreviousObject.Checked = TalkBackAutoTest.Properties.Settings.Default.set_RepeatPreviousObject;
            cbDifflang.Checked = TalkBackAutoTest.Properties.Settings.Default.set_DiffLang;
            cb_NoOutputAfterTab.Checked = TalkBackAutoTest.Properties.Settings.Default.set_NoOutputAfterTab;
            cb_NoFocusableHeader.Checked = TalkBackAutoTest.Properties.Settings.Default.set_NoFocusableHeader;
        }

        private void setDarkMode(bool value)
        {
            try
            {
                string sValue = "no";
                if (value == true)//on
                {
                    sValue = "yes";
                }
                else//false
                {
                    sValue = "no";
                }
                RunCommand("adb -s " + device.serial + " shell \"cmd uimode night " + sValue + "\"");
            }
            catch (Exception ex)
            {
                printLog("setDarkMode: " + ex.Message, "error");
            }
        }

        private bool isRepeatWordsStartEnd(string sentence)
        {
            // Loại bỏ dấu câu và chuyển về chữ thường
            //char[] delimiters = { ',', ';'};
            //string cleanedSentence = new string(sentence.Where(c => !delimiters.Contains(c)).ToArray()).ToLower();
            sentence = sentence.ToLower();
            string cleanedSentence = Regex.Replace(sentence, @"[;,]", " ");
            // Tách thành các từ
            string[] words = cleanedSentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Kiểm tra với các độ dài cụm từ khác nhau
            int maxPhraseLength = words.Length / 2;
            for (int len = 1; len <= maxPhraseLength; len++)
            {
                string startPhrase = string.Join(" ", words.Take(len));
                string endPhrase = string.Join(" ", words.Skip(words.Length - len));

                if (startPhrase == endPhrase)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isRepeatWords(string sentence)
        {
            //string pattern = @"\b(\w+)\b(?:\s*,?\s*\1\b)+";
            //bool isRepeated = Regex.IsMatch(sentence, pattern, RegexOptions.IgnoreCase);
            //return isRepeated;

            // Regex tách từ bằng Unicode
            string wordPattern = @"\b[\p{L}]+\b";
            // Tách câu thành từ bằng Unicode
            var matches = Regex.Matches(sentence, wordPattern, RegexOptions.IgnoreCase);

            // Đưa tất cả từ vào danh sách
            List<string> words = new List<string>();
            foreach (Match match in matches)
            {
                words.Add(match.Value.ToLower()); // Đưa về chữ thường để không phân biệt hoa thường
            }

            bool hasRepeats = false;

            // Kiểm tra các cụm từ có độ dài khác nhau (1 từ, 2 từ, ..., n/2 từ)
            for (int length = 1; length <= words.Count / 2; length++)
            {
                for (int start = 0; start + 2 * length <= words.Count; start++)
                {
                    // Ghép cụm từ thành chuỗi con
                    var firstPart = string.Join(" ", words.GetRange(start, length));
                    var secondPart = string.Join(" ", words.GetRange(start + length, length));

                    if (firstPart == secondPart)
                    {
                        hasRepeats = true;
                        break;
                    }
                }
                if (hasRepeats) break;
            }

            return hasRepeats;

        }



        string global_en_word = "";
        private int ischangelang(string sentence, string lang)
        {
            //TBD



            //sentence


            global_en_word = "";
            sentence = sentence.ToLower();
            if (lang == "vi" || lang == "ko")
            {
                // char[] separators = new char[] { ' ', ',', '.', '?', '-' };
                char[] separators = new char[] { ' ', ',', '.', '?' };

                string[] inputWords = sentence.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                //string[] engWords = new string[1000];
                List<string> engWords = new List<string>();

                int idx = 0;

                string[] words = File.ReadAllLines(ENG_WORDS_PATH);
                int count = 0;
                foreach (string word in inputWords)
                {
                    if (word.Length >= 2 && Array.Exists(words, w => w.Equals(word, StringComparison.OrdinalIgnoreCase)))
                    {
                        count++;
                        //engWords[idx++] = word;
                        engWords.Add(word);
                    }
                }

                if (engWords.Count() > 0)
                {
                    foreach (string w in engWords)
                    {
                        global_en_word += w + ", ";
                    }
                    global_en_word = global_en_word.TrimEnd(',', ' ');
                }

                //if (inputWords.Length  == 2 && (float)(count) / inputWords.Length <= 0.5f)//ty le 50% la false
                //{
                //    return 0;//ok pass
                //}

                //else if (inputWords.Length  >=3 && (float)(count) / inputWords.Length < 0.35f)//ty le 80% la false
                //{
                //    return 0;//ok
                //}

                if (inputWords.Length ==1 && count ==1)//chi co 1 tu duy nhat, kha nang cao ten rieng
                {
                    return -1;
                }

                if ((float)(count) / inputWords.Length > 0.8f)//ty le 80% la false
                {
                    return 1;
                }
                else if ((float)(count) / inputWords.Length > 0.4f && count >= 5)//ty le 50% la false
                {
                    return -1;
                }
                else if ((float)(count) / inputWords.Length > 0f && (count >= 2 && inputWords.Length >= 3))//theo
                {
                    return -1;//consider
                }
                else
                {
                    return 0;//ok
                }
            }
            return 0;
        }

        //TBD
        private bool isDiffBetweenTextAndDes(string text, string des)
        {
            return false;
        }
        private bool isDiffBetweenTBAndUI(string talkbackText, string uiText)
        {
            return false;
        }
        //TBD
        private string removeElement(string s)
        {
            s = s.ToLower();
            List<string> listElement = getListElement();
            if (listElement != null)
            {
                listElement = listElement.OrderBy(str => str.Length).ThenBy(str => str).Reverse().ToList();
                foreach (string word in listElement)
                {

                    if (word.Trim() != "" && s.ToLower().Contains(word.ToLower()))
                    {

                        string pattern = "\\b" + Regex.Escape(word) + "\\b";
                        string replacement = "";

                        s = Regex.Replace(s, pattern, replacement, RegexOptions.IgnoreCase);

                        //talkbackText = talkbackText.Replace(line, "");
                    }

                    //s = s.Replace(word.ToLower(), "");
                }
            }
            return s;

        }
        string globalSamePrevious = "";
        private int SamePrevious(string talkbackText, string previousTalkbackText,string currentScreen,string previousScreen)
        {
            //toi uu
            globalSamePrevious = "";


            if (currentScreen != previousScreen) return 0;
            if (talkbackText == TRYCATCH_NA || previousTalkbackText == TRYCATCH_NA) return 0;

            if (previousTalkbackText == "" || talkbackText == "") return 0;
            //if (talkbackText.ToLower().Contains(previousTalkbackText.ToLower()) == true || previousTalkbackText.ToLower().Contains(talkbackText.ToLower()) == true)

            //if (talkbackText.ToLower().Contains(previousTalkbackText.ToLower()) == true)
            //need remove 1 so cai


            if (previousTalkbackText == talkbackText)//giong nhau 100% ke ca cac element
            {
                globalSamePrevious = talkbackText;
                return 1;
            }

            talkbackText = removeElement(talkbackText);
            previousTalkbackText = removeElement(previousTalkbackText);


            //if (talkbackText == previousTalkbackText)
            //{
            //    globalSamePrevious = talkbackText;
            //    return 1;//same
            //}

            string[] words1 = talkbackText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();//remove nhung tu trong baclklist
            string[] words2 = previousTalkbackText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();//remove nhung tu trong baclklist
            int length = words1.Length < words2.Length ? words1.Length : words2.Length;
            int lengthMax = words1.Length > words2.Length ? words1.Length : words2.Length;
            int countSame = 0;
            for (int i = 0; i < length; i++)
            {
                if (words1[i].Trim() == words2[i].Trim())
                {
                    globalSamePrevious += words1[i] + ", ";
                    countSame++;
                }
            }

            globalSamePrevious = globalSamePrevious.TrimEnd(',', ' ');

            float rate = (float)countSame / lengthMax;
            if (rate > 0.9f)
            {
                return -1;//consider
            }
            else if (rate >= 0.5f && lengthMax >= 3)
            {
                return -1;//consider
            }
            else if (rate >= 0.6f)
            {
                return -1;//consider
            }

            return 0;//not same -1 la consider 
        }

        private bool inBlackListUnlabelled(string talkbackText)
        {
            if (TalkBackAutoTest.Properties.Settings.Default.blacklist_unlabelled.ToString().Trim() == "")
            {
                return false;
            }

            string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_unlabelled.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (lines.Length > 0)
            {
                foreach (string line in lines)
                {
                    if (line.Trim() != "" && talkbackText.ToLower().Contains(line.ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private bool inBlackListDupWord(string talkbackText)
        {
            if (TalkBackAutoTest.Properties.Settings.Default.blacklist_dup_word.ToString().Trim() == "")
            {
                return false;
            }

            string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_dup_word.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (lines.Length > 0)
            {
                foreach (string line in lines)
                {
                    if (line.Trim() != "" && talkbackText.ToLower().Contains(line.ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //private bool inBlackListRepeat(string talkbackText)
        //{

        //    if (TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text.ToString().Trim() == "")
        //    {
        //        return false;
        //    }

        //    string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text.Split(
        //        new string[] { "\r\n", "\r", "\n" },
        //        StringSplitOptions.None
        //    );
        //    if (lines.Length > 0)
        //    {
        //        foreach (string line in lines)
        //        {
        //            if (line.Trim() != "" && talkbackText.ToLower() == line.ToLower())
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
        private List<string> getListElement()
        {

            List<string> l = new List<string>();
            if (File.Exists(@BLACKLIST_ELEMENT_PATH))
            {
                string allTextBlacklist = File.ReadAllText(@BLACKLIST_ELEMENT_PATH);
                l = allTextBlacklist.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.RemoveEmptyEntries
               ).ToList();

                l = l.OrderBy(str => str.Length).ThenBy(str => str).Reverse().ToList();
                return l;
            }
            return null;
        }

        private string RemoveWholeBlackList(string talkbackText)
        {
            try
            {
                string content = talkbackText;

                if (File.Exists(@BLACKLIST_ENG_WORDS_PATH))
                {
                    string allTextBlacklist = File.ReadAllText(@BLACKLIST_ENG_WORDS_PATH);
                    string[] lines = allTextBlacklist.Split(
                        new string[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries
                   );

                    string[] lines1 = TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word.Split(
                        new string[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    if (lines1.Length == 0)
                    {
                        //lines = lines;
                    }
                    else if (lines.Length == 0)
                    {
                        lines = lines1;
                    }
                    else
                    {
                        lines = lines.Concat(lines1).ToArray();
                    }



                    if (lines.Length > 0)
                    {

                        lines = lines.OrderBy(str => str.Length).ThenBy(str => str).Reverse().ToArray();

                        foreach (string line in lines)
                        {
                            if (line.Trim() != "" && content.ToLower().Contains(line.ToLower()))
                            {
                                //string pattern = @$"\b{Regex.Escape(line)}\b";
                                string pattern = "\\b" + Regex.Escape(line) + "\\b";
                                string replacement = "";

                                content = Regex.Replace(content, pattern, replacement, RegexOptions.IgnoreCase);
                            }
                        }
                    }
                    return content;
                }

                return content;
            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
                return talkbackText;
            }
        }
        private string RemoveBlackListRepeat(string talkbackText)
        {



            if (TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text.ToString().Trim() == "")
            {
                return talkbackText;
            }

            string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (lines.Length > 0)
            {
                lines = lines.OrderBy(str => str.Length).ThenBy(str => str).Reverse().ToArray();
                foreach (string line in lines)
                {
                    if (line.Trim() != "" && talkbackText.ToLower().Contains(line.ToLower()))
                    {

                        string pattern = "\\b" + Regex.Escape(line) + "\\b";
                        string replacement = "";

                        talkbackText = Regex.Replace(talkbackText, pattern, replacement, RegexOptions.IgnoreCase);

                        //talkbackText = talkbackText.Replace(line, "");
                    }
                }
            }

            return talkbackText;
        }

        private string RemoveBlackListLanguage(string talkbackText)
        {
            //string s=  talkbackText;
            talkbackText = RemoveWholeBlackList(talkbackText);

            //if (TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word.ToString().Trim() == "")
            //{
            //    return talkbackText;
            //}

            //string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word.Split(
            //    new string[] { "\r\n", "\r", "\n" },
            //    StringSplitOptions.RemoveEmptyEntries
            //);
            //if (lines.Length > 0)
            //{
            //    lines = lines.OrderBy(str => str.Length).ThenBy(str => str).Reverse().ToArray();

            //    foreach (string line in lines)
            //    {
            //        if (line.Trim() != "" && talkbackText.ToLower().Contains(line.ToLower()))
            //        {
            //            string pattern = "\\b" + Regex.Escape(line) + "\\b";
            //            string replacement = "";

            //            talkbackText = Regex.Replace(talkbackText, pattern, replacement, RegexOptions.IgnoreCase);

            //            //talkbackText = talkbackText.Replace(line, "");
            //        }
            //    }
            //}

            return talkbackText;
        }

        //private string removeBlackListLanguage(string talkbackText)
        //{
        //    string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word.Split(
        //        new string[] { "\r\n", "\r", "\n" },
        //        StringSplitOptions.None
        //    );
        //    if (lines.Length > 0)
        //    {
        //        foreach (string line in lines)
        //        {
        //            if (line.Trim() != "")
        //            {
        //                if (talkbackText.Contains(line))
        //                {
        //                    talkbackText = talkbackText.Replace(line, "");
        //                }
        //            }
        //        }
        //    }

        //    return talkbackText;
        //}

        private bool ischildrenOf(string tgParent, XElement script)
        {
            try
            {
                XElement parent = script.Parent;
                while (parent != null)
                {
                    string resId = parent.Attribute("resource-id").Value.ToString();
                    if (resId == tgParent)
                    {
                        return true;
                    }
                    parent = parent.Parent;
                }
                return false;
            }
            catch (Exception ex)
            {
                printLog("ischildrenOf:" + ex.Message);
                return false;
            }
        }

        private bool isIgnore(string res, string package, XElement script)
        {
            if (package == "com.sec.android.app.myfiles" && res == "com.sec.android.app.myfiles:id/contents_container")
            {
                if (COUNT_IGNORE_MYFILES == 0)//chua co phat nao
                {
                    COUNT_IGNORE_MYFILES++;
                    return false;
                }
                else
                {
                    COUNT_IGNORE_MYFILES++;
                    return true;
                }
            }

            //toi uu hon cho nay pkg va to tien la kia, neu myfile <=1 thi return false. lp thi true
            if (package == "com.sec.android.app.myfiles" && ischildrenOf("com.sec.android.app.myfiles:id/contents_container", script) == true)
            {
                if (COUNT_IGNORE_MYFILES <= 1)//chua co phat nao
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //end


            if (package == "com.samsung.android.app.notes" && res == "com.samsung.android.app.notes:id/root_cardview")
            {
                if (COUNT_IGNORE_NOTES == 0)//chua co phat nao
                {
                    COUNT_IGNORE_NOTES++;
                    return false;
                }
                else
                {
                    COUNT_IGNORE_NOTES++;
                    return true;
                }
            }

            //toi uu hon cho nay pkg va to tien la kia, neu myfile <=1 thi return false. lp thi true
            if (package == "com.samsung.android.app.notes" && ischildrenOf("com.samsung.android.app.notes:id/root_cardview", script) == true)
            {
                if (COUNT_IGNORE_NOTES <= 1)//chua co phat nao
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //end

            //if (package == "com.sec.android.gallery3d" && res == "com.sec.android.gallery3d:id/thumbnail_preview_layout")
            if (package == "com.sec.android.gallery3d" && res == "com.sec.android.gallery3d:id/recycler_view_item")
            {
                if (COUNT_IGNORE_GALLERY == 0)//chua co phat nao
                {
                    COUNT_IGNORE_GALLERY++;
                    return false;
                }
                else
                {
                    COUNT_IGNORE_GALLERY++;
                    return true;
                }
            }
            //toi uu hon cho nay pkg va to tien la kia, neu myfile <=1 thi return false. lp thi true
            //if (package == "com.sec.android.gallery3d" && ischildrenOf("com.sec.android.gallery3d:id/thumbnail_preview_layout", script) == true)
            if (package == "com.sec.android.gallery3d" && ischildrenOf("com.sec.android.gallery3d:id/recycler_view_item", script) == true)
            {
                if (COUNT_IGNORE_GALLERY <= 1)//chua co phat nao
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //end

            if (package == "com.sec.android.app.voicenote" && res == "com.sec.android.app.voicenote:id/listRow_layout")
            {
                if (COUNT_IGNORE_VOICE == 0)//chua co phat nao
                {
                    COUNT_IGNORE_VOICE++;
                    return false;
                }
                else
                {
                    COUNT_IGNORE_VOICE++;
                    return true;
                }
            }
            //toi uu hon cho nay pkg va to tien la kia, neu myfile <=1 thi return false. lp thi true
            if (package == "com.sec.android.app.voicenote" && ischildrenOf("com.sec.android.app.voicenote:id/listRow_layout", script) == true)
            {
                if (COUNT_IGNORE_VOICE <= 1)//chua co phat nao
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //end

            if (package == "com.sec.android.app.clockpackage" && res == "com.sec.android.app.clockpackage:id/alarm_list_cardView")
            {
                if (COUNT_IGNORE_CLOCK == 0)//chua co phat nao
                {
                    COUNT_IGNORE_CLOCK++;
                    return false;
                }
                else
                {
                    COUNT_IGNORE_CLOCK++;

                    //kiem tra co bao nhieu thang o duoi thi send TAB
                    return true;
                }
            }
            //toi uu hon cho nay pkg va to tien la kia, neu myfile <=1 thi return false. lp thi true
            if (package == "com.sec.android.app.clockpackage" && ischildrenOf("com.sec.android.app.clockpackage:id/alarm_list_cardView", script) == true)
            {
                if (COUNT_IGNORE_CLOCK <= 1)//chua co phat nao
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            //end

            return false;
        }

        private Object getFocusedObject(string folderResult, string fileObject, string currentLang, string talkbackText, string testingmode, string objId, string xmlPath)
        {
            try
            {

                int objIdx = Int16.Parse(fileObject);


                Object obj = new Object();

                Object obj1 = new Object();
                obj1.no = -1;
                Object obj2 = new Object();
                obj2.no = -2;

                XDocument doc = XDocument.Load(@xmlPath);
                var Scripts = doc.Descendants("node");

                List<XElement> ChildNodes = new List<XElement>();
                bool hasFocused = false;

                int countClockId = 0;

                foreach (XElement script in Scripts)
                {
                    ScriptsAll.Add(script);
                    string focused = script.Attribute("focused").Value.ToString();
                    string package = script.Attribute("package").Value.ToString();
                    string res = script.Attribute("resource-id").Value.ToString();
                    if (package == "com.sec.android.app.clockpackage" && res == "com.sec.android.app.clockpackage:id/alarm_list_cardView")
                    {
                        if (focused == "true")
                        {
                            countClockId = 1;
                        }
                        else if (countClockId > 0)
                        {
                            countClockId++;
                        }
                    }

                    if (focused == "true")
                    {
                        hasFocused = true;
                        //int c = 6;

                        //XDocument doc1 = XDocument.Parse(script.ToString());

                        string className = script.Attribute("class").Value.ToString();
                        //string res = script.Attribute("resource-id").Value.ToString();
                        string text = script.Attribute("text").Value.ToString();
                        string des = script.Attribute("content-desc").Value.ToString();
                        string bounds = script.Attribute("bounds").Value.ToString();
                        //string package = script.Attribute("package").Value.ToString();
                        string NAF = script.Attribute("NAF") == null ? "false" : script.Attribute("NAF").Value.ToString().ToLower();
                        string currentScreen = getCurrentScreen();
                        if (isExistedObjectInfo(script.ToString(), currentScreen))
                        {
                            printLog("Return Obj: " + script.ToString() + " existed ");
                            obj1.objectInformation = EscapeXml(script.ToString());
                            obj1.screen = currentScreen;
                            return obj1;
                        }



                        if (isIgnore(res, package, script))
                        {
                            printLog("Return Obj Ignore:" + package + ": " + script.ToString() + " ignored ");
                            obj1.objectInformation = EscapeXml(script.ToString());
                            obj1.screen = currentScreen;
                            return obj2;
                        }

                        string allText = "";
                        string allDes = "";

                        string allTextDes_optimize = "";
                        try
                        {
                            XDocument doc1 = XDocument.Parse(script.ToString());
                            var childs = doc1.Descendants("node");

                            foreach (XElement x in childs)
                            {
                                ChildNodes.Add(x);
                                if (x.Attribute("text").Value.ToString() != "")
                                {
                                    allText += x.Attribute("text").Value.ToString() + ", ";
                                }
                                if (x.Attribute("content-desc").Value.ToString() != "")
                                {
                                    allDes += x.Attribute("content-desc").Value.ToString() + ", ";

                                    allTextDes_optimize += x.Attribute("content-desc").Value.ToString() + ", ";
                                }
                                else
                                {
                                    if (x.Attribute("text").Value.ToString() != "")
                                    {
                                        allTextDes_optimize += x.Attribute("text").Value.ToString() + ", ";
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            //info child
                        }

                        allText = allText.TrimEnd(',', ' ');
                        allDes = allDes.TrimEnd(',', ' ');
                        allTextDes_optimize = allTextDes_optimize.TrimEnd(',', ' ');


                        //string objectInformation = "[Class]_" + className + "[ID]_" + res + "[TEXT]_" + allText + "[DESC]_" + allDes + "[Bounds]_" + bounds.ToString();

                        string objectInformation = script.ToString();

                        string packageVersion = getPackageVersion(package);

                        string result = "Pass";
                        string ErrorType = "Re-Check";

                        string remark = allTextDes_optimize;

                        //compare talkback text va des

                        //toi uu here
                        string[] array_bounds = bounds.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                        //talkback
                        //croptFocusImage(folderResult, "obj" + fileObject + ".png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));

                        string previousTalkbackText = "";
                        //int previousId = Int16.Parse(fileObject) - 1 - 1;
                        int previousId = listObject.Count - 1;
                        if (previousId >= 0)
                        {
                            previousTalkbackText = listObject[previousId].talkbackText;
                        }

                        //en vi ko -> vi: ko: 
                        //if (!inBlackListUnlabelled(talkbackText) && talkbackText.ToLower().Contains("unlabelled") || talkbackText.ToLower().Contains("Chưa được gắn nhãn") || talkbackText.Contains("라벨이 지정되지 않음"))
                        //talkbackText.ToLower().Contains("Chưa được gắn nhãn") || talkbackText.Contains("라벨이 지정되지 않음")

                        if (talkbackText == TRYCATCH_NA)
                        {
                            obj = new Object(objIdx, objId, currentScreen, package, packageVersion, objectInformation, talkbackText, "NT", System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                            drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                            return obj;
                        }




                        if (cb_Unlabelled.Checked == true && !inBlackListUnlabelled(talkbackText))
                        {
                            if (talkbackText == "Unlabelled" || talkbackText == "Chưa được gắn nhãn" || talkbackText == "라벨이 지정되지 않음")
                            {
                                result = "Fail";
                                result = GLOBAL_FAIL;
                                ErrorType = "UNLABELLED";//fasle alarm  bo sung rule + blacklist
                            }
                            else if (talkbackText.StartsWith("Unlabelled,") || talkbackText.StartsWith("Chưa được gắn nhãn,") || talkbackText.StartsWith("라벨이 지정되지 않음,"))
                            {
                                result = GLOBAL_FAIL;
                                ErrorType = "UNLABELLED";//fasle alarm  bo sung rule + blacklist
                            }

                            else if (talkbackText.EndsWith(", Unlabelled") || talkbackText.EndsWith(", Chưa được gắn nhãn") || talkbackText.EndsWith(", 라벨이 지정되지 않음"))
                            {
                                result = GLOBAL_FAIL;
                                ErrorType = "UNLABELLED";//fasle alarm  bo sung rule + blacklist
                            }
                            else if (talkbackText.Contains(", Unlabelled,") || talkbackText.Contains(", Chưa được gắn nhãn,") || talkbackText.Contains(", 라벨이 지정되지 않음,"))
                            {
                                result = GLOBAL_FAIL;
                                ErrorType = "UNLABELLED";//fasle alarm  bo sung rule + blacklist
                            }
                            else if (talkbackText.Contains("Unlabelled") || talkbackText.Contains("Chưa được gắn nhãn") || talkbackText.Contains("라벨이 지정되지 않음"))
                            {
                                result = GLOBAL_CONSIDER;
                                ErrorType = "UNLABELLED";//fasle alarm  bo sung rule + blacklist
                            }
                        }


                        //else if (NAF == "true")//rule?
                        //else if(NAF =="true" && allDes == "" && allText == "")//NAF va check them childs cha them UI text ko co gi ????
                        if (cb_NAF.Checked == true && result != "Fail" && NAF == "true" && allDes == "" && allText == "" && talkbackText.Trim() == "")//NAF va check them childs cha them UI text ko co gi ????
                        {
                            result = GLOBAL_FAIL;
                            //ErrorType = "NAF";
                            ErrorType = "NO_OUTPUT";

                            //drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                        }
                        if (cb_NoOutput.Checked == true && result != "Fail" && allDes == "" && allText == "" && talkbackText.Trim() == "")//da duoc focus
                        {
                            result = GLOBAL_FAIL;
                            ErrorType = "NO_OUTPUT";//NAF_FOCUS
                            //drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));                 
                        }
                        if (cb_NoOutput.Checked == true && result != "Fail" && talkbackText.Trim() == "" && File.Exists(@folderResult + "/no_toast.txt"))//da duoc focus
                        {
                            result = GLOBAL_CONSIDER;
                            ErrorType = "NO_OUTPUT";//NAF_FOCUS
                            //drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                        }

                        //CONSIDER REMOVE IT
                        //else if (talkbackText.Trim() == "")//phu thuoc quality talbacktext tam thoi dùng video recheck 
                        //{
                        //    result = "Fail";
                        //    ErrorType = "NO_OUTPUT";//false alarm -> time de toi uu hon
                        //}
                        //GLOBAL_FAIL

                        //else if (!inBlackListDupWord(talkbackText) &&(isRepeatWordsStartEnd(talkbackText) || isRepeatWords(talkbackText)))

                        string outputDupWord = "";
                        if (cb_DupWord.Checked == true && result != "Fail" && !inBlackListDupWord(talkbackText))
                        {
                            outputDupWord = isDupWordNew(talkbackText);
                            if (outputDupWord != "")
                            {
                                result = GLOBAL_FAIL;
                                ErrorType = "DUP_WORD";//rule+blacklist
                            }
                        }

                        if (previousId >= 0 && cb_RepeatPreviousObject.Checked == true && result != "Fail" && RemoveBlackListRepeat(talkbackText) != "")
                        {
                            int rs = SamePrevious(talkbackText, previousTalkbackText,currentScreen, listObject[previousId].screen);
                            if (rs == 1)
                            {
                                //result = "Fail";//Consider -> remark how to check again?
                                result = GLOBAL_CONSIDER;//Consider -> remark how to check again?
                                ErrorType = "REPEAT_PREVIOUS_OBJECT";
                                //update talkback "(Eng words:" + globalSamePrevious + ")"
                            }
                            else if (rs == -1)//consider . 0 la false
                            {
                                result = GLOBAL_CONSIDER;//Consider -> remark how to check again?
                                ErrorType = "REPEAT_PREVIOUS_OBJECT";

                                //update talkacback globalSamePrevious
                            }
                        }

                        string tbAfterRemove = RemoveBlackListLanguage(talkbackText);

                        if (cbDifflang.Checked == true && result != "Fail" && tbAfterRemove != "")
                        {
                            int isChange = ischangelang(tbAfterRemove, currentLang);
                            if (isChange == 1)
                            {
                                result = GLOBAL_FAIL;
                                ErrorType = "DIFF_LANG";
                            }
                            else if (isChange == -1)
                            {
                                result = GLOBAL_CONSIDER;
                                ErrorType = "DIFF_LANG";
                            }
                        }

                        //if (result!="Fail" && isDiffBetweenTextAndDes(allText, allDes))//TBD
                        //{
                        //    result = "Fail";
                        //    ErrorType = "DIFF_TEXT_DES";
                        //}
                        //if (result!="Fail" && isDiffBetweenTBAndUI(talkbackText, uiFocusText)) //TBD
                        //{
                        //    result = "Fail";
                        //    ErrorType = "DIFF_TB_UI";
                        //}


                        //else if (currentLang != getLangId(remark))
                        //{
                        //    result = "Fail";
                        //    ErrorType = "DIFF_LANG";
                        //}

                        //need check 2804
                        //if (cb_NoOutput.Checked == true && result != "Fail" && (talkbackText.Contains(",,") || talkbackText.StartsWith(", ") || remark.StartsWith(", ") || remark.Contains(",,")))//consider thieu ouput
                        //if (cb_NoOutput.Checked == true && result != "Fail" && (HAVECOMMA2 == true || remark.StartsWith(", ") || remark.Contains(",,")))//consider thieu ouput
                        //if (cb_NoOutput.Checked == true && result != "Fail" && (HAVECOMMA2 == true || remark.StartsWith(", ") || remark.Contains(",,")))//consider thieu ouput
                        //{
                        //    result = "Consider";
                        //    ErrorType = "NO_OUTPUT";//NAF_FOCUS
                        //    //drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));                 
                        //}

                        string uiFocusText = "";
                        if (result == GLOBAL_PASS)
                        {
                            uiFocusText = getTextOCR(currentLang, @folderResult + "//" + "cropfocus_obj" + fileObject + ".png", @folderResult + "/no_toast1.txt");

                            if (talkbackText.Trim().ToLower() == remark.Trim().ToLower())
                            {
                                result = GLOBAL_PASS;
                                ErrorType = "NA";
                            }
                            else if (talkbackText.Trim().ToLower().Contains(remark.Trim().ToLower()))
                            {
                                result = GLOBAL_PASS;
                                ErrorType = "NA";
                            }
                            //ConvertToNoTone
                            else if (ConvertToNoTone(talkbackText.Trim().ToLower()).Contains(ConvertToNoTone(remark.Trim().ToLower())))
                            {
                                result = GLOBAL_PASS;
                                ErrorType = "NA_NoTone";
                            }

                            if (talkbackText.Trim().ToLower() == "")
                            {
                                if (rdByLog.Checked == true)
                                {
                                    result = GLOBAL_CONSIDER;
                                    ErrorType = "NO_OUTPUT";
                                }
                                else
                                {
                                    result = GLOBAL_PASS;
                                    ErrorType = "Re-Check_NoOutput";
                                }

                            }

                        }

                        if (uiFocusText != "")
                        {
                            allTextDes_optimize = allTextDes_optimize + " | UI_TEXT:" + uiFocusText;
                        }

                        if (ErrorType == "DIFF_LANG" && global_en_word != "")
                        {
                            obj = new Object(objIdx, objId, currentScreen, package, packageVersion, objectInformation, talkbackText + " (Eng words:" + global_en_word + ")", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                            drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                            return obj;
                        }

                        if (ErrorType == "REPEAT_PREVIOUS_OBJECT" && globalSamePrevious != "")//"(Eng words:" + globalSamePrevious + ")"
                        {
                            obj = new Object(objIdx, objId, currentScreen, package, packageVersion, objectInformation, talkbackText + " (Repeat words:" + globalSamePrevious + ")", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                            drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                            return obj;
                        }
                        if (ErrorType == "DUP_WORD" && outputDupWord != "")//"(Eng words:" + globalSamePrevious + ")"
                        {
                            obj = new Object(objIdx, objId, currentScreen, package, packageVersion, objectInformation, talkbackText + " (Dup words:" + outputDupWord + ")", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                            drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                            return obj;
                        }

                        obj = new Object(objIdx, objId, currentScreen, package, packageVersion, objectInformation, talkbackText, result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                        drawOnImage(folderResult + "/obj" + fileObject + ".png", folderResult + "/obj" + fileObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                        return obj;
                    }

                }

                if (cb_NoOutputAfterTab.Checked == false && hasFocused == false)
                {
                    printLog("Ignore case check NoOutPutAfterTab", "error");
                    return null;
                }

                if (hasFocused == false)//thuc te dang khong duoc focus
                {
                    if (listObject.Count > 0)
                    {
                        Object objLatest = listObject[listObject.Count - 1];//get latest
                        if (objLatest != null)
                        {
                            //TBD, cop screenshot tu thang tc truoc do nhu another
                            //TBD can check them tb co toast ko????

                            int latestIndex = objIdx - 1;
                            obj = new Object(objIdx, objId, objLatest.screen, objLatest.package, objLatest.pkgVersion, UnescapeXml(objLatest.objectInformation), "", "Consider", System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "", testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), "NO_CHANGE_FOCUS_AFTER_TAB");
                            //TBD Copy tu latest qua hien tai

                            string folderIndex = @txtWS.Text + "//Result//" + "obj" + (latestIndex);

                            if (File.Exists(@folderIndex + "/obj" + latestIndex + "_canvas.png"))
                            {
                                File.Copy(@folderIndex + "/obj" + latestIndex + "_canvas.png", folderResult + "/obj" + objIdx + "_canvas.png", true);//image
                            }


                            //enddsd
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                return obj;
                //int a = 5;
            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
                return null;
            }
        }

        //private void btnChooseWS_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        CommonOpenFileDialog fbd = new CommonOpenFileDialog();
        //        fbd.IsFolderPicker = true;
        //        if (fbd.ShowDialog() == CommonFileDialogResult.Ok)
        //        {

        //            string driveName = Path.GetPathRoot(fbd.FileName);
        //            DriveInfo drive = new DriveInfo(driveName);

        //            if (fbd.FileName.Contains(" "))
        //            {
        //                MessageBox.Show("Have Whitespace Character in Path. Please choose again without Whitespace Character in Path");
        //            }
        //            else if (Path.GetPathRoot(fbd.FileName) == fbd.FileName)
        //            {
        //                MessageBox.Show("Please choose Workspace with sub folder, not only letter such as C:\\, D:\\ ...!");
        //            }
        //            else
        //            {
        //                //txt_choose_save_folder.Text = fbd.FileName.TrimEnd('\\') + "\\";
        //                txtWS.Text = fbd.FileName;

        //                //setas_path = "\"" + setas_path + "\"";
        //                TalkBackAutoTest.Properties.Settings.Default.txtWS = txtWS.Text;
        //                TalkBackAutoTest.Properties.Settings.Default.Save();
        //                initProject();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Try Catch: " + ex.Message);
        //    }
        //}

        private void btnChooseWS_Click(object sender, EventArgs e)
        {
            BetterFolderBrowser f = new BetterFolderBrowser();
            f.Title = "Pick Output Folder";
            f.Multiselect = false;
            try
            {
                f.RootFolder = @txtWS.Text;
            }
            catch { }

            if (f.ShowDialog() == DialogResult.OK)
            {
                if (f.SelectedFolder.Contains(" "))
                {
                    MessageBox.Show("Have Whitespace Character in Path. Please choose again without Whitespace Character in Path");
                }
                else
                {
                    string driveName = Path.GetPathRoot(f.SelectedFolder);
                    DriveInfo drive = new DriveInfo(driveName);

                    if (f.SelectedFolder.Contains(" "))
                    {
                        MessageBox.Show("Have Whitespace Character in Path. Please choose again without Whitespace Character in Path");
                    }
                    else if (Path.GetPathRoot(f.SelectedFolder) == f.SelectedFolder)
                    {
                        MessageBox.Show("Please choose Workspace with sub folder, not only letter such as C:\\, D:\\ ...!");
                    }
                    else
                    {
                        //txt_choose_save_folder.Text = fbd.FileName.TrimEnd('\\') + "\\";
                        txtWS.Text = f.SelectedFolder;

                        //setas_path = "\"" + setas_path + "\"";
                        TalkBackAutoTest.Properties.Settings.Default.txtWS = txtWS.Text;
                        TalkBackAutoTest.Properties.Settings.Default.Save();
                        initProject();
                    }
                }
            }

            //try
            //{
            //    FolderBrowserDialog fbd = new FolderBrowserDialog();
            //    fbd.Reset();
            //    fbd.ShowNewFolderButton = false;
            //    if (txtWS.Text != "")
            //    {
            //        fbd.SelectedPath = @txtWS.Text +"\\";
            //    }


            //    //fbd.IsFolderPicker = true;


            //    //fbd.RootFolder = Environment.SpecialFolder.MyComputer;

            //    if (fbd.ShowDialog() == DialogResult.OK)
            //    {

            //        string driveName = Path.GetPathRoot(fbd.SelectedPath);
            //        DriveInfo drive = new DriveInfo(driveName);

            //        if (fbd.SelectedPath.Contains(" "))
            //        {
            //            MessageBox.Show("Have Whitespace Character in Path. Please choose again without Whitespace Character in Path");
            //        }
            //        else if (Path.GetPathRoot(fbd.SelectedPath) == fbd.SelectedPath)
            //        {
            //            MessageBox.Show("Please choose Workspace with sub folder, not only letter such as C:\\, D:\\ ...!");
            //        }
            //        else
            //        {
            //            //txt_choose_save_folder.Text = fbd.FileName.TrimEnd('\\') + "\\";
            //            txtWS.Text = fbd.SelectedPath;

            //            //setas_path = "\"" + setas_path + "\"";
            //            TalkBackAutoTest.Properties.Settings.Default.txtWS = txtWS.Text;
            //            TalkBackAutoTest.Properties.Settings.Default.Save();
            //            initProject();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Try Catch: " + ex.Message);
            //}
        }

        private void txtPkg_TextChanged(object sender, EventArgs e)
        {
            //try
            //{
            //    TalkBackAutoTest.Properties.Settings.Default.txtPkg = stressTestPkg = txtPkg.Text;
            //    TalkBackAutoTest.Properties.Settings.Default.Save();
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show("Try Catch: " + ex.Message);
            //    printLog("Try Catch: " + ex.Message, "eror");
            //}
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            //getAnotherObjectsFromStartEnd(0, 80);
            // int a = 5;

            //SamePrevious("Default calendar color, Select color","Default calendar color, Select color");
            //RemoveWholeBlackList("Create note, Button");
            //SamePrevious("M.hình khóa, đã chọn, Hộp kiểm", "Lock scre1e1n, not checked, Check box");
            //ischangelang("Text Summary","vi");
            //removeElement("Đã chọn, 1232");
            //string folderpath = @"D:\F_Folder\2025\TalkBack\ToastImg";
            //string sourceFile1 = @"1_obj1.png";
            //string sourceFile2 = @"1_obj3.png";
            //string sourceFile3 = @"2_obj1.png";
            //string sourceFile4 = @"3_obj1.png";
            //string sourceFile5 = @"5_obj1.png";
            //string sourceFile6 = @"old1_obj1.png";
            //string sourceFile7 = @"1_obj24.png";
            //int it = 1;
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile1, @folderpath + "/croptb_" + sourceFile1, it);
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile2, @folderpath + "/croptb_" + sourceFile2, it);
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile3, @folderpath + "/croptb_" + sourceFile3, it);
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile4, @folderpath + "/croptb_" + sourceFile4, it);
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile5, @folderpath + "/croptb_" + sourceFile5, it);
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile6, @folderpath + "/croptb_" + sourceFile6, it);
            //cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile7, @folderpath + "/croptb_" + sourceFile7, it);
            //int a = 5;
            //getAnotherObjectsFromStartEnd(0, 12);
            ExportExcel(txtWS.Text + "//result.xml");
        }


        private bool isExistedObject(Object o)
        {
            try
            {
                string info1 = UnescapeXml(o.objectInformation);
                XElement script1 = XElement.Parse(info1);
                XDocument doc1 = XDocument.Parse(script1.ToString());
                var childs1 = doc1.Descendants("node");
                string resId1 = "", clazz1 = "", text1 = "", des1 = "";
                foreach (XElement x in childs1)
                {
                    resId1 += x.Attribute("resource-id").Value.ToString() + " ";
                    clazz1 += x.Attribute("class").Value.ToString() + " ";
                    text1 += x.Attribute("text").Value.ToString() + " ";
                    des1 += x.Attribute("content-desc").Value.ToString() + " ";
                    break;
                }

                foreach (Object obj in listObject)
                {
                    //if (obj.talkbackText == o.talkbackText && obj.screen == o.screen)//ton tai
                    if (obj.screen == o.screen)//ton tai
                    {
                        string info2 = UnescapeXml(obj.objectInformation);
                        XElement script2 = XElement.Parse(info2);
                        XDocument doc2 = XDocument.Parse(script2.ToString());
                        var childs2 = doc2.Descendants("node");
                        string resId2 = "", clazz2 = "", text2 = "", des2 = "";
                        foreach (XElement x in childs2)
                        {
                            resId2 += x.Attribute("resource-id").Value.ToString() + " ";
                            clazz2 += x.Attribute("class").Value.ToString() + " ";
                            text2 += x.Attribute("text").Value.ToString() + " ";
                            des2 += x.Attribute("content-desc").Value.ToString() + " ";
                            break;
                        }


                        if (resId1 == resId2 && clazz1 == clazz2 && text1 == text2 && des1 == des2)
                        {
                            TIMES_DUP++;
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool isExistedObjectInfo(string info, string screen)
        {
            try
            {
                //string info1 = UnescapeXml(info);
                string info1 = info;
                XElement script1 = XElement.Parse(info1);
                XDocument doc1 = XDocument.Parse(script1.ToString());
                var childs1 = doc1.Descendants("node");
                string resId1 = "", clazz1 = "", text1 = "", des1 = "";
                foreach (XElement x in childs1)
                {
                    resId1 += x.Attribute("resource-id").Value.ToString() + " ";
                    clazz1 += x.Attribute("class").Value.ToString() + " ";
                    text1 += x.Attribute("text").Value.ToString() + " ";
                    des1 += x.Attribute("content-desc").Value.ToString() + " ";
                    //break;
                }

                string key = resId1 + "_" + clazz1 + "_" + text1 + "_" + des1;
                if (hashMap.ContainsKey(key))
                {
                    return true;
                }

                //foreach (Object obj in listObject)
                //{
                //    //if (obj.talkbackText == o.talkbackText && obj.screen == o.screen)//ton tai
                //    if (obj.screen == screen)//ton tai
                //    {
                //        string info2 = UnescapeXml(obj.objectInformation);
                //        XElement script2 = XElement.Parse(info2);
                //        XDocument doc2 = XDocument.Parse(script2.ToString());
                //        var childs2 = doc2.Descendants("node");
                //        string resId2 = "", clazz2 = "", text2 = "", des2 = "";
                //        foreach (XElement x in childs2)
                //        {
                //            //need check again
                //            //if (!resId1.Contains(resId2) || !clazz1.Contains(clazz2) || !text1.Contains(text2) || !des1.Contains(des2))
                //            //{
                //            //    return false;
                //            //}

                //            resId2 += x.Attribute("resource-id").Value.ToString() + " ";
                //            clazz2 += x.Attribute("class").Value.ToString() + " ";
                //            text2 += x.Attribute("text").Value.ToString() + " ";
                //            des2 += x.Attribute("content-desc").Value.ToString() + " ";
                //            //break;
                //        }


                //        if (resId1 == resId2 && clazz1 == clazz2 && text1 == text2 && des1 == des2)
                //        {
                //            return true;
                //        }
                //    }
                //}
                return false;
            }
            catch
            {
                return false;
            }
        }
        //isExistedFirstObject
        private bool isExistedFirstObject(string FirstObjectInfo, string FirstObjectScreen, string objectInfo, string objectScreen)
        {
            try
            {
                string info1 = UnescapeXml(FirstObjectInfo);
                XElement script1 = XElement.Parse(info1);
                XDocument doc1 = XDocument.Parse(script1.ToString());
                var childs1 = doc1.Descendants("node");
                string resId1 = "", clazz1 = "", text1 = "", des1 = "";
                foreach (XElement x in childs1)
                {
                    resId1 += x.Attribute("resource-id").Value.ToString() + " ";
                    clazz1 += x.Attribute("class").Value.ToString() + " ";
                    text1 += x.Attribute("text").Value.ToString() + " ";
                    des1 += x.Attribute("content-desc").Value.ToString() + " ";
                    //break;
                }

                if (FirstObjectScreen == objectScreen)//ton tai
                {
                    string info2 = UnescapeXml(objectInfo);
                    XElement script2 = XElement.Parse(info2);
                    XDocument doc2 = XDocument.Parse(script2.ToString());
                    var childs2 = doc2.Descendants("node");
                    string resId2 = "", clazz2 = "", text2 = "", des2 = "";
                    foreach (XElement x in childs2)
                    {
                        resId2 += x.Attribute("resource-id").Value.ToString() + " ";
                        clazz2 += x.Attribute("class").Value.ToString() + " ";
                        text2 += x.Attribute("text").Value.ToString() + " ";
                        des2 += x.Attribute("content-desc").Value.ToString() + " ";
                        //break;
                    }


                    if (resId1 == resId2 && clazz1 == clazz2 && text1 == text2 && des1 == des2)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        //private bool isExistedObject(Object o)
        //{
        //    foreach (Object obj in listObject)
        //    {
        //        if (obj.objectInformation == o.objectInformation && obj.talkbackText == o.talkbackText && obj.screen == o.screen)//ton tai
        //        {
        //            TIMES_DUP++;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private bool isExistedObjectInfo(string info,string screen,string talkbackText)
        //{
        //    //foreach (Object obj in listObject)
        //    for (int i = 0; i < listObject.Count; i++)
        //    {
        //        Object obj = listObject[i];
        //        if (obj.objectInformation == info && obj.talkbackText == talkbackText && obj.screen == screen)//ton tai
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private void TAB_COMMAND()
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell input keyevent TAB", 0);
            }
        }


        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

        //private void croptoSquare(string imageFilePath,string imageFileName)
        //{
        //    int x = 10, y = 20, width = 200, height = 100;
        //    Bitmap source = new Bitmap(@"D:\F_Folder\2025\AI_2025\demo2.jpg");
        //    Bitmap CroppedImage = source.Clone(new System.Drawing.Rectangle(x, y, width, height), source.PixelFormat);
        //    int a = 5;
        //    ////Location of 320x240 image
        //    //string fileName = imageFilePath + "//" + imageFileName;


        //    ////Load image from file
        //    //using (Image image = Image.FromFile(fileName))
        //    //{

        //    //    int width = image.Size.Width;
        //    //    int height = image.Size.Height;

        //    //    // Create a new image at the cropped size
        //    //    Bitmap cropped = new Bitmap(width, 100);

        //    //    // Create a Graphics object to do the drawing, *with the new bitmap as the target*
        //    //    using (Graphics g = Graphics.FromImage(cropped))
        //    //    {
        //    //        // Draw the desired area of the original into the graphics object
        //    //        g.DrawImage(image, new Rectangle(0, 0, width, 100), new Rectangle(0, 0, width, 100), GraphicsUnit.Pixel);
        //    //        string cropfileName = imageFilePath + "//crop_" + imageFileName;
        //    //        // Save the result
        //    //        cropped.Save(fileName);
        //    //    }
        //    //}
        //}

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

        public string UnescapeXml(string s)
        {
            string unxml = s;
            if (!string.IsNullOrEmpty(unxml))
            {
                // replace entities with literal values
                unxml = unxml.Replace("&apos;", "'");
                unxml = unxml.Replace("&quot;", "\"");
                unxml = unxml.Replace("&gt;", ">");
                unxml = unxml.Replace("&lt;", "<");
                unxml = unxml.Replace("&amp;", "&");
            }
            return unxml;
        }
        //int global_count = 0;
        //int global_count0 = 0;
        private void analyzeXML(int index)
        {
            //create folder result
            try
            {
                int IdxArray = index - 1;
                //index = index - 1;
                string folderIndex = @txtWS.Text + "//Result//" + "obj" + (index);
                string xmlPath = @folderIndex + "/" + "obj" + index + ".xml";
                XDocument doc = XDocument.Load(@xmlPath);
                var Scripts = doc.Descendants("node");

                foreach (XElement script in Scripts)
                {
                    int numberOfObject = getNumberOfObject();
                    string className1 = script.Attribute("class").Value.ToString();
                    string res1 = script.Attribute("resource-id").Value.ToString();
                    string text1 = script.Attribute("text").Value.ToString();
                    string des1 = script.Attribute("content-desc").Value.ToString();
                    string focusable1 = script.Attribute("focusable").Value.ToString();
                    string clickable1 = script.Attribute("clickable").Value.ToString();

                    string bounds1 = script.Attribute("bounds").Value.ToString();
                    string package1 = script.Attribute("package").Value.ToString();
                    string NAF1 = script.Attribute("NAF") == null ? "false" : script.Attribute("NAF").Value.ToString().ToLower();
                    //global_count0++;
                    //3005
                    if ((text1 == "" && des1 == "") || focusable1 == "true")
                    {
                        continue;
                    }
                    //global_count++;
                    //get tex1 des1 tu thang con



                    //string keyHashMap = resId2 + "_" + clazz2 + "_" + text2 + "_" + des2;
                    //end

                    // string objectInformation = "[OBJ]_[TEXT]" + text1 + "_[DES]" + des1 + "_[BOUND]" + bounds1;
                    //string objectInformation = "[Class]_" + className1 + "[ID]_" + res1 + "[TEXT]_" + text1 + "[DESC]_" + des1 + "[Bounds]_" + bounds1;

                    string curentScreenNow = listObject[IdxArray].screen;

                    //string objectInformation = "[Class]_" + className1 + "[ID]_" + res1 + "[TEXT]_" + text1 + "[DESC]_" + des1;
                    string objectInformation = script.ToString();
                    if (isExistedObjectInfo(objectInformation, curentScreenNow) == true)
                    {
                        continue;
                    }

                    string allTextDes_optimize = "";
                    try
                    {
                        XDocument doc1 = XDocument.Parse(script.ToString());
                        var childs = doc1.Descendants("node");

                        foreach (XElement x in childs)
                        {
                            if (x.Attribute("content-desc").Value.ToString() != "")
                            {
                                allTextDes_optimize += x.Attribute("content-desc").Value.ToString() + " ";
                            }
                            else
                            {
                                if (x.Attribute("text").Value.ToString() != "")
                                {
                                    allTextDes_optimize += x.Attribute("text").Value.ToString() + " ";
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //info child
                    }


                    //if (NAF1 == "true" && allTextDes_optimize == "")//co toan bo text con ko co thong tin -> consider
                    //{
                    //    //if (isExistedObjectInfo(objectInformation, curentScreenNow) == false)
                    //    if (isExistedObjectInfo(objectInformation, curentScreenNow) == false)
                    //    {
                    //        string result = "Consider";//consider
                    //        string ErrorType = "NAF";
                    //        string objId = "OBJ" + (numberOfObject + 1);
                    //        //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                    //        string currentScreen = listObject[IdxArray].screen;
                    //        string package = listObject[IdxArray].package;
                    //        string packageVersion = listObject[IdxArray].pkgVersion;
                    //        string testingmode = listObject[IdxArray].testingmode;
                    //        Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                    //        listObject.Add(obj);
                    //        syncHashMap(obj);
                    //        string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                    //        System.IO.Directory.CreateDirectory(@folderResult);
                    //        //System.IO.Directory.CreateDirectory(@folderResult+"//more_details");
                    //        //copy from to
                    //        //if (File.Exists(@folderIndex + "/obj" + index + "_canvas.png"))
                    //        //{
                    //        //    File.Copy(@folderIndex + "/obj" + index + "_canvas.png", folderResult + "/obj" + numberOfObject + "_canvas.png", true);//image
                    //        //}
                    //        if (File.Exists(@folderIndex + "/obj" + index + ".png"))
                    //        {
                    //            File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                    //        }
                    //        if (File.Exists(@folderIndex + "/more_details/obj" + index + ".png"))
                    //        {
                    //            File.Copy(@folderIndex + "/more_details/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                    //        }
                    //        if (File.Exists(@folderIndex + "/obj" + index + ".log"))
                    //        {
                    //            File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                    //        }
                    //        if (File.Exists(@folderIndex + "/obj" + index + ".xml"))
                    //        {
                    //            File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                    //        }

                    //        //File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                    //        //File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                    //        //File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image

                    //        string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                    //        drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                    //        //end

                    //        //updateXML();
                    //        //Thread.Sleep(1000);

                    //        //XU LY HERE
                    //        copyToMoreDetailFolder(folderResult, numberOfObject.ToString());
                    //        continue;
                    //    }
                    //}

                    if (cb_NoFocusableHeader.Checked == true)
                    {
                        if (text1 + des1 != "" && focusable1 == "false")
                        //if (allTextDes_optimize != "" && focusable1 == "false")
                        {
                            //if (isExistedObjectInfo(objectInformation, curentScreenNow) == false)
                            //{
                            //string result = "NC";
                            //string ErrorType = "NO_FOCUSABLE";
                            //if (className1.Contains("RelativeLayout"))
                            //{
                            //    ErrorType = "NO_FOCUSABLE_ROWLIST";
                            //}
                            if (text1 + des1 != "")
                            {
                                if (focusable1 == "false")
                                {
                                    XElement parent = script.Parent;
                                    if (parent != null)
                                    {

                                        string className = parent.Attribute("class").Value.ToString();

                                        string focusable2 = parent.Attribute("focusable").Value.ToString();
                                        if (focusable2 == "false")
                                        {

                                            XElement parent3 = parent.Parent;
                                            if (parent3 != null)
                                            {
                                                string focusable3 = parent3.Attribute("focusable").Value.ToString();
                                                if (focusable3 == "false")
                                                {
                                                    string ErrorType = "NO_FOCUSABLE_HEADER";
                                                    string result = "Consider";



                                                    string objId = "OBJ" + (numberOfObject + 1);
                                                    //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                                                    string currentScreen = listObject[IdxArray].screen;
                                                    string package = listObject[IdxArray].package;
                                                    string packageVersion = listObject[IdxArray].pkgVersion;
                                                    string testingmode = listObject[IdxArray].testingmode;
                                                    Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                                                    listObject.Add(obj);
                                                    syncHashMap(obj);
                                                    string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                                                    System.IO.Directory.CreateDirectory(@folderResult);
                                                    //System.IO.Directory.CreateDirectory(@folderResult+"//more_details");

                                                    //copy from to
                                                    if (File.Exists(@folderIndex + "/obj" + index + ".png"))
                                                    {
                                                        File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                                    }
                                                    if (File.Exists(@folderIndex + "/more_details/obj" + index + ".png"))
                                                    {
                                                        File.Copy(@folderIndex + "/more_details/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                                    }
                                                    if (File.Exists(@folderIndex + "/obj" + index + ".log"))
                                                    {
                                                        File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                                                    }
                                                    if (File.Exists(@folderIndex + "/obj" + index + ".xml"))
                                                    {
                                                        File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                                                    }

                                                    //File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                                    //File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                                                    //File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image

                                                    string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                                                    drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                                                    //end

                                                    //updateXML();
                                                    //Thread.Sleep(1000);

                                                    //XU LY HERE
                                                    copyToMoreDetailFolder(folderResult, numberOfObject.ToString());
                                                    continue;
                                                }

                                            }
                                            else
                                            {
                                                string ErrorType = "NO_FOCUSABLE_HEADER";
                                                string result = GLOBAL_CONSIDER;

                                                string objId = "OBJ" + (numberOfObject + 1);
                                                //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                                                string currentScreen = listObject[IdxArray].screen;
                                                string package = listObject[IdxArray].package;
                                                string packageVersion = listObject[IdxArray].pkgVersion;
                                                string testingmode = listObject[IdxArray].testingmode;
                                                Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                                                listObject.Add(obj);
                                                syncHashMap(obj);
                                                string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                                                System.IO.Directory.CreateDirectory(@folderResult);
                                                //System.IO.Directory.CreateDirectory(@folderResult+"//more_details");

                                                //copy from to
                                                if (File.Exists(@folderIndex + "/obj" + index + ".png"))
                                                {
                                                    File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                                }
                                                if (File.Exists(@folderIndex + "/more_details/obj" + index + ".png"))
                                                {
                                                    File.Copy(@folderIndex + "/more_details/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                                }
                                                if (File.Exists(@folderIndex + "/obj" + index + ".log"))
                                                {
                                                    File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                                                }
                                                if (File.Exists(@folderIndex + "/obj" + index + ".xml"))
                                                {
                                                    File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                                                }

                                                //File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                                //File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                                                //File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image

                                                string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                                                drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                                                //end

                                                //updateXML();
                                                //Thread.Sleep(1000);

                                                //XU LY HERE
                                                copyToMoreDetailFolder(folderResult, numberOfObject.ToString());
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string ErrorType = "NO_FOCUSABLE_HEADER";
                                        string result = GLOBAL_CONSIDER;


                                        string objId = "OBJ" + (numberOfObject + 1);
                                        //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                                        string currentScreen = listObject[IdxArray].screen;
                                        string package = listObject[IdxArray].package;
                                        string packageVersion = listObject[IdxArray].pkgVersion;
                                        string testingmode = listObject[IdxArray].testingmode;
                                        Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), allTextDes_optimize, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                                        listObject.Add(obj);
                                        syncHashMap(obj);
                                        string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                                        System.IO.Directory.CreateDirectory(@folderResult);
                                        //System.IO.Directory.CreateDirectory(@folderResult+"//more_details");

                                        //copy from to
                                        if (File.Exists(@folderIndex + "/obj" + index + ".png"))
                                        {
                                            File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                        }
                                        if (File.Exists(@folderIndex + "/more_details/obj" + index + ".png"))
                                        {
                                            File.Copy(@folderIndex + "/more_details/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                        }
                                        if (File.Exists(@folderIndex + "/obj" + index + ".log"))
                                        {
                                            File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                                        }
                                        if (File.Exists(@folderIndex + "/obj" + index + ".xml"))
                                        {
                                            File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                                        }

                                        //File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                                        //File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                                        //File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image

                                        string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                                        drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                                        //end

                                        //updateXML();
                                        //Thread.Sleep(1000);

                                        //XU LY HERE
                                        copyToMoreDetailFolder(folderResult, numberOfObject.ToString());
                                        continue;
                                    }
                                }
                            }
                            //}
                        }
                    }
                    //else if ((text1.Trim() != "" || des1.Trim() != "") && (focusable1 == "false" || clickable1 == "false"))
                    //else if (allTextDes_optimize != "" && (focusable1 == "false" || clickable1 == "false"))
                    //{
                    //    continue;//bo di
                    //    //continue;
                    //    if (isExistedObjectInfo(objectInformation,curentScreenNow) == false)
                    //    {
                    //        string result = "NC";
                    //        string ErrorType = "NO_FOCUSABLE";
                    //        //if (className1.Contains("RelativeLayout"))
                    //        //{
                    //        //    ErrorType = "NO_FOCUSABLE_ROWLIST";
                    //        //}
                    //        if (text1 + des1 != "")
                    //        {
                    //            if (focusable1 == "false")
                    //            {
                    //                XElement parent = script.Parent;
                    //                if (parent != null)
                    //                {

                    //                    string className = parent.Attribute("class").Value.ToString();
                    //                    if (className.Contains("recyclerview"))
                    //                    {
                    //                        //ErrorType = "NO_FOCUSABLE_HEADER";
                    //                        //result = "Fail";
                    //                    }
                    //                    else
                    //                    {
                    //                        string focusable2 = parent.Attribute("focusable").Value.ToString();
                    //                        if (focusable2 == "false")
                    //                        {

                    //                            XElement parent3 = parent.Parent;
                    //                            string focusable3 = parent3.Attribute("focusable").Value.ToString();
                    //                            if (focusable3 == "false")
                    //                            {
                    //                                ErrorType = "NO_FOCUSABLE_HEADER";
                    //                                result = "Fail";
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            else if (clickable1 == "false")
                    //            {
                    //                XElement parent = script.Parent;
                    //                if (parent != null)
                    //                {
                    //                    string className = parent.Attribute("class").Value.ToString();
                    //                    if (className.Contains("recyclerview"))
                    //                    {
                    //                        //ErrorType = "NO_CLICKABLE_HEADER";
                    //                        //result = "Fail";
                    //                    }
                    //                    else
                    //                    {
                    //                        string clickable2 = parent.Attribute("clickable").Value.ToString();
                    //                        if (clickable2 == "false")
                    //                        {
                    //                            XElement parent3 = parent.Parent;
                    //                            string clickable3 = parent3.Attribute("clickable").Value.ToString();
                    //                            if (clickable3 == "false")
                    //                            {
                    //                                ErrorType = "NO_CLICKABLE_HEADER";
                    //                                result = "Fail";
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            //if (focusable1 == "false" && clickable1 == "true")
                    //            //{
                    //            //    ErrorType = "NO_FOCUSABLE_ERROR";
                    //            //}
                    //            //if (focusable1 == "true" && clickable1 == "false")
                    //            //{
                    //            //    ErrorType = "NO_FOCUSABLE_ERROR";
                    //            //}
                    //            //if (focusable1 == "false" && clickable1 == "false")
                    //            //{
                    //            //    ErrorType = "NO_FOCUSABLE_HEADER";
                    //            //    result = "Fail";
                    //            //}
                    //        }

                    //        if (ErrorType == "NO_FOCUSABLE") continue;

                    //        string objId = "OBJ" + (numberOfObject + 1);
                    //        //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                    //        string currentScreen = listObject[index].screen;
                    //        string package = listObject[index].package;
                    //        string packageVersion = listObject[index].pkgVersion;
                    //        string testingmode = listObject[index].testingmode;
                    //        Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), text1 + "_" + des1, testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                    //        listObject.Add(obj);
                    //        string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                    //        System.IO.Directory.CreateDirectory(@folderResult);
                    //        //copy from to
                    //        File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                    //        File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                    //        File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                    //        string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                    //        drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                    //        //end
                    //        //updateXML();
                    //        //Thread.Sleep(1000);
                    //        continue;
                    //    }
                    //}
                    // if (ischangelang(allTextDes_optimize, getCurrentLang()) <=1)//change lang 2 la ok
                    string tbAfterRemove1 = RemoveBlackListLanguage(text1);
                    string tbAfterRemove2 = RemoveBlackListLanguage(des1);
                    if ((text1 != "" && tbAfterRemove1 != "" && ischangelang(tbAfterRemove1, getCurrentLang()) == 1) || (des1 != "" && tbAfterRemove2 != "" && ischangelang(tbAfterRemove2, getCurrentLang()) == 1))
                    {
                        //if (isExistedObjectInfo(objectInformation, curentScreenNow) == false)
                        //{
                        string result = "Consider";

                        string ErrorType = "DIFF_LANG";
                        string objId = "OBJ" + (numberOfObject + 1);
                        //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                        string currentScreen = listObject[index].screen;
                        string package = listObject[index].package;
                        string packageVersion = listObject[index].pkgVersion;
                        string testingmode = listObject[index].testingmode;
                        string contentTextDes = text1 != "" ? text1 : des1;

                        Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), contentTextDes + " (Eng words:" + global_en_word + ")", testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                        listObject.Add(obj);
                        syncHashMap(obj);
                        string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                        System.IO.Directory.CreateDirectory(@folderResult);
                        //System.IO.Directory.CreateDirectory(@folderResult + "//more_details");
                        //copy from to
                        //if(File.Exists(@folderIndex + "/obj" + index + "_canvas.png"))
                        //{
                        //    File.Copy(@folderIndex + "/obj" + index + "_canvas.png", folderResult + "/obj" + numberOfObject + "_canvas.png", true);//image
                        //}
                        if (File.Exists(@folderIndex + "/obj" + index + ".png"))
                        {
                            File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                        }
                        if (File.Exists(@folderIndex + "/more_details/obj" + index + ".png"))
                        {
                            File.Copy(@folderIndex + "/more_details/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                        }
                        if (File.Exists(@folderIndex + "/obj" + index + ".log"))
                        {
                            File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                        }
                        if (File.Exists(@folderIndex + "/obj" + index + ".xml"))
                        {
                            File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                        }
                        string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                        drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                        //end
                        //updateXML();
                        //Thread.Sleep(1000);

                        //XU LY HERE
                        copyToMoreDetailFolder(folderResult, numberOfObject.ToString());
                        continue;
                        //}
                    }

                    //string tbAfterRemove1 = RemoveBlackListLanguage(text1);
                    //string tbAfterRemove2 = RemoveBlackListLanguage(des1);

                    if ((text1 != "" && tbAfterRemove1 != "" && ischangelang(tbAfterRemove1, getCurrentLang()) == -1) || (des1 != "" && tbAfterRemove2 != "" && ischangelang(tbAfterRemove2, getCurrentLang()) == -1))
                    {
                        //if (isExistedObjectInfo(objectInformation, curentScreenNow) == false)
                        //{
                        string result = "Consider";

                        string ErrorType = "DIFF_LANG";
                        string objId = "OBJ" + (numberOfObject + 1);
                        //string currentScreen = "Same screen with OBJ" + index + ";" + listObject[index].screen;
                        string currentScreen = listObject[index].screen;
                        string package = listObject[index].package;
                        string packageVersion = listObject[index].pkgVersion;
                        string testingmode = listObject[index].testingmode;
                        string contentTextDes = text1 != "" ? text1 : des1;
                        Object obj = new Object(++numberOfObject, objId, currentScreen, package, packageVersion, objectInformation, "TalkbackText_NT", result, System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), contentTextDes + " (Eng words:" + global_en_word + ")", testingmode, Newtonsoft.Json.JsonConvert.SerializeObject(device), ErrorType);
                        listObject.Add(obj);
                        syncHashMap(obj);
                        string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject);
                        System.IO.Directory.CreateDirectory(@folderResult);
                        //System.IO.Directory.CreateDirectory(@folderResult + "//more_details");
                        //copy from to

                        //copy from to
                        //if (File.Exists(@folderIndex + "/obj" + index + "_canvas.png"))
                        //{
                        //    File.Copy(@folderIndex + "/obj" + index + "_canvas.png", folderResult + "/obj" + numberOfObject + "_canvas.png", true);//image
                        //}
                        if (File.Exists(@folderIndex + "/obj" + index + ".png"))
                        {
                            File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                        }
                        if (File.Exists(@folderIndex + "/more_details/obj" + index + ".png"))
                        {
                            File.Copy(@folderIndex + "/more_details/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                        }
                        if (File.Exists(@folderIndex + "/obj" + index + ".log"))
                        {
                            File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                        }
                        if (File.Exists(@folderIndex + "/obj" + index + ".xml"))
                        {
                            File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                        }

                        //File.Copy(@folderIndex + "/obj" + index + ".png", folderResult + "/obj" + numberOfObject + ".png", true);//image
                        //File.Copy(@folderIndex + "/obj" + index + ".log", folderResult + "/obj" + numberOfObject + ".log", true);//image
                        //File.Copy(@folderIndex + "/obj" + index + ".xml", folderResult + "/obj" + numberOfObject + ".xml", true);//image
                        string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
                        drawOnImage(folderResult + "/obj" + numberOfObject + ".png", folderResult + "/obj" + numberOfObject + "_canvas.png", Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[2]), Int16.Parse(array_bounds[4]) - Int16.Parse(array_bounds[1]), Int16.Parse(array_bounds[5]) - Int16.Parse(array_bounds[2]));
                        //end
                        //updateXML();
                        //Thread.Sleep(1000);

                        //XU LY HERE
                        copyToMoreDetailFolder(folderResult, numberOfObject.ToString());

                        continue;
                        //}
                    }
                    //else if (currentLang != getLangId(text1 + " " + des1))//later
                    //{
                    //    result = "Fail";
                    //    ErrorType = "DIFF_LANG";
                    //}


                    //kiem tra loi
                }

            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
            }
        }

        private void getAnotherObjectsFromStartEnd(int startIndex, int endIndex)
        {
            //ongoing quet het cac case fail khac trong cac xml va add them vao list
            //for (int index = startIndex + 1; index <= endIndex; index++)
            for (int index = startIndex + 1; index < endIndex; index++)
            {
                try
                {
                    analyzeXML(index);
                }
                catch
                {
                    int a = 6;
                }
            }
            updateXML();
        }




















































































































































































































































































































































        private string getCurrentLang()
        {
            string currentLocale = getCurrentLocale();
            string language = "en";
            if (currentLocale == "vi-VN") language = "vi";
            if (currentLocale == "en-US") language = "en";
            if (currentLocale == "ko-KR") language = "ko";

            if (currentLocale.Contains("en-")) language = "en";
            if (currentLocale.Contains("vi-")) language = "vi";
            if (currentLocale.Contains("ko-")) language = "ko";
            return language;
        }

        private void deleteOldData(string folderResult)
        {
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(@folderResult);

                foreach (FileInfo file in di.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        printLog("deleteOldData: " + ex.Message, "error");
                    }
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        printLog("deleteOldData: " + ex.Message, "error");
                    }


                }
            }
            catch (Exception ex)
            {
                printLog("deleteOldData: " + ex.Message, "error");
            }
        }

        private int getNumberOfObject()
        {
            if (listObject.Count == 0)
            {
                return 0;
            }
            else
            {
                return listObject.Count;
            }
        }

        private void getObjectInScreen(int MAX1SCREEN, string testingmode)
        {
            //update MAXSCREENSHOT
            try
            {
                MAX_SCREENSHOT = TalkBackAutoTest.Properties.Settings.Default.txtMaximumScreenShot;
                MAX_SCREENSHOT = MAX_SCREENSHOT > 0 && MAX_SCREENSHOT < 11 ? MAX_SCREENSHOT : 5;
            }
            catch (Exception ex)
            {
                MAX_SCREENSHOT = 5;
                printLog("update MAXSCREENSHOT:" + ex.Message, "error");
            }

            if (rdByLog.Checked == true)//toi uu case log
            {
                MAX_SCREENSHOT = 1;
            }

            printLog("getObjectInScreen MAXSCREENSHOT:" + MAX_SCREENSHOT, "info");

            int numberofEachScreen = 0;
            //get NAF object

            int countContinue = 0;
            int countContinueminus1 = 0;
            int countContinueminus2 = 0;

            string FirstObjectInfo = "";
            string FirstObjectScreen = "";

            string previousObjectInfo = "";
            string previousObjectScreen = "";

            int sum_no_focus_after_tab = 0;

            int count_not_add_lientiep = 0;
            for (int i = 1; i <= 1000; i++)
            {
                try
                {
                    //create folder result
                    int numberOfObject = getNumberOfObject();
                    ////end 2605
                    string folderResult = @txtWS.Text + "//Result//" + "obj" + (numberOfObject + 1);

                    string fileObject = (numberOfObject + 1) + "";
                    string fileName = "obj" + fileObject + ".mp4";

                    MScreenRecordingClass Recording = new MScreenRecordingClass();
                    if (Directory.Exists(@folderResult))
                    {
                        //Delete all old data
                        deleteOldData(@folderResult);
                        //end delete all old data       
                    }

                    if (!Directory.Exists(@folderResult))
                    {
                        System.IO.Directory.CreateDirectory(@folderResult);
                    }

                    //System.IO.Directory.CreateDirectory(@folderResult + "//more_details");
                    Recording.startRecordScreenAndroid(@folderResult, fileName, device.serial);
                    //bool checkNoTTS = false;
                    int count = 0;
                    while (isnotTTSInLog() == false)//van chua sach goi tiep
                    {
                        cleanLogcat();
                        count++;
                        if (count >= 5)
                        {
                            break;
                        }
                    }
                    //Thread.Sleep(8000);

                    screenShot(folderResult, "obj" + fileObject + ".png");//screenshot+tab+pull
                    //screenShot(folderResult, "obj_2nd_" + fileObject + ".png");

                    if (rdByLog.Checked == true)
                    {
                        Thread.Sleep(5000);
                    }

                    dumpWindow(folderResult, "obj" + fileObject + ".xml");

                    //Gen ra 1 file nhu cu
                    try
                    {
                        File.Copy(@folderResult + "/1_obj" + fileObject + ".png", @folderResult + "/obj" + fileObject + ".png", true);
                    }
                    catch (Exception ex)
                    {
                        printLog("Copy screenshot:" + ex.Message, "error");
                    }


                    if (rdByScreenshot.Checked == true)
                    {
                        for (int it = 1; it <= MAX_SCREENSHOT; it++)
                        {
                            CropTalkBackImage(folderResult, it + "_obj" + fileObject + ".png", it);
                        }
                    }
                    //TALKBACK_OFF();

                    //TALKBACK_ON();
                    //stop record
                    Recording.stopRecordScreenAndroid(@folderResult, fileName, device.serial, true);
                    string language = getCurrentLang();


                    //string talkbackText = getTextOCR(language, @folderResult + "//" + "croptb_obj" + fileObject + ".png", @folderResult + "/no_toast.txt");
                    HAVECOMMA2 = false;
                    string talkbackText = TRYCATCH_NA;
                    if (rdByScreenshot.Checked == true)
                    {
                        talkbackText = getTextOCRMultipleFiles(language, @folderResult, fileObject);
                    }
                    else
                    {
                        talkbackText = getTBTextByLogcat();
                    }

                    Object o = getFocusedObject(folderResult, fileObject, language, talkbackText, testingmode, "OBJ" + fileObject, folderResult + "//obj" + fileObject + ".xml");

                    count_not_add_lientiep++;
                    if (count_not_add_lientiep > 15)//20 lan lien tiep ko add duoc cai nao 3005
                    {
                        break;
                    }

                    if (o != null && o.no == -2)//check som
                    {
                        countContinueminus2++;
                        if (countContinueminus2 > 5)//lap lai 100 phan tu giong nhau thi dung
                        {
                            break;
                        }
                        continue;
                    }


                    if (o != null && o.no == -1)//check som
                    {
                        countContinueminus1++;
                        if (previousObjectInfo != "")
                        {
                            if (previousObjectInfo == o.objectInformation && previousObjectScreen == o.screen)
                            {
                                break;
                            }
                        }

                        previousObjectInfo = o.objectInformation;
                        previousObjectScreen = o.screen;

                        if (countContinueminus1 > 3)//lap lai 3 phan tu giong nhau thi dung
                        {
                            break;
                        }
                        continue;

                    }


                    //printLog("Stop Record:" + fileName);

                    int countContinueRecent = 0;
                    //if (o != null && (o.talkbackText.Contains("Gần đây, Nút") || o.talkbackText.Contains("Recents, Button")))
                    if (o != null && InIgnoreList(o.talkbackText))
                    {
                        //begin 2605
                        numberOfObject--;
                        //end 2605
                        countContinueRecent++;
                        if (countContinueRecent >= 5)
                        {
                            countContinueRecent = 0;
                            break;
                        }
                        continue;
                    }


                    if (o == null || o.no == 0)
                    {
                        //return;
                        if (listObject.Count == 0 || cb_NoOutputAfterTab.Checked == false)// nhung case khong chon cai nay moi dem, ko thi continue
                        {
                            countContinue++;
                        }
                        if (countContinue >= 5)
                        {
                            countContinue = 0;
                            break;
                        }
                        continue;
                    }

                    bool haveSecondObject = false;
                    if (FirstObjectInfo != "")
                    {
                        if (isExistedFirstObject(FirstObjectInfo, FirstObjectScreen, o.objectInformation, o.screen) && numberOfObject > 2 && haveSecondObject == true)
                        {
                            //begin 2605
                            numberOfObject--;
                            //end 2605

                            break;// lap lai phan tu dau tien
                        }
                        else
                        {
                            haveSecondObject = true;
                        }
                    }

                    if (FirstObjectInfo == "")
                    {
                        FirstObjectInfo = o.objectInformation;
                        FirstObjectScreen = o.screen;
                    }




                    if (o.errortype == "NO_CHANGE_FOCUS_AFTER_TAB")
                    {
                        sum_no_focus_after_tab++;
                    }
                    else
                    {
                        sum_no_focus_after_tab = 0;
                    }

                    if (sum_no_focus_after_tab > 1 && o.errortype == "NO_CHANGE_FOCUS_AFTER_TAB")//no focus after tab lan 2
                    {
                        //begin 2605
                        numberOfObject--;
                        //end 2605
                        continue;
                    }

                    if (sum_no_focus_after_tab >= 20 && o.package == "com.samsung.android.voc")//qua 20 lan no focus after tab
                    {
                        //begin 2605
                        numberOfObject--;
                        //end 2605
                        break;
                    }
                    if (sum_no_focus_after_tab >= 5)//qua 5 lan no focus after tab
                    {
                        //begin 2605
                        numberOfObject--;
                        //end 2605
                        break;
                    }

                    if (o != null && o.focusedObject != "")
                    {
                        //update testing mode
                        //o.testingmode = testingmode;
                        // o.deviceInfo = Newtonsoft.Json.JsonConvert.SerializeObject(device);

                        //begin 2605
                        if (listObject.Count == 0)
                        {
                            numberOfObject = 1;
                            o.no = 1;
                            o.focusedObject = "OBJ1";
                        }
                        else
                        {
                            numberOfObject = listObject.Count;
                            o.no = listObject.Count + 1;
                            o.focusedObject = "OBJ" + o.no;
                        }
                        //end 2605
                        listObject.Add(o);
                        syncHashMap(o);
                        updateXML();
                        numberofEachScreen++;
                        getlogcat(folderResult + "//" + "obj" + fileObject + ".log", 10000);

                        copyToMoreDetailFolder(folderResult, fileObject);
                        //XU LY HERE

                        //reset neu nhu add dc cai khac vao
                        countContinue = 0;
                        countContinueRecent = 0;
                        count_not_add_lientiep = 0;
                    }



                    if (numberofEachScreen >= MAX1SCREEN)
                    {
                        break;
                    }

                }
                catch (Exception ex)
                {
                    printLog("getObjectInScreen Inside:" + ex.Message, "error");
                }
            }
        }

        private string getTBTextByLogcat()
        {
            string[] kq = getTTSText(", ", "auto");
            return kq[1];
        }

        private bool InIgnoreList(string talkbackText)
        {
            string[] ignoreArray = new string[] { "Gần đây, Nút", "Gân đây Nút", "Gân đây, Nút", "Gân đây; Nút", "Recents, Button", "Bảng ở cạnh", "Edge panels", "Showing Samsung Keyboard", };
            foreach (string x in ignoreArray)
            {
                if (talkbackText.ToLower().Contains(x.ToLower()))
                {
                    return true;
                }

            }

            return false;
        }

        private void deleteResultFolder()
        {
            try
            {
                if (Directory.Exists(@txtWS.Text + "//Result"))
                {
                    Directory.Delete(@txtWS.Text + "//Result", true);
                }
            }
            catch (Exception ex)
            {
                printLog("deleteResultFolder:" + ex.Message, "error");
            }
        }

        private void TALKBACK_ON()
        {
            if (device.serial != null)
            {
                printLog("Start Talkback Service");
                RunCommand("adb -s " + device.serial + " shell settings put secure enabled_accessibility_services com.samsung.android.accessibility.talkback/com.samsung.android.marvin.talkback.TalkBackService", 5000);
                Thread.Sleep(5000);
            }
        }
        private void TALKBACK_OFF()
        {
            if (device.serial != null)
            {
                printLog("Stop Talkback Service");
                RunCommand("adb -s " + device.serial + " shell settings put secure enabled_accessibility_services com.android.talkback/com.google.android.marvin.talkback.TalkBackService");
            }
        }

        private void initCOUNTIGNORE()
        {
            COUNT_IGNORE_MYFILES = 0;
            COUNT_IGNORE_NOTES = 0;
            COUNT_IGNORE_GALLERY = 0;
            COUNT_IGNORE_VOICE = 0;
            COUNT_IGNORE_CLOCK = 0;
        }


        private bool InBlackListScreen(string screenName)
        {
            string blacklist_screen = TalkBackAutoTest.Properties.Settings.Default.blacklist_screen; 
            string[] lines = blacklist_screen.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (line.Trim() == screenName)
                {
                    return true;
                }
            }

            return false;
        }



        private void RunProjectWithNewThread()
        {
            try
            {
                //set night mode false
                setDarkMode(false);
                //}
                int MAX1SCREEN = txtEventNumberMax1screen.Text == "" ? 200 : Int16.Parse(txtEventNumberMax1screen.Text);
                int MAXALLSCREEN = txtEventNumberMaxAll.Text == "" ? 1000 : Int16.Parse(txtEventNumberMaxAll.Text);

                string testtingMode = "";


                string oldScreen = "";


                initCOUNTIGNORE();

                if (rbStressTestMode.Checked == true)//STRESSTEST MODE
                {

                    if (stressTestPkg == "")
                    {
                        printLog("Not support empty package");
                        return;
                    }

                    testtingMode = "STRESSTEST";
                    string GLOBAL_FAIL = "Fail";
                    string GLOBAL_PASS = "Pass";
                    string GLOBAL_CONSIDER = "Consider";
                    //02. Talkback On
                    TALKBACK_ON();
                    //Create Folder Result
                    System.IO.Directory.CreateDirectory(@txtWS.Text + "//Result");

                    //update number of project

                    //03. adb Tab
                    bool stopTest = false;
                    for (int i1 = 1; i1 <= 1000; i1++)
                    {
                        try
                        {
                            if (getNumberOfObject() >= MAXALLSCREEN)
                            {
                                break;
                            }
                            //if (i1 == 1)//first time
                            //{
                            //    RunCommand("adb shell monkey -p " + txtPkg.Text + " 200", 2000);
                            //}
                            if (device.serial != null)
                            {
                                int idxTimeout = 0;
                                while (true)
                                {
                                    //string actualStressTestPkg = stressTestPkg;
                                    string outputCommand ="";
                                    if (stressTestPkg == ALL_APPS)
                                    {
                                        //actualStressTestPkg = "com.sec.android.app.clockpackage";
                                        TALKBACK_OFF();
                                        outputCommand = RunCommand("adb -s " + device.serial + " shell monkey " + (500 + idxTimeout * 50), 2000);
                                    }
                                    else
                                    {
                                        TALKBACK_OFF();
                                        outputCommand = RunCommand("adb -s " + device.serial + " shell monkey -p " + stressTestPkg + " " + (500 + idxTimeout * 50), 2000);
                                    }

                                    if (outputCommand.Contains("monkey aborted"))
                                    {
                                        printLog("This package not support Monkey test");
                                        stopTest = true;
                                        break;
                                    }
                                    //RunCommand("adb -s " + device.serial + " shell monkey -p " + actualStressTestPkg + " " + (200 + idxTimeout * 20), 2000);
                                    string currentScreen = getCurrentScreen();

                                    if (stressTestPkg != ALL_APPS)
                                    {
                                        if (!currentScreen.Contains(stressTestPkg))//ko phai pkg can
                                        {
                                            idxTimeout++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (!currentScreenContainAnyPackage(currentScreen))
                                        {
                                            idxTimeout++;
                                            continue;
                                        }

                                        if (InBlackListScreen(currentScreen))
                                        {
                                            continue;
                                        }
                                    }

                                    if (currentScreen != oldScreen)
                                    {
                                        oldScreen = currentScreen;
                                        
                                        break;
                                    }
                                    if (idxTimeout >= 8)
                                    {
                                        stopTest = true;
                                        break;
                                    }

                                    idxTimeout++;
                                }


                                //xu ly here dung test
                                if (stopTest == true)
                                {
                                    break;
                                }

                                //HERE
                                TALKBACK_ON();


                                //string currenScreen = getCurrentScreen();
                                int indexStart = getNumberOfObject();
                                getObjectInScreen(MAX1SCREEN, testtingMode);
                                int indexEnd = getNumberOfObject();

                                getAnotherObjectsFromStartEnd(indexStart, indexEnd);

                            }
                        }
                        catch (Exception ex)
                        {
                            printLog("RunProjectWithNewThread Inside:" + ex.Message, "error");
                        }

                    }
                    //04. Screenshot and get takback text/soud (need on talkback show text first)

                    //04. Dump to get focused object

                    //05 display on listview and compare result, update both xml file

                    //06. Show result

                    //talkback off
                    TALKBACK_OFF();
                    this.Invoke(new Action(() =>
                    {
                     MessageBox.Show("Test StressTest done. please try more screens");
                    }));
                }
                else if (rbManualMode.Checked == true)//dang o 1 man hinh manual
                {
                    testtingMode = "SEMI-AUTO";
                    string GLOBAL_FAIL = "Fail";
                    string GLOBAL_PASS = "Pass";
                    string GLOBAL_CONSIDER = "Consider";


                    string currentScreen = getCurrentScreen();
                    if (InBlackListScreen(currentScreen))
                    {
                        MessageBox.Show("Bot support because it's in Blacklist screen. please try more screens");
                        return;       
                    }

                    //02. Talkback On
                    TALKBACK_ON();
                    int indexStart = getNumberOfObject();
                    getObjectInScreen(MAX1SCREEN, testtingMode);
                    int indexEnd = getNumberOfObject();
                    getAnotherObjectsFromStartEnd(indexStart, indexEnd);
                    //talkback off
                    TALKBACK_OFF();
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Test Semi-Auto done. please try more screens");
                    }));
                }
                else if (rbActivityMode.Checked == true)//dang o 1 man hinh manual
                {
                    //developing
                    if (stressTestPkg == "")
                    {
                        printLog("Not support empty package");
                        return;
                    }

                   
                    testtingMode = "ACTIVITY MODE";
                    string GLOBAL_FAIL = "Consider";
                    string GLOBAL_PASS = "Pass";
                    string GLOBAL_CONSIDER = "Consider";



                   


                    if (stressTestPkg == ALL_APPS)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Not support:" + ALL_APPS + " for mode:" + testtingMode);
                        }));
                        return;
                    }

                    //02. Talkback On
                    TALKBACK_ON();
                   
                    //grant permission of activity
                    //GrantPermission(stressTestPkg, "grant");
                    //get All activites of package
                    string[] kq_activity = getActivityList(stressTestPkg);


                    this.Invoke(new Action(() =>
                    {
                        ActivityForm frm2 = new ActivityForm(this);
                        frm2.StartPosition = FormStartPosition.CenterParent;
                        frm2.TopMost = true;
                        frm2.ShowDialog(this);
                    }));

                    string path = txtWS.Text + "\\list_activity.txt";
                    kq_activity = File.ReadAllLines(@path);
                    


                    //foreach al activity
                    if (kq_activity != null && kq_activity.Count() > 0)
                    {
                        foreach (string activity in kq_activity)
                        {
                            try
                            {
                                RunCommand("adb -s " + device.serial + " shell input keyevent 3&adb -s " + device.serial + " shell input keyevent 3");//HOME
                                cleanLogcat();
                                string[] rs = activity.Split(new string[] { ";" }, StringSplitOptions.None);
                                string error = RunCommandGetStandartErrorAndOuput("adb -s " + device.serial + " shell am start -n " + rs[0] + "/" + rs[1]);

                                if (error.Contains("Permission Denial") || error.Contains("Requires permission"))
                                {
                                    printLog("Not check activity: " + rs[1] + " because of: Permission Denial");
                                    cleanLogcat();
                                    continue;
                                }
                                else if (error.Contains("Activity not started"))
                                {
                                    printLog("Not check activity: " + rs[1] + " because of: Activity not started");
                                    cleanLogcat();
                                    continue;
                                }
                                else if (error.Contains("does not exist"))
                                {
                                    printLog("Not check activity: " + rs[1] + " because of: does not exist");
                                    cleanLogcat();
                                    continue;
                                }

                                if (isFCAndAnr())
                                {
                                    printLog("Not check activity: "+rs[1]+" because of: FC or ANR");
                                    cleanLogcat();
                                    continue;
                                }

                                //else check here
                                //string currentScreen = getCurrentScreen();

                                string currentScreen = getCurrentScreen();
                                if (InBlackListScreen(currentScreen))
                                {
                                    printLog("Not support because it's in Blacklist screen. please try more screens");
                                    cleanLogcat();
                                    continue;
                                }

                                if (!currentScreen.Contains(stressTestPkg))
                                {
                                    printLog("Not check activity: " + rs[1] + " because of: No UI");
                                    cleanLogcat();
                                    continue;
                                }


                                int indexStart = getNumberOfObject();
                                getObjectInScreen(MAX1SCREEN, testtingMode);
                                int indexEnd = getNumberOfObject();
                                getAnotherObjectsFromStartEnd(indexStart, indexEnd);
                            }
                            catch
                            {
                            }

                        }
                    }
                    //start activity if not fc
                    //fc skip

                    

                    //endfor

                    ////talkback off
                    TALKBACK_OFF();
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Test Activity mode done. Please try more screens");
                    }));
                }
                //else if (rbScriptMode.Checked == true)//dang o 1 man hinh manual
                //{
                //    //developing
                //    testtingMode = "SCRIPT";
                //    printLog("developing");
                //}


            }
            catch (Exception ex)
            {
                printLog("RunProjectWithNewThread:" + ex.Message, "error");
            }
            finally
            {

                this.Invoke(new Action(() =>
                    {
                        btnRun.Text = "Run";
                        btnRun.BackColor = Color.FromArgb(192, 255, 192);
                    }));
                }
                //dasd
                //update again button stop thread
                printLog("TRYCAT_FINNALLY", "error");
                if (threadRunProject != null && threadRunProject.IsAlive)
                {
                    threadRunProject.Abort();
                }
                TALKBACK_OFF();
        }


        private string RunCommandGetStandartErrorAndOuput(string command, string type = "")
        {
            string rs = "";
            try
            {
                if (device.serial != "")
                {
                    Process p1 = new Process();
                    // Redirect the output stream of the child process.
                    p1.StartInfo.UseShellExecute = false;
                    p1.StartInfo.RedirectStandardError = true;
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                    p1.StartInfo.Arguments = "/c " + command;
                    printLog(command);
                    p1.StartInfo.CreateNoWindow = true;
                    p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p1.Start();

                    string output1 = p1.StandardError.ReadToEnd();
                    string output2 = p1.StandardOutput.ReadToEnd();
                    p1.WaitForExit();
                    return output1 +" "+output2;
                }
                else
                {
                    printLog("Device is disconnected.Can not run command");
                    return "Device is disconnected.Can not run command";
                }
            }
            catch (Exception ex)
            {
                printLog(ex.Message);
                return ex.Message;
            }

        }

        private bool isFCAndAnr()
        {
            try
            {
                Process p1 = new Process();
                // Redirect the output stream of the child process.
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                // RunCommand("adb -s " + current_serial + " shell logcat -v threadtime -t 1000 >" + pathName + "/LOG_FC.txt");
                p1.StartInfo.Arguments = "/c adb -s " + device.serial + " shell \"logcat -v threadtime -t 30000|grep -E 'FATAL EXCEPTION|F libc    : Fatal signal|am_anr|E ActivityManager: ANR in'\"";
                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p1.Start();
                string output = p1.StandardOutput.ReadToEnd();
                p1.WaitForExit();
                if (output == "")//ko co cai nao thi ko FC
                {
                    return false;
                }
                else
                {
                    var op = output.Split(new string[] { "\\n" }, StringSplitOptions.None);
                    foreach (string line in op)
                    {
                        //F libc    : Fatal signal
                        //MessageBox.Show(line);
                        if ((line.Contains("FATAL EXCEPTION") || line.Contains("F libc    : Fatal signal")) && !line.Contains("logcat -v threadtime") && !line.Contains("adbd"))
                        {
                            return true;
                        }
                        if ((line.Contains("am_anr") || line.Contains("E ActivityManager: ANR in")) && !line.Contains("logcat -v threadtime") && !line.Contains("adbd"))
                        {
                            return true;
                        }
                    }
                }


                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private void stopMonkey()
        {
            if (device.serial != null)
            {
                Process p = new Process();
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                p.StartInfo.Arguments = "/c adb -s " + device.serial + " shell kill $(pgrep monkey)";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                string output = p.StandardError.ReadToEnd();
            }
        }


        private string DetectEnvironment()
        {
            string msg = "";
            try
            {
                if (rdByLog.Checked == true)
                {
                    printLog("CheckByLogcat no need DetectEnvironment");
                    return "";
                }

                string output = RunCommand("where easyocr");
                if (output == "")
                {
                    msg += "Please install easyocr via command pip install easyocr\r\n";
                }

                //string outputOpenCV = RunCommand("python -c \"import cv2; print(cv2.__version__)\"");
                string outputOpenCV = RunCommand("pip show opencv-python");

                if (outputOpenCV == "")
                {
                    msg += "Please install opencv via command pip install opencv-python numpy easyocr\r\n";
                }


                if (!File.Exists(@AI_MODEL_FILES))
                {
                    msg += "Please add mode files to path:" + @AI_MODEL_PATH + "\r\n";
                }
                if (txtWS.Text == "")
                {
                    msg += "Please set Workspace path first " + "\r\n";
                }
                return msg == "" ? "" : "You need install environment files before running as below:\r\n\r\n" + msg;
            }
            catch (Exception ex)
            {
                printLog("DetectEnvironment:" + ex.Message, "error");
                return "";
            }
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            //detect environment
            //set active tab
            tabControl1.SelectTab(tabPage1);

            if (device.serial != null)
            {
                if (btnRun.Text == "Run")
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want run this project?", "Run Project", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {

                        string msg = DetectEnvironment();
                        if (msg != "")
                        {
                            MessageBox.Show(msg);
                            return;
                        }

                        ConfirmPopUp f1 = new ConfirmPopUp();
                        f1.StartPosition = FormStartPosition.CenterParent;
                        f1.TopMost = true;
                        var result = f1.ShowDialog();

                        if (result == DialogResult.Yes)
                        {
                            listObject.Clear();
                            hashMap.Clear();
                            //updateNumberOfProject(0);
                            //delete Result folder?????
                            deleteResultFolder();
                            //end
                        }


                        threadRunProject = new Thread(RunProjectWithNewThread);
                        threadRunProject.Name = "Run Project with new Thread";
                        if (!threadRunProject.IsAlive)
                            threadRunProject.Start();

                        btnRun.Text = "Stop";
                        btnRun.BackColor = Color.FromArgb(255, 128, 128);
                    }
                }
                else//stop
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want stop project?", "Stop Project", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        threadRunProject.Abort();
                        btnRun.Text = "Run";
                        btnRun.BackColor = Color.FromArgb(192, 255, 192);
                        stopMonkey();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please connect device first");
            }
        }

        private void updateXML()
        {

            string xmlContent = @"<?xml version='1.0' encoding='UTF-8'?>" + "\r\n\r\n" + "<TestRun>" + "\r\n" + "  <!--An XML for Data map-->" + "\r\n";

            foreach (Object x in listObject)
            {
                //string deviceInfo = device.serial == null ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(device);
                string line = @"  <Script DeviceInfo='" + x.deviceInfo + "' TestingMode ='" + x.testingmode + "' No='" + x.no + "' FocusedObject='" + x.focusedObject + "' Screen='" + x.screen + "' Package='" + x.package + "' PkgVersion='" + x.pkgVersion + "' ObjectInformation='" + x.objectInformation + "' TalkbackText='" + x.talkbackText + "' Result='" + x.result + "' TestingTime='" + x.testingtime + "' Remark='" + x.remark + "' ErrorType='" + x.errortype + "'  />";
                xmlContent += line + "\r\n";
            }

            xmlContent += "</TestRun>";
            //string xmlContent = MySerializer<Object>.Serialize(lists);
            //write
            string xmlPath = txtWS.Text + "\\result.xml";
            using (StreamWriter writer1 =
                        new StreamWriter(xmlPath))
            {
                writer1.Write(xmlContent);
            }

            readXml(@xmlPath);
            //int m = 5;
        }

        private void updatecomboBoxPkg()
        {
            //initProject();
            listAppName = getListAppName();
            List<AppName> monkeyList = new List<AppName>();
            foreach (AppName x in listAppName)
            {
                if (x.supportMonkey == true)
                {
                    //monkeyList.Add(x);
                    monkeyList.Add(new AppName(x.appName, x.pkgName, x.supportMonkey));
                }
            }
            monkeyList.Sort((a, b) => string.Compare(a.pkgName, b.pkgName, StringComparison.Ordinal));
            //biding data
            txtPkg.DataSource = monkeyList;
            txtPkg.DisplayMember = "pkgName";
            txtPkg.ValueMember = "pkgName";
            renderDone = true;
        }


        private void updatecomboBoxPkgSearch()
        {
            //initProject();
            List<AppName> listAppNameSearch = getListAppNameSearch();

            if (listAppNameSearch.Count == 0)
            {
                listAppNameSearch = listAppName;
            }

            List<AppName> monkeyList = new List<AppName>();
            foreach (AppName x in listAppNameSearch)
            {
                if (x.supportMonkey == true)
                {
                    //monkeyList.Add(x);
                    monkeyList.Add(new AppName(x.appName, x.pkgName, x.supportMonkey));
                }
            }
            monkeyList.Sort((a, b) => string.Compare(a.pkgName, b.pkgName, StringComparison.Ordinal));
            //biding data
            txtPkg.DataSource = monkeyList;
            txtPkg.DisplayMember = "pkgName";
            txtPkg.ValueMember = "pkgName";
            renderDone = true;
        }

        private void fakeCombobox()
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //fakeCombobox();

            //int a = 5;
        }

        private void dumpWindow(string path, string name)
        {
            try
            {
                if (device.serial != null)
                {
                    Process p1 = new Process();
                    // Redirect the output stream of the child process.
                    p1.StartInfo.UseShellExecute = false;
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                    //p1.StartInfo.Arguments = "/c adb -s " + current_serial + " shell monkey -p " + packagename + " 1";
                    string command = "adb -s " + device.serial + " shell uiautomator dump && adb -s " + device.serial + " pull /sdcard/window_dump.xml " + @path + "//" + name;
                    p1.StartInfo.Arguments = "/c " + command;
                    printLog(command);
                    p1.StartInfo.CreateNoWindow = true;
                    p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p1.Start();
                    //Thread.Sleep(60000);
                    string output1 = p1.StandardOutput.ReadToEnd();
                    p1.WaitForExit();
                    Thread.Sleep(10000);//need optimize timeout here
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch:" + ex.Message);
                printLog("dumpWindow:" + ex.Message, "error");
            }
        }

        private void screenShot(string path, string name)
        {
            try
            {
                float SLEEPTIME = 0.5f;
                if (rdByLog.Checked == true)
                {
                    SLEEPTIME = 0.5f;
                }
                Process p1 = new Process();
                // Redirect the output stream of the child process.
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";

                string s = "/c adb -s " + device.serial + " shell \"input keyevent TAB";
                for (int i = 1; i <= MAX_SCREENSHOT; i++)
                {
                    s += ";sleep " + SLEEPTIME + ";screencap -p /data/local/tmp/" + i + "_" + name + "";
                }
                s += "\"";
                s += " & adb -s " + device.serial + " pull ";

                for (int i = 1; i <= MAX_SCREENSHOT; i++)
                {
                    s += "\"/data/local/tmp/" + i + "_" + name + "\" ";
                }

                s += "\"" + @path + "\" & adb -s " + device.serial + " shell \"rm";

                for (int i = 1; i <= MAX_SCREENSHOT; i++)
                {
                    s += " /data/local/tmp/" + i + "_" + name + "";
                }
                s += "\"";


                printLog(s);
                p1.StartInfo.Arguments = s;
                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //Thread.Sleep(500);//screenshot
                p1.Start();
                //Thread.Sleep(60000);
                string output1 = p1.StandardOutput.ReadToEnd();
                p1.WaitForExit();

                //Rename

                //Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch:" + ex.Message);
                printLog("screenShot:" + ex.Message, "error");
            }
        }

        private string RunCommandOCR(string command, int waitTime = 0)
        {
            try
            {
                Process p1 = new Process();
                // Redirect the output stream of the child process.
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                //p1.StartInfo.Arguments = "/c adb -s " + current_serial + " shell monkey -p " + packagename + " 1";
                p1.StartInfo.Arguments = "/c " + command;
                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                p1.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

                Thread.Sleep(500);//case screenshot
                p1.Start();
                printLog(command);
                //Thread.Sleep(60000);
                string output1 = p1.StandardOutput.ReadToEnd();
                string allText = "NA";
                if (output1.Contains("ok"))
                {
                    if (File.Exists(@PY_RS_PATH))
                    {
                        allText = File.ReadAllText(@PY_RS_PATH, Encoding.UTF8);
                    }
                }
                p1.WaitForExit();

                Thread.Sleep(waitTime);

                // allText = allText.Replace(";", ",").Replace(".", ",");
                allText = allText.Replace(";", ",");
                return allText;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch:" + ex.Message);
                printLog("RunCommandOCR:" + ex.Message, "error");
                return TRYCATCH_NA;
            }
        }

        private string RunCommandShow(string command, int waitTime = 0)
        {
            try
            {
                Process p1 = new Process();
                // Redirect the output stream of the child process.
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                //p1.StartInfo.Arguments = "/c adb -s " + current_serial + " shell monkey -p " + packagename + " 1";
                p1.StartInfo.Arguments = "/c " + command;
                p1.StartInfo.CreateNoWindow = false;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

                //p1.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

                //Thread.Sleep(500);//case screenshot
                p1.Start();

                //Thread.Sleep(60000);
                string output1 = p1.StandardOutput.ReadToEnd();
                printLog(command);
                p1.WaitForExit();

                Thread.Sleep(waitTime);
                return output1;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch:" + ex.Message);
                printLog("RunCommandShow:" + ex.Message, "error");
                return "";
            }
        }


        private string RunCommand(string command, int waitTime = 0)
        {
            try
            {
                Process p1 = new Process();
                // Redirect the output stream of the child process.
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                //p1.StartInfo.Arguments = "/c adb -s " + current_serial + " shell monkey -p " + packagename + " 1";
                p1.StartInfo.Arguments = "/c " + command;
                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                //p1.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

                //Thread.Sleep(500);//case screenshot
                p1.Start();

                //Thread.Sleep(60000);
                string output1 = p1.StandardOutput.ReadToEnd();
                printLog(command);
                p1.WaitForExit();

                Thread.Sleep(waitTime);
                return output1;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch:" + ex.Message);
                printLog("RunCommand:" + ex.Message, "error");
                return "";
            }
        }

        private void refeshDeviceInformation()
        {
            //    threadRunProject1 = new Thread(getDeviceInformation);
            //    threadRunProject1.Name = "Get Device Information";
            //    if (!threadRunProject1.IsAlive)
            //        threadRunProject1.Start();
            //    //fakeXML();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            //refeshDeviceInformation();
            resetGLOBAL_LANGUAGE();
            resetGLOBAL_DPI();

            threadRunProject1 = new Thread(() => getDeviceInformation(1));
            threadRunProject1.Name = "Get Device Information";
            if (!threadRunProject1.IsAlive)
                threadRunProject1.Start();
            //fakeXML();
        }

        private void updateSumaryOnUI(int total, int pass, int fail, int consider, int nt)
        {
            lbTotal.Text = "Total Objects:" + total;
            lbPass.Text = "Pass:" + pass;
            lbFail.Text = "Fail:" + fail;

            lbConsider.Text = "Consider:" + consider;
            lbNT.Text = "NT:" + nt;
        }

        //private void updateNumberOfProject(int value)
        //{
        //    numberOfObject = value;
        //}

        private void updateListViewByKey(string keyword)
        {
            if (keyword == "")
            {
                string xmlPath = txtWS.Text + "\\result.xml";
                readXml(@xmlPath);
                return;
            }
            if (listObject.Count > 0)
            {
                string keyWordNoTone = ConvertToNoTone(keyword.ToLower());
                List<Object> listObjectSearch = new List<Object>();
                {
                    foreach (Object x in listObject)
                    {
                        if (ConvertToNoTone(x.talkbackText.ToLower()).Contains(keyWordNoTone) || ConvertToNoTone(x.remark.ToLower()).Contains(keyWordNoTone) || ConvertToNoTone(x.result.ToLower()).Contains(keyWordNoTone) || ConvertToNoTone(x.errortype.ToLower()).Contains(keyWordNoTone))
                        {
                            listObjectSearch.Add(x);
                        }
                    }
                }
                //update list view
                this.Invoke(new Action(() =>
                {
                    lvResult.Items.Clear();
                    int dem = 0;

                    for (int i = 0; i < listObjectSearch.Count; i++)
                    {
                        try
                        {
                            dem++;
                            //TimeResult                  
                            ListViewItem item1 = new ListViewItem();
                            Object x = listObjectSearch[i];
                            item1.Text = x.no.ToString();
                            //item1.SubItems.Add(AppName);
                            item1.SubItems.Add(x.focusedObject);

                            item1.SubItems.Add(x.errortype);
                            item1.SubItems.Add(x.talkbackText);
                            item1.SubItems.Add(x.remark);


                            item1.SubItems.Add(x.screen);

                            item1.SubItems.Add(x.package);
                            item1.SubItems.Add(x.pkgVersion);


                            item1.SubItems.Add(x.result);

                            item1.SubItems.Add(x.testingtime);
                            item1.SubItems.Add(x.testingmode);
                            item1.SubItems.Add(UnescapeXml(x.objectInformation));

                            string Result = x.result;
                            //Add the items to the ListView.
                            lvResult.Items.AddRange(new ListViewItem[] { item1 });
                            if (Result == "Fail")
                            {
                                item1.BackColor = Color.Red;
                                //fail++;
                            }
                            else if (Result == "NT")
                            {
                                item1.BackColor = Color.WhiteSmoke;
                                //nt++;
                                //fail++;
                            }
                            else if (Result == "Pass")
                            {
                                //total_pass++;
                                item1.BackColor = Color.LightGreen;
                                //pass++;
                            }
                            else if (Result == "Consider")
                            {
                                //total_pass++;
                                //item1.BackColor = Color.OrangeRed;
                                item1.BackColor = ColorTranslator.FromHtml("#FFB84D");
                                //consider++;
                            }
                            else//other case
                            {
                                //pass++;
                            }

                        }
                        catch (Exception ex)
                        {
                            printLog("readXmlInforSeach:" + ex.Message, "info");
                        }
                    }
                }));
            }

        }

        private void readXml(string xml_path)
        {
            this.Invoke(new Action(() =>
            {
                lvResult.Items.Clear();
                if (File.Exists(@xml_path) == false)
                {
                    //update on UI
                    listObject.Clear();
                    hashMap.Clear();
                    updateSumaryOnUI(0, 0, 0, 0, 0);
                    return;
                }
                try
                {
                    doc = XDocument.Load(@xml_path);
                    var Scripts = doc.Descendants("Script");

                    int dem = 0;

                    int elementCount = doc.Root.Elements().Count();


                    //update numberojpoject
                    //updateNumberOfProject(elementCount);

                    int total = 0, pass = 0, fail = 0, consider = 0, nt = 0;

                    listObject.Clear();
                    hashMap.Clear();

                    for (int i = 0; i < elementCount; i++)
                    {
                        try
                        {
                            XElement script = Scripts.ElementAt(i);
                            dem++;
                            string FocusedObject = script.Attribute("FocusedObject").Value;
                            string Screen = script.Attribute("Screen").Value;
                            string Package = script.Attribute("Package").Value;
                            string PkgVersion = script.Attribute("PkgVersion").Value;

                            string ObjectInformation = script.Attribute("ObjectInformation").Value;
                            string TalkbackText = script.Attribute("TalkbackText").Value;
                            string Result = script.Attribute("Result").Value;

                            string TestingTime = script.Attribute("TestingTime") != null ? script.Attribute("TestingTime").Value : "NA";
                            string TestingMode = script.Attribute("TestingMode") != null ? script.Attribute("TestingMode").Value : "NA";
                            string No = script.Attribute("No") != null ? script.Attribute("No").Value : dem.ToString();
                            string Remark = script.Attribute("Remark") != null ? script.Attribute("Remark").Value : "NA";
                            string DeviceInf = script.Attribute("DeviceInfo") != null ? script.Attribute("DeviceInfo").Value : "";
                            string ErrorType = script.Attribute("ErrorType") != null ? script.Attribute("ErrorType").Value : "NA";

                            //TimeResult                  
                            ListViewItem item1 = new ListViewItem();



                            item1.ToolTipText = "Hihi";
                            item1.Text = No;
                            //item1.SubItems.Add(AppName);
                            item1.SubItems.Add(FocusedObject);

                            item1.SubItems.Add(ErrorType);
                            item1.SubItems.Add(TalkbackText);
                            item1.SubItems.Add(Remark);


                            item1.SubItems.Add(Screen);

                            item1.SubItems.Add(Package);
                            item1.SubItems.Add(PkgVersion);


                            item1.SubItems.Add(Result);

                            item1.SubItems.Add(TestingTime);
                            item1.SubItems.Add(TestingMode);
                            item1.SubItems.Add(ObjectInformation);

                            total++;

                            //Add the items to the ListView.
                            lvResult.Items.AddRange(new ListViewItem[] { item1 });
                            if (Result == "Fail")
                            {
                                item1.BackColor = Color.Red;
                                fail++;
                            }
                            else if (Result == "NT")
                            {
                                item1.BackColor = Color.WhiteSmoke;
                                nt++;
                                //fail++;
                            }
                            else if (Result == "Pass")
                            {
                                //total_pass++;
                                item1.BackColor = Color.LightGreen;
                                pass++;
                            }
                            else if (Result == "Consider")
                            {
                                //total_pass++;
                                //item1.BackColor = Color.OrangeRed;
                                item1.BackColor = ColorTranslator.FromHtml("#FFB84D");
                                consider++;
                            }
                            else//other case
                            {
                                pass++;
                            }

                            Object o = new Object(Int16.Parse(No), FocusedObject, Screen, Package, PkgVersion, ObjectInformation, TalkbackText, Result, TestingTime, Remark, TestingMode, DeviceInf, ErrorType);
                            listObject.Add(o);
                            //hash map here
                            syncHashMap(o);
                            //end hashmap

                            
                        }
                        catch (Exception ex)
                        {
                            printLog("readXmlInfor:" + ex.Message, "info");
                        }
                    }
                    //update on UI
                    updateSumaryOnUI(total, pass, fail, consider, nt);
                    lvResult.Items[listObject.Count - 1].EnsureVisible();

                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    printLog("readXml:" + ex.Message, "error");
                }
            }));
        }

        private void syncHashMap(Object o)
        {
            string info1 = UnescapeXml(o.objectInformation);
            XElement script1 = XElement.Parse(info1);
            XDocument doc1 = XDocument.Parse(script1.ToString());
            var childs1 = doc1.Descendants("node");
            string resId1 = "", clazz1 = "", text1 = "", des1 = "";
            foreach (XElement x in childs1)
            {
                resId1 += x.Attribute("resource-id").Value.ToString() + " ";
                clazz1 += x.Attribute("class").Value.ToString() + " ";
                text1 += x.Attribute("text").Value.ToString() + " ";
                des1 += x.Attribute("content-desc").Value.ToString() + " ";
                //break;
            }
            hashMap[resId1 + "_" + clazz1 + "_" + text1 + "_" + des1] = o;
        }

        private void updateDeviceInfomationonUI(string modelName, string binaryName, string serial, string type, string branch)
        {
            this.Invoke(new Action(() =>
            {
                lbModel.Text = "Model:" + modelName;
                lbBinary.Text = "Binary:" + binaryName;
                lbSerial.Text = "Serial:" + serial;
                lbType.Text = "Type:" + type;
                lbBranch.Text = "Branch:" + branch;
                lbLang.Text = "System Language:" + getCurrentLang();

                //update here

                if (type == "user")
                {
                    rdByScreenshot.Checked = true;
                    rdByLog.Checked = false;
                    rdByLog.Enabled = false;
                }
                else if (type == "userdebug" || type == "eng")
                {
                    rdByScreenshot.Checked = false;
                    rdByLog.Checked = true;
                    rdByLog.Enabled = true;
                }
            }));
        }


        private string ToModelName(string s)
        {
            return s.Trim();
        }

        private string ToBinaryName(string s)
        {
            return s.Trim();
        }

        private string ToType(string s)
        {
            return s.Trim();
        }

        private string ToBranch(string s)
        {
            return s.Trim();
        }

        private string updateLang(string output)
        {
            if (output == "") return "NA";

            string[] rs = output.Split(new string[] { " " }, StringSplitOptions.None);

            return rs[1].Replace("[", "").Replace("]", "").Trim();
        }

        private string ToOS(string output, string output1)
        {
            // xu ly
            string os = "";
            output = output.TrimEnd('\r', '\n');
            output1 = output1.TrimEnd('\r', '\n');

            //BOS
            if (output == "16") return "B OS";
            if (output == "15" && output1.ToLower() == "baklava") return "B OS";
            if (output == "15" && output1.ToLower() == "16") return "B OS";

            //VOS
            if (output == "15") return "V OS";
            else if (output == "14" && output1.ToLower() == "vanillaicecream") return "V OS";
            else if (output == "14" && output1.ToLower() == "15") return "V OS";

            if (output == "14") return "U OS";
            else if (output == "13" && output1.ToLower() == "upsidedowncake") return "U OS";
            else if (output == "13" && output1.ToLower() == "14") return "U OS";

            else if (output == "13") return "T OS";
            else if (output == "12") return "S OS";
            else if (output == "11") return "R OS";
            else if (output == "10") return "Q OS";
            else if (output == "9") return "P OS";
            else if (output == "8") return "O OS";
            else if (output == "7") return "N OS";
            else if (output == "6") return "M OS";
            else if (output == "5") return "L OS";
            else if (output == "15") return "V OS";
            else if (output == "16") return "X OS";
            else if (output == "17") return "Y OS";
            return os;
        }

        private void getFastDeviceInfo(string serial)
        {
            string command = "adb -s " + serial + " shell \"getprop ro.product.model;getprop ro.build.PDA;getprop ro.build.type;getprop ro.product.brand;getprop ro.build.version.release;getprop ro.build.version.release_or_codename;getprop | grep persist.sys.locale\"";
            printLog(command);
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c " + command;

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            //handle output here
            string[] lines = output.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            device = new DeviceInfo(ToModelName(lines[0]), ToBinaryName(lines[1]), serial, ToType(lines[2]), ToBranch(lines[3]), ToOS(lines[4], lines[5]));//6 is langguage
            GLOBAL_LANGUAGE = updateLang(lines[6]);


            //GLOBAL_DPI = getDPI();
            int a = 5;
            //device = new DeviceInfo(modelName, binaryName, serial, type, branch, os);

            //end
        }

        private void getDeviceInformation(int mode = 0)
        {


            //string serial = getSerial();
            string output = getAdbDevice();
            string[] serials = getDevicesArr(output);
            string serial_no = (serials.Length >= 1) ? serials[0] : "";
            string serial = serial_no;

            bool haveOpenForm = false;


            if (threadRunProject != null && threadRunProject.IsAlive && serials.Count() == 0)
            {
                threadRunProject.Abort();
            }



            if (serials.Count() > 1)//tu 2 devices
            {

                if (mode == 0)
                {
                    foreach (string seri in serials)
                    {
                        if (device.serial != null && device.serial == seri)
                        {
                            //nothing
                            return;
                        }
                    }
                }

                listSerial = serials;

                ChooseSerialForm f1 = new ChooseSerialForm();
                f1.StartPosition = FormStartPosition.CenterScreen;
                var result = f1.ShowDialog();

                haveOpenForm = true;
                if (result == DialogResult.OK)
                {
                    serial = f1.serial;
                }
                else
                {
                    //serial_no = serials[0];
                    serial = serial_no;
                    //return;
                }
                //09042024
                //update global connect

            }
            haveOpenForm = false;
            if (serial != "")
            {
                if (device.serial != null && device.serial == serial && mode == 0)
                {
                    //nothing
                    printLog("Keep current device");
                    return;
                }
                printLog("Begin Get device information");
                //string modelName = getModelName(serial);
                //string binaryName = getBinaryName(serial);
                ////string branchName = getBrandName(serial);
                //string type = getTypeName(serial);
                //string branch = getBrandName(serial);
                //string os = getAdbDeviceOSName(serial);


                dictPkgVersion.Clear();
                getFastDeviceInfo(serial);
                //device = new DeviceInfo(modelName, binaryName, serial, type, branch, os);

                updateDeviceInfomationonUI(device.modelName, device.binaryName, device.serial, device.type, device.branch);
                printLog("Done Get device information");
            }
            else
            {
                dictPkgVersion.Clear();
                printLog("Begin remove device information");
                device = new DeviceInfo();
                updateDeviceInfomationonUI(device.modelName, device.binaryName, device.serial, device.type, device.branch);
                printLog("Done remove device information");

            }

        }

        private string getSerial()
        {
            string output = getAdbDevice();
            string[] serials = getDevicesArr(output);
            string serial_no = (serials.Length >= 1) ? serials[0] : "";
            return serial_no;
        }

        private string getAdbDeviceOSName(string serial)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb -s " + serial + " shell getprop ro.build.version.release";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();

            Process p1 = new Process();
            // Redirect the output stream of the child process.
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p1.StartInfo.Arguments = "/c adb -s " + serial + " shell getprop ro.build.version.release_or_codename";
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p1.Start();
            string output1 = p1.StandardOutput.ReadToEnd();

            p1.WaitForExit();

            // xu ly
            string os = "";
            output = output.TrimEnd('\r', '\n');
            output1 = output1.TrimEnd('\r', '\n');

            //BOS
            if (output == "16") return "B OS";
            if (output == "15" && output1.ToLower() == "baklava") return "B OS";
            if (output == "15" && output1.ToLower() == "16") return "B OS";

            //VOS
            if (output == "15") return "V OS";
            else if (output == "14" && output1.ToLower() == "vanillaicecream") return "V OS";
            else if (output == "14" && output1.ToLower() == "15") return "V OS";

            if (output == "14") return "U OS";
            else if (output == "13" && output1.ToLower() == "upsidedowncake") return "U OS";
            else if (output == "13" && output1.ToLower() == "14") return "U OS";

            else if (output == "13") return "T OS";
            else if (output == "12") return "S OS";
            else if (output == "11") return "R OS";
            else if (output == "10") return "Q OS";
            else if (output == "9") return "P OS";
            else if (output == "8") return "O OS";
            else if (output == "7") return "N OS";
            else if (output == "6") return "M OS";
            else if (output == "5") return "L OS";
            else if (output == "15") return "V OS";
            else if (output == "16") return "X OS";
            else if (output == "17") return "Y OS";
            return os;
        }

        private string getModelName(string serial)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb -s " + serial + " shell getprop ro.product.model";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return output.Trim();
        }

        private string getBinaryName(string serial)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb -s " + serial + " shell getprop ro.build.PDA";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return output.Trim();
        }

        private string getTypeName(string serial)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb -s " + serial + " shell getprop ro.build.type";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return output.Trim();
        }

        private string getBrandName(string serial)
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb -s " + serial + " shell getprop ro.product.brand";

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return output.Trim(); ;
        }

        private string getAdbDevice()
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb devices";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            return output;
        }

        private string[] getDevicesArr(string text)
        {
            string[] ele = text.Split(new string[] { "\tdevice\r\n" }, StringSplitOptions.None);

            int ele_count = ele.Length;
            //if (ele_count > 2)
            //{
            string[] ele_out = new string[ele_count - 1];
            for (int i = 0; i < ele_count - 1; i++)
            {
                string raw_line = ele[i];
                string[] raw_arr = raw_line.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                string out_val = raw_arr[(raw_arr.Length - 1)].Replace("\t", String.Empty);
                ele_out[i] = out_val;
            }
            return ele_out;
        }

        private void panel_leftTop_Paint(object sender, PaintEventArgs e)
        {

        }

        //bool refresh1 = false;
        private void Form1_Activated(object sender, EventArgs e)
        {
            //refeshDeviceInformation();
            //MessageBox.Show("Test");

        }

        delegate void StringArgReturningVoidDelegate(string text, string type);
        private void printLog(string alog, string type = "info")
        {

            if (this.label1.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(printLog);
                this.Invoke(d, new object[] { alog, type });
            }
            else
            {
                string time_a = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                logs.AppendText(time_a);
                logs.SelectionStart = logs.TextLength;
                logs.SelectionLength = 0;
                switch (type)
                {
                    case "success":
                        logs.SelectionColor = Color.Green;
                        break;
                    case "info":
                        logs.SelectionColor = Color.Blue;
                        break;
                    case "error":
                        logs.SelectionColor = Color.Red;
                        break;
                    default:
                        logs.SelectionColor = Color.Black;
                        break;
                }

                logs.AppendText("  " + alog);
                logs.AppendText(Environment.NewLine);
                logs.ScrollToCaret();

                string line = time_a + " " + alog;
                SaveToLogFile(line);
            }

        }

        private void SaveToLogFile(string line)
        {
            try
            {
                if (!File.Exists(txtWS.Text + "\\TBLog.log"))
                {
                    File.Create(txtWS.Text + "\\TBLog.log");
                }
                else
                {
                    File.AppendAllText(@txtWS.Text + "\\TBLog.log", line + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                //printLog("SaveToLogFile:"+ex.Message, "error");
            }
        }

        private void readLog()
        {
            try
            {
                if (File.Exists(txtWS.Text + "\\TBLog.log"))
                {
                    string rs = File.ReadAllText(txtWS.Text + "\\TBLog.log");
                    logs.Text = rs;
                    // set the current caret position to the end
                    logs.SelectionStart = logs.Text.Length;
                    // scroll it automatically
                    logs.ScrollToCaret();
                }
            }
            catch { }
        }

        private void initcomboBox()
        {
            if (TalkBackAutoTest.Properties.Settings.Default.txtPkg == "")
            {
                TalkBackAutoTest.Properties.Settings.Default.txtPkg = ALL_APPS;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            //stressTestPkg = txtPkg.Text = TalkBackAutoTest.Properties.Settings.Default.txtPkg;
            stressTestPkg = txtPkg.Text = TalkBackAutoTest.Properties.Settings.Default.txtPkg;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            getDeviceInformation();
            initProject();  
            updatecomboBoxPkg();
            initcomboBox();

                    
        }

        private string getPackageVersion(string packageName)
        {

            //kiem tra ton tai

            if (dictPkgVersion.ContainsKey(packageName) && dictPkgVersion[packageName].Trim() != "")
            {
                return dictPkgVersion[packageName].Trim();
            }
            try
            {
                if (device.serial != null)
                {
                    string command = "adb -s " + device.serial + " shell \"dumpsys  package " + packageName + " | grep versionName";
                    string output = RunCommand(command);
                    if (output == "")
                    {
                        return output;
                    }
                    else
                    {
                        string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Count() > 0 && lines[0].Contains("=") && lines[0].Contains("versionName"))
                        {
                            string[] rs = lines[0].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                            dictPkgVersion[packageName] = rs[1].Trim();
                            return dictPkgVersion[packageName];
                        }
                        else
                        {
                            return "";
                        }


                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                printLog("getPackageVersion:" + ex.Message, "error");
                return "";
            }
        }

        private string getCurrentScreen()
        {
            try
            {
                if (device.serial != null)
                {
                    //int recent = 0;
                    string command = "adb -s " + device.serial + " shell \"dumpsys activity activities | grep ResumedActivity";
                    string output = RunCommand(command);
                    if (output == "")
                    {
                        return output;
                    }
                    else
                    {
                        string[] rs = output.Split(new string[] { " " }, StringSplitOptions.None);
                        foreach (string s in rs)
                        {
                            if (s.Contains(@"/") && s.Contains("."))
                            {
                                //s = s.TrimEnd('}');
                                return s.TrimEnd('}');
                            }
                        }
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                printLog("getCurrentScreen:" + ex.Message, "error");
                return "";
            }
        }

        private void rbStressTestMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbStressTestMode.Checked == true)
            {
                TalkBackAutoTest.Properties.Settings.Default.modeTesting = 1;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
        }

        private void rbManualMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbManualMode.Checked == true)
            {
                TalkBackAutoTest.Properties.Settings.Default.modeTesting = 2;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
        }

        private void rbScriptMode_CheckedChanged(object sender, EventArgs e)
        {
            //if (rbScriptMode.Checked == true)
            //{
            //    //TalkBackAutoTest.Properties.Settings.Default.modeTesting = 3;
            //    //TalkBackAutoTest.Properties.Settings.Default.Save();
            //}
        }

        private void rbActivityMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbActivityMode.Checked == true)
            {
                TalkBackAutoTest.Properties.Settings.Default.modeTesting = 3;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
        }


        private void btnRefresh_VisibleChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("Test");

        }

        private int getDPI()
        {
            try
            {
                if (device.serial != null)
                {

                    if (GLOBAL_DPI != 0)
                    {
                        return GLOBAL_DPI;
                    }
                    else
                    {
                        string command = "adb -s " + device.serial + " shell wm density";
                        string output = RunCommand(command);
                        if (output == "")
                        {
                            return 0;
                        }
                        else
                        {
                            string[] lines = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Count() > 0 && lines[0].Contains("Physical density"))
                            {
                                string[] rs = lines[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                GLOBAL_DPI = Int16.Parse(rs[1].Trim());
                                return GLOBAL_DPI;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception ex)
            {
                printLog("getDPI:" + ex.Message, "error");
                return 0;
            }
        }

        private string getCurrentLocale()
        {
            if (device.serial == null)
            {
                GLOBAL_LANGUAGE = "";
                return "";
            }
            if (GLOBAL_LANGUAGE != "")
            {
                return GLOBAL_LANGUAGE;
            }
            try
            {
                if (device.serial == null)
                {
                    printLog("Need connect device first to check diff Lang Issue Type", "info");
                    return "";
                }
                else
                {
                    string output = RunCommand("adb -s " + device.serial + " shell \"getprop | grep persist.sys.locale\"");
                    if (output == "") return "NA";

                    string[] rs = output.Split(new string[] { " " }, StringSplitOptions.None);

                    GLOBAL_LANGUAGE = rs[1].Replace("[", "").Replace("]", "").Trim();
                    return GLOBAL_LANGUAGE;
                }

            }
            catch (Exception ex)
            {
                printLog("getCurrentLocale:" + ex.Message, "error");
                return "";
            }
        }

        private void resetGLOBAL_LANGUAGE()
        {
            GLOBAL_LANGUAGE = "";
        }
        private void resetGLOBAL_DPI()
        {
            GLOBAL_DPI = 0;
        }

        private void btnChangeLanguage_Click(object sender, EventArgs e)
        {
            if (cbLanguage.Text == "")
            {
                MessageBox.Show("Please choose language");
            }
            else
            {
                if (device.serial != null)
                {

                    if (device.type.Trim() == "user")
                    {
                        MessageBox.Show("Please using Eng device");
                        return;
                    }
                    string locale1 = "en-US";
                    string langlang1 = "en";
                    string country1 = "US";

                    if (cbLanguage.Text == "vi-VN")
                    {
                        locale1 = cbLanguage.Text;
                        langlang1 = "vi";
                        country1 = "VN";
                    }
                    if (cbLanguage.Text == "en-US")
                    {
                        locale1 = cbLanguage.Text;
                        langlang1 = "en";
                        country1 = "US";
                    }
                    if (cbLanguage.Text == "ko-KR")
                    {
                        locale1 = cbLanguage.Text;
                        langlang1 = "ko";
                        country1 = "KR";
                    }
                    //if (cbLanguage.Text == "en-US") langlang = "en";
                    //if (cbLanguage.Text == "ko-KR") langlang = "ko";

                    RunCommand("adb -s " + device.serial + " root && adb -s " + device.serial + " shell \"setprop persist.sys.locale " + cbLanguage.Text + "; sleep 1; settings put system system_locales " + cbLanguage.Text + "; sleep 1; setprop ctl.restart zygote;reboot\"");
                    //RunCommand("adb -s " + device.serial + " root && adb -s " + device.serial + " shell \"setprop persist.sys.locale " + locale1 + "; sleep 1;setprop persist.sys.language " + langlang1 + ";sleep 1;setprop persist.sys.country " + country1 + ";setprop ctl.restart zygote\" && adb -s " + device.serial + " reboot");
                    GLOBAL_LANGUAGE = "";


                    //if (cbLanguage.Text.Contains("en-")) GLOBAL_LANGUAGE = "en";
                    //if (cbLanguage.Text.Contains("vi-")) GLOBAL_LANGUAGE = "vi";
                    //if (cbLanguage.Text.Contains("ko-")) GLOBAL_LANGUAGE = "ko";

                    //GLOBAL_LANGUAGE = "en";
                    //resetGLOBAL_LANGUAGE();
                }
                else
                {
                    MessageBox.Show("Please connect device first");
                }
            }
        }

        private void openLogFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int p = lvResult.FocusedItem.Index;
                string objId = lvResult.Items[p].SubItems[1].Text.ToString();
                string tc_folder = txtWS.Text + "/Result/" + objId.ToLower();
                var f = Process.Start(tc_folder);
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog("openLogFolderToolStripMenuItem_Click:" + ex.Message, "error");
            }
        }

        private void generatePLMFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int p = lvResult.FocusedItem.Index;

                string sobjId = lvResult.Items[p].SubItems[0].Text.ToString();
                int objId = Int16.Parse(sobjId) - 1;

                if (listObject[objId].result == "Pass" && listObject[objId].errortype.Contains("NA"))
                {
                    MessageBox.Show("Not support Pass cases");
                }
                else
                {
                    generatePLMFormat(objId);
                }
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog("generatePLMFormatToolStripMenuItem_Click:" + ex.Message, "error");
            }
        }


        //private string getExpected(string errorType)
        //{
        //    if (errorType == "NAF")//NAF clickable/focusable=true but no text/description
        //    {
        //        return "Should add more text/description for object NAF: Not Friendly Accessibility. Need add more text/description";
        //    }
        //    if (errorType == "NAF_Focus")//NAF clickable/focusable=true but no text/description
        //    {
        //        return "Should add more text/description for object or change focusable=false for this object";
        //    }
        //    if (errorType == "UNLABELED")//tach nho casse unlabel/ wrong output, empty ouput...
        //    {
        //        return "Should add more text/description because Talkback read UNLABELED";
        //    }
        //    if (errorType == "NO_OUTPUT")//tach nho casse unlabel/ wrong output, empty ouput...
        //    {
        //        return "Should add more text/description because Talkback no read";
        //    }
        //    //if (errorType == "WRONG_OUPUT")//tach nho casse unlabel/ wrong output, empty ouput...
        //    //{
        //    //    return "Should add more text/description for object or change focusable=false for this object";
        //    //}
        //    if (errorType == "NO_FOCUSABLE")//co the click co thong tin nhung lai ko the focus
        //    {
        //        return "Should add focusable=true for this object, need analyze to prevent fail-alarm";
        //    }
        //    if (errorType == "DIFF_LANG")//co the click co thong tin nhung lai ko the focus
        //    {
        //        return "Should translate with current language";
        //    }

        //    if (errorType == "DUP_WORD")//dup word in tb
        //    {
        //        return "Should be not duplicate words in talkback output";
        //    }
        //    if (errorType == "NOT_CHANGE_LANG")//dup word in tb
        //    {
        //        return "Some words should be change language";
        //    }

        //    if (errorType == "NO_FOCUS_AFTER_TAB")
        //    {
        //        return "No any object focused, please check again";
        //    }

        //    return "Should check this object and correct between object text/description nad talkback output";
        //}


        //18022025
        private string getObjectX(Object o)
        {
            string info = UnescapeXml(o.objectInformation);

            XElement script = XElement.Parse(info);

            string allText = "";
            string allDes = "";
            string resId = "";


            string allTextDes_optimize = "";
            try
            {

                XDocument doc1 = XDocument.Parse(script.ToString());
                var childs = doc1.Descendants("node");

                foreach (XElement x in childs)
                {
                    if (resId == "")
                    {
                        resId = x.Attribute("resource-id").Value.ToString();
                    }

                    if (x.Attribute("text").Value.ToString() != "")
                    {
                        allText += x.Attribute("text").Value.ToString() + " ";
                    }
                    if (x.Attribute("content-desc").Value.ToString() != "")
                    {
                        allDes += x.Attribute("content-desc").Value.ToString() + " ";

                        allTextDes_optimize += x.Attribute("content-desc").Value.ToString() + " ";
                    }
                    else
                    {
                        if (x.Attribute("text").Value.ToString() != "")
                        {
                            allTextDes_optimize += x.Attribute("text").Value.ToString() + " ";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //info child
            }

            allText = allText.TrimEnd(' ');
            allDes = allDes.TrimEnd(' ');

            int MAX_LENGTH = 50;

            if (allText != "")
            {
                string optimizeText = allText.Length > MAX_LENGTH ? allText.Substring(0, MAX_LENGTH) + "..." : allText;
                return "'" + optimizeText + "'";
            }
            else if (allDes != "")
            {
                string optimizeDes = allDes.Length > MAX_LENGTH ? allDes.Substring(0, MAX_LENGTH) + "..." : allDes;
                return "'" + optimizeDes + "'";
            }
            else if (resId != "")
            {
                return "'" + resId + "'";
            }


            return "in screenshot";

        }
        private string getTillePLMByErrorType(Object o)
        {

            string ObjectX = getObjectX(o);

            DeviceInfo d1 = JsonConvert.DeserializeObject<DeviceInfo>(o.deviceInfo);

            string osName = d1.os == null ? "" : d1.os;
            string AppName = getAppNameFromPackageName(o.package);

            string title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Talkback problem at Object:" + ObjectX;
            if (o.errortype == "UNLABELLED")//vi ko
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' is read 'Unlabeled'";

            }
            else if (o.errortype == "NAF")//xml duy dang doc parent -> dac trung?
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' has no descriptions talkback output while it is focusable";
            }
            //else if (o.errortype == "NAF_FOCUS")//NAF_FOCUS dua tren xml
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Has not any description athough could focusable";
            //}

            else if (o.errortype == "NO_OUTPUT")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' has no descriptions talkback output while it is focusable";
            }
            else if (o.errortype == "DUP_WORD")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' is read words: '" + isDupWordNew(o.talkbackText) + "' repeating several times";
            }
            else if (o.errortype == "REPEAT_PREVIOUS_OBJECT")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' is read same as previous object";
            }
            //else if (o.errortype == "WRONG_OUPUT")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Talkback read content is different with Object UI Text/Description";
            //}
            else if (o.errortype == "NO_CHANGE_FOCUS_AFTER_TAB")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " There has no focus on any objects when using TAB key";
            }
            else if (o.errortype == "DIFF_LANG")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' does not change language";
            }

            //else if (o.errortype == "DIFF_TEXT_DES")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Not match content between Text and Description";//model AI?
            //}
            //else if (o.errortype == "DIFF_TB_UI")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Not match content between TalkBack and UI";//model AI?
            //}

            //comapre giua text va des
            //compare noi dung tren ui voi noi dung tb doc


            //another
            else if (o.errortype == "NAF" && o.talkbackText == "TalkbackText_NT")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' has no descriptions talkback output while it is focusable";
            }
            else if (o.errortype == "NO_FOCUSABLE_HEADER" && o.talkbackText == "TalkbackText_NT")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' should be focusable";
            }
            //else if (o.errortype == "NO_FOCUSABLE_HEADER")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": has text/description but not support focus when using TAB key";//define chinh xac chu nao
            //}
            else if (o.errortype == "DIFF_LANG" && o.talkbackText == "TalkbackText_NT")
            {
                title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' does not change language";
            }
            return title;
        }

        private string getBodyPLMByErrorType(Object o)
        {
            DeviceInfo d1 = JsonConvert.DeserializeObject<DeviceInfo>(o.deviceInfo);

            string ObjectX = getObjectX(o);

            string osName = d1.os == null ? "" : d1.os;
            string AppName = getAppNameFromPackageName(o.package);
            string folder_log = txtWS.Text + "//Result//" + o.focusedObject.ToLower();




            string TARGET = "[TARGET]" + "\r\n" +
                                    "- [Model Name]: [" + d1.modelName.Trim() + "]" + "\r\n" +
                                    "- [SW Version[: [" + d1.binaryName + "]" + "\r\n" +
                                    "- [OS] : [" + osName + "]" + "" + "\r\n" +
                                    "- [Test Item] : [Automation Test]" + "\r\n" +
                                    "- [Object Information]: [" + ObjectX + "]" + "\r\n";


            string PRECONDITION = "\r\n[PRECONDITION]" + "\r\n" +
                                    "- Using TalkbackAuto Test Tool  latest on ToolHub: https://mobilerndhub.sec.samsung.net/toolhub" + "\r\n" +
                                    "- Using TAB key on keyboard or command: 'adb shell input keyevent TAB' to focus on object" + "\r\n";


            string STEP = "\r\n[STEP]" + "\r\n" +
                                    "1. Run TalkbackAuto Tool with " + o.testingmode + " Mode\r\n" +
                                    "2. Go to Screen: " + o.screen + "\r\n" +
                                    "3. Focus on object: " + ObjectX + " by TAB key on keyboard\r\n";

            string OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: [" + o.errortype + "]" + " Talkback problem at Object:" + ObjectX;


            if (o.errortype == "UNLABELLED")//vi ko
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' is read 'Unlabeled'";

            }
            else if (o.errortype == "NAF")//xml duy dang doc parent -> dac trung?
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' has no descriptions talkback output while it is focusable";
            }
            //else if (o.errortype == "NAF_FOCUS")//NAF_FOCUS dua tren xml
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Has not any description athough could focusable";
            //}

            else if (o.errortype == "NO_OUTPUT")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' has no descriptions talkback output while it is focusable";
            }
            else if (o.errortype == "DUP_WORD")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' is read words: '" + isDupWordNew(o.talkbackText) + "' repeating several times";
            }
            else if (o.errortype == "REPEAT_PREVIOUS_OBJECT")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' is read same as previous object";
            }
            //else if (o.errortype == "WRONG_OUPUT")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Talkback read content is different with Object UI Text/Description";
            //}
            else if (o.errortype == "NO_CHANGE_FOCUS_AFTER_TAB")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " There has no focus on any objects when using TAB key";
            }
            else if (o.errortype == "DIFF_LANG")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' does not change language";
            }

            //else if (o.errortype == "DIFF_TEXT_DES")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Not match content between Text and Description";//model AI?
            //}
            //else if (o.errortype == "DIFF_TB_UI")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Not match content between TalkBack and UI";//model AI?
            //}

            //comapre giua text va des
            //compare noi dung tren ui voi noi dung tb doc


            //another
            else if (o.errortype == "NAF" && o.talkbackText == "TalkbackText_NT")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + "Object: '" + ObjectX + "' has no descriptions talkback output while it is focusable";
            }
            //else if (o.errortype == "NO_FOCUSABLE_HEADER")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": has text/description but not support focus when using TAB key";//define chinh xac chu nao
            //}
            else if (o.errortype == "DIFF_LANG" && o.talkbackText == "TalkbackText_NT")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' does not change language";
            }


            OBSERVE += "\r\n";

            //1 so loi them thong tin xml

            string EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Please fix it to Talkback could work more correctly" + "\r\n";

            if (o.errortype == "UNLABELLED")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Object: '" + ObjectX + "' should be added description" + "\r\n";
            }
            else if (o.errortype == "NAF")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Object: '" + ObjectX + "' should be added description/talkback output, if not, focus should be removed for it" + "\r\n";

                OBSERVE += "\r\n" + getFistNode(UnescapeXml(o.objectInformation)) + "\r\n\r\n";
            }
            else if (o.errortype == "NO_OUTPUT")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Object: '" + ObjectX + "' should be added description/talkback output, if not, focus should be removed for it" + "\r\n";

                OBSERVE += "\r\n" + getFistNode(UnescapeXml(o.objectInformation)) + "\r\n\r\n";
            }
            else if (o.errortype == "DUP_WORD")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "The dup word: '" + isDupWordNew(o.talkbackText) + "' should be removed" + "\r\n";
            }
            //else if (o.errortype == "REPEAT_PREVIOUS_OBJECT")
            else if (o.errortype == "REPEAT_PREVIOUS_OBJECT")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Description should be changed so that objects are not read same each other" + "\r\n";
            }
            //else if (o.errortype == "WRONG_OUPUT")
            //{
            //    title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + "" + o.objectInformation + ": Talkback read content is different with Object UI Text/Description";
            //}
            else if (o.errortype == "NO_CHANGE_FOCUS_AFTER_TAB")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "The next object should be focus and read objec after tab" + "\r\n";
            }
            else if (o.errortype == "DIFF_LANG")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Text should be translated following system language" + "\r\n";
            }

            //else if (o.errortype == "DIFF_TEXT_DES")
            //{
            //    OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
            //                "Error Type: [" + o.errortype + "] Not match content between Text and Description\r\n\r\n";

            //    EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
            //                       "Please change content description to match between Text and Description" + "\r\n";
            //}
            //else if (o.errortype == "DIFF_TB_UI")
            //{
            //    OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
            //                "Error Type: [" + o.errortype + "] Not match content between TalkBack and UI \r\n\r\n";

            //    EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
            //                       "Please change content description to match between TalkBackText and UI Text" + "\r\n";
            //}
            //comapre giua text va des
            //compare noi dung tren ui voi noi dung tb doc
            //another
            else if (o.errortype == "NAF" && o.talkbackText == "TalkbackText_NT")
            {

                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Object: '" + ObjectX + "' should be added description/talkback output, if not, focus should be removed for it" + "\r\n";

                OBSERVE += "\r\n\r\n" + getFistNode(UnescapeXml(o.objectInformation)) + "\r\n\r\n";
            }
            else if (o.errortype == "DIFF_LANG" && o.talkbackText == "TalkbackText_NT")
            {
                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Text should be translated following system language" + "\r\n";

            }

            else if (o.errortype == "NO_FOCUSABLE_HEADER")
            {
                OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                                    "Error Type: " + "[" + o.testingmode + "]" + "[" + o.errortype + "]" + " Object: '" + ObjectX + "' should be focusable";

                EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                                   "Text Header should be focusable" + "\r\n";
            }

            string NOTE = "\r\n[NOTE]" + "\r\n" +
                                    "Please refer attachments (Logcat file) and use TAB_KEY on keyboard (adb shell input keyevent TAB) to reproduce issue\r\n\r\n" +
                                    "**NOTE FOR TESTER**\r\n" +
                                    "(Tester need to upload forder Log at:" + folder_log + ")";

            string body = TARGET +

                          PRECONDITION +

                          STEP +

                          OBSERVE +

                          EXPECTED +

                          NOTE;

            return body;
        }
        //end 18022025

        private string getFistNode(string Nodexml)
        {
            XDocument doc = XDocument.Parse(Nodexml);
            var Scripts = doc.Descendants("node");
            if (Scripts.Count() > 0)
            {
                foreach (XElement script in Scripts)
                {
                    string focused = script.Attribute("focused").Value.ToString();
                    if (focused == "true")
                    {
                        string s = script.ToString();
                        string output = "";
                        int idx = 0;
                        while (idx < s.Length)
                        {

                            if (idx + 4 < s.Length && idx > 10 && s[idx] == '<' && s[idx + 1] == 'n' && s[idx + 2] == 'o' && s[idx + 3] == 'd' && s[idx + 4] == 'e')
                            {
                                break;
                            }
                            output += s[idx];
                            idx++;
                        }
                        return output.Trim();
                    }
                }
            }

            return Nodexml;
        }

        private void generatePLMFormat(int index)
        {
            try
            {
                Object o = listObject[index];

                //string folder_log = txtWS.Text + "//Result//" + o.focusedObject.ToLower();
                //string title = "[TalkBackAutoTest][" + AppName + "]" + "[" + o.pkgVersion + "]" + "[Mode:" + o.testingmode + "]" + "[Error Type:" + o.errortype + "]" + " Talkback problem at Object:" + o.objectInformation;
                string title = getTillePLMByErrorType(o);
                string body = getBodyPLMByErrorType(o);

                SetValueForTitle = title;
                SetValueForBody = body;
                //string TARGET = "[TARGET]" + "\r\n" +
                //                        "- [Model Name]: [" + d1.modelName.Trim() + "]" + "\r\n" +
                //                        "- [SW Version[: [" + d1.binaryName + "]" + "\r\n" +
                //                        "- [OS] : [" + osName + "]" + "" + "\r\n" +
                //                        "- [PO Name] : [" + AppName + "]" + "" + "\r\n" +
                //                        "- [Testing Screen Mode] : [" + o.testingmode + "]" + "" + "\r\n" +
                //                        "- [Package Name]: [" + o.package + "]" + "\r\n" +
                //                        "- [App Version Name]: [" + o.pkgVersion + "]" + "\r\n" +
                //                        "- [Object Information]: [" + o.objectInformation + "]" + "\r\n";


                //string PRECONDITION = "\r\n[PRECONDITION]" + "\r\n" +
                //                        "- Using TalkbackAuto Test Tool  latest on ToolHub: https://mobilerndhub.sec.samsung.net/toolhub" + "\r\n";

                //string STEP = "\r\n[STEP]" + "\r\n" +
                //                        "1. Run TalkbackAuto Tool > Select package " + o.package + " > Choose Testing Mode:" + o.testingmode + " > Run" + "\r\n" +
                //                        "1.1 Get Windown dump and screenshot of Screen\r\n" +
                //                        "1.2 Tab to focus to Focusable object\r\n" +
                //                        "1.3 Check Focused object and compare with Talkback Text/sound ouput\r\n";
                //string OBSERVE = "\r\n[OBSERVE]" + "\r\n" +
                //                        "Talkback Problem with Error Type: [" + o.errortype + "]\r\n\r\n";
                //string EXPECTED = "\r\n[EXPECTED]" + "\r\n" +
                //                        getExpected(o.errortype) + "\r\n";

                //string NOTE = "\r\n[NOTE]" + "\r\n" +
                //                        "Please refer attachments (Logcat file)\r\n\r\n" +
                //                        "**NOTE FOR TESTER**\r\n" +
                //                        "(Tester need to upload forder Log at:" + folder_log + ")";

                //SetValueForBody = TARGET +

                //                  PRECONDITION +

                //                  STEP +

                //                  OBSERVE +

                //                  EXPECTED +

                //                  NOTE;

                PLMForm frm2 = new PLMForm();
                frm2.StartPosition = FormStartPosition.CenterParent;
                frm2.ShowDialog();
                // MessageBox.Show("Ongoing");
            }
            catch (Exception ex)
            {
                printLog("generatePLMFormat:" + ex.Message, "error");
            }
        }

        private void cleanLogcat()
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell logcat -c & adb -s " + device.serial + " shell logcat -c");
            }
        }

        private void getlogcat(string pathName, int numberOfEvent)
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell logcat -v threadtime -t " + numberOfEvent + " >" + pathName);
            }
        }

        private void btnChooseTMFolder_Click(object sender, EventArgs e)
        {

            try
            {
                BetterFolderBrowser f = new BetterFolderBrowser();
                f.Title = "Pick Script Folder";
                f.Multiselect = false;
                if (f.ShowDialog() == DialogResult.OK)
                {

                    string driveName = Path.GetPathRoot(f.SelectedFolder);
                    DriveInfo drive = new DriveInfo(driveName);

                    if (f.SelectedFolder.Contains(" "))
                    {
                        MessageBox.Show("Have Whitespace Character in Path. Please choose again without Whitespace Character in Path");
                    }
                    else if (Path.GetPathRoot(f.SelectedFolder) == f.SelectedFolder)
                    {
                        MessageBox.Show("Please choose TN Script folder with sub folder, not only letter such as C:\\, D:\\ ...!");
                    }
                    else
                    {
                        //txt_choose_save_folder.Text = fbd.FileName.TrimEnd('\\') + "\\";
                        //txtScriptFolder.Text = f.SelectedFolder;

                        //setas_path = "\"" + setas_path + "\"";
                        //TalkBackAutoTest.Properties.Settings.Default.txtScriptFolder = txtScriptFolder.Text;
                        //TalkBackAutoTest.Properties.Settings.Default.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("btnChooseTMFolder_Click: " + ex.Message);
            }
        }

        private void btnOpenWs_Click(object sender, EventArgs e)
        {
            try
            {
                string wsFolder = txtWS.Text;
                var f = Process.Start(wsFolder);
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog("btnOpenWs_Click:" + ex.Message, "error");
            }
        }


        private void txtEventNumberMax1screen_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.txtEventNumberMax1screen = txtEventNumberMax1screen.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtEventNumberMax1screen_TextChanged: " + ex.Message, "error");
            }
        }

        private void txtEventNumberMaxAll_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.txtEventNumberMaxAll = txtEventNumberMaxAll.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtEventNumberMaxAll_TextChanged: " + ex.Message, "error");
            }
        }

        private void cbDisableSysKey_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDisableSysKey.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.cbDisableSysKey = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.cbDisableSysKey = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cbDisableSysKey_CheckedChanged: " + ex.Message, "error");
            }
        }

        private bool currentScreenContainAnyPackage(string currentScreen)
        {
            if (listAppName.Count() == 0)
            {
                listAppName = getListAppName();
            }
            //find app name
            foreach (AppName x in listAppName)
            {
                if (currentScreen.Contains(x.pkgName))
                {
                    return true;
                }
            }
            return false;
        }


        private string getAppNameFromPackageName(string packageName)
        {

            if (listAppName.Count() == 0)
            {
                listAppName = getListAppName();
            }
            //find app name
            foreach (AppName x in listAppName)
            {
                if (x.pkgName == packageName)
                {
                    return x.appName;
                }
            }
            return packageName;
        }


        private void viewScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int p = lvResult.FocusedItem.Index;
                string objId = lvResult.Items[p].SubItems[1].Text.ToString();
                string imageFile0 = txtWS.Text + "/Result/" + objId.ToLower() + "/" + objId.ToLower() + "_canvas.png";
                string imageFile = txtWS.Text + "/Result/" + objId.ToLower() + "/more_details/" + objId.ToLower() + ".png";
                string imageFile1 = txtWS.Text + "/Result/" + objId.ToLower() + "/" + objId.ToLower() + ".png";
                if (File.Exists(imageFile0))
                {
                    var f = Process.Start(imageFile0);
                }
                else if (File.Exists(imageFile))
                {
                    var f = Process.Start(imageFile);
                }
                else if (File.Exists(imageFile1))
                {
                    var f = Process.Start(imageFile1);
                }
                else
                {
                    printLog("Not existed File", "error");
                }
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog("viewScreenshotToolStripMenuItem_Click:" + ex.Message, "error");
            }
        }

        private void viewLogcatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int p = lvResult.FocusedItem.Index;
                string objId = lvResult.Items[p].SubItems[1].Text.ToString();
                string logFile = txtWS.Text + "/Result/" + objId.ToLower() + "/" + objId.ToLower() + ".log";
                var f = Process.Start(logFile);
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog("viewLogcatToolStripMenuItem_Click:" + ex.Message, "error");
            }
        }


        private void ExportExcel(string xmlPath)
        {
            try
            {
                doc = XDocument.Load(@xmlPath);
                var Scripts = doc.Descendants("Script");
                int elementCount = doc.Root.Elements().Count();
                int row_count = 0;
                string[,] str_kq = new string[elementCount + 1, 13];

                for (int i = 0; i < elementCount; i++)
                {
                    XElement script = Scripts.ElementAt(i);
                    string FocusedObject = script.Attribute("FocusedObject").Value;
                    string Screen = script.Attribute("Screen").Value;
                    string Package = script.Attribute("Package").Value;
                    string PkgVersion = script.Attribute("PkgVersion").Value;

                    string ObjectInformation = script.Attribute("ObjectInformation").Value;
                    string TalkbackText = script.Attribute("TalkbackText").Value;
                    string Result = script.Attribute("Result").Value;

                    string TestingTime = script.Attribute("TestingTime") != null ? script.Attribute("TestingTime").Value : "NA";
                    string TestingMode = script.Attribute("TestingMode") != null ? script.Attribute("TestingMode").Value : "NA";
                    string No = script.Attribute("No") != null ? script.Attribute("No").Value : (i + 1).ToString();
                    string Remark = script.Attribute("Remark") != null ? script.Attribute("Remark").Value : "NA";
                    string DeviceInf = script.Attribute("DeviceInfo") != null ? script.Attribute("DeviceInfo").Value : "";
                    string ErrorType = script.Attribute("ErrorType") != null ? script.Attribute("ErrorType").Value : "NA";

                    str_kq[row_count, 0] = No.ToString();
                    str_kq[row_count, 1] = FocusedObject;
                    str_kq[row_count, 2] = ErrorType;
                    str_kq[row_count, 3] = TalkbackText;
                    str_kq[row_count, 4] = Remark;
                    str_kq[row_count, 5] = Screen;
                    str_kq[row_count, 6] = Package;
                    str_kq[row_count, 7] = PkgVersion;


                    str_kq[row_count, 8] = Result;
                    str_kq[row_count, 9] = TestingTime;
                    str_kq[row_count, 10] = TestingMode;
                    //str_kq[row_count, 11] = ObjectInformation;
                    str_kq[row_count, 11] = "Please check oin XML file";

                    str_kq[row_count, 12] = DeviceInf;
                    row_count++;
                }

                // khởi tạo wb rỗng
                #region style
                XSSFWorkbook wb = new XSSFWorkbook();

                // Tạo ra 1 sheet
                ISheet sheet = wb.CreateSheet();
                wb.SetSheetName(0, "TalkbackTesting");
                //config style
                NPOI.SS.UserModel.IFont boldFont = wb.CreateFont();
                boldFont.Boldweight = (short)FontBoldWeight.Bold;
                boldFont.Color = IndexedColors.White.Index;

                //head
                ICellStyle HeadStyle = (ICellStyle)wb.CreateCellStyle();
                HeadStyle.WrapText = true;
                HeadStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                HeadStyle.VerticalAlignment = VerticalAlignment.Top;
                HeadStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
                HeadStyle.FillPattern = FillPattern.SolidForeground;
                HeadStyle.SetFont(boldFont);

                //CONSIDER Sstyle
                //byte[] rgb = new byte[3] { 255, 184, 77 };//#FFB84D
                //XSSFColor color = new XSSFColor(rgb);

                ICellStyle CONSIDERStyle = (ICellStyle)wb.CreateCellStyle();
                CONSIDERStyle.WrapText = false;
                CONSIDERStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                CONSIDERStyle.VerticalAlignment = VerticalAlignment.Top;
                CONSIDERStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                CONSIDERStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                CONSIDERStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                CONSIDERStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                //CONSIDERStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;dsds
                CONSIDERStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Orange.Index;
                CONSIDERStyle.FillPattern = FillPattern.SolidForeground;

                //Pass Style
                ICellStyle PassStyle = (ICellStyle)wb.CreateCellStyle();
                PassStyle.WrapText = false;
                PassStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                PassStyle.VerticalAlignment = VerticalAlignment.Top;
                PassStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                PassStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                PassStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                PassStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                PassStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
                PassStyle.FillPattern = FillPattern.SolidForeground;
                //Fail Style
                ICellStyle FailStyle = (ICellStyle)wb.CreateCellStyle();
                FailStyle.WrapText = false;
                FailStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                FailStyle.VerticalAlignment = VerticalAlignment.Top;
                FailStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                FailStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                FailStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                FailStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                FailStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
                FailStyle.FillPattern = FillPattern.SolidForeground;
                //NA
                ICellStyle NAStyle = (ICellStyle)wb.CreateCellStyle();
                NAStyle.WrapText = false;
                NAStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                NAStyle.VerticalAlignment = VerticalAlignment.Top;
                NAStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                NAStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                NAStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                NAStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                NAStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightOrange.Index;
                NAStyle.FillPattern = FillPattern.SolidForeground;
                //NT
                ICellStyle NTStyle = (ICellStyle)wb.CreateCellStyle();
                NTStyle.WrapText = false;
                NTStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                NTStyle.VerticalAlignment = VerticalAlignment.Top;
                NTStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                NTStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                NTStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                NTStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                NTStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                NTStyle.FillPattern = FillPattern.SolidForeground;
                //RUNNING
                ICellStyle RUNNINGStyle = (ICellStyle)wb.CreateCellStyle();
                RUNNINGStyle.WrapText = false;
                RUNNINGStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                RUNNINGStyle.VerticalAlignment = VerticalAlignment.Top;
                RUNNINGStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                RUNNINGStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                RUNNINGStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                RUNNINGStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                RUNNINGStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
                RUNNINGStyle.FillPattern = FillPattern.SolidForeground;
                //default
                ICellStyle defaultStyle = (ICellStyle)wb.CreateCellStyle();
                defaultStyle.WrapText = false;
                defaultStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                defaultStyle.VerticalAlignment = VerticalAlignment.Top;
                defaultStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                defaultStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                defaultStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                defaultStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                #endregion style
                //
                // Ghi tên cột ở row 1
                String[] data_head = { "No", "FocusedObject", "ErrorType", "TalkbackText", "XML & UI Text", "Screen", "Package", "PkgVersion", "Result", "TestingTime", "TestingMode", "ObjectInformation", "DeviceInf" };
                int[] data_head_width = { 10, 30, 30, 100, 100, 100, 100, 50, 30, 70, 50, 100, 100 };
                var row0 = sheet.CreateRow(0);
                row0.Height += 100;
                for (int i1 = 0; i1 < data_head_width.Length; i1++)
                {
                    row0.CreateCell(i1).SetCellValue(data_head[i1]);
                    row0.Cells[i1].CellStyle = HeadStyle;
                    sheet.SetColumnWidth(i1, data_head_width[i1] * 100);
                }

                // bắt đầu duyệt mảng và ghi tiếp tục
                for (int i = 0; i < row_count; i++)
                {
                    var newRow = sheet.CreateRow(i + 1);
                    // set giá trị
                    for (int j = 0; j < data_head.Length; j++)
                    {

                        str_kq[i, j] = str_kq[i, j].Length > 10000 ? str_kq[i, j].Substring(0, 10000) + "..." : str_kq[i, j];
                        newRow.CreateCell(j).SetCellValue(str_kq[i, j]);
                        newRow.Cells[j].CellStyle = defaultStyle;

                        if (j == 8)
                        {
                            if (str_kq[i, j].ToUpper() == "PASS")
                            {
                                newRow.Cells[j].CellStyle = PassStyle;
                            }
                            else if (str_kq[i, j].ToUpper() == "FAIL")
                            {
                                newRow.Cells[j].CellStyle = FailStyle;
                            }
                            else if (str_kq[i, j].ToUpper() == "NA")
                            {
                                newRow.Cells[j].CellStyle = NAStyle; ;
                            }
                            else if (str_kq[i, j].ToUpper() == "NT")
                            {
                                newRow.Cells[j].CellStyle = NTStyle;
                            }
                            else if (str_kq[i, j].ToUpper() == "RUNNING")
                            {
                                newRow.Cells[j].CellStyle = RUNNINGStyle;
                            }
                            else if (str_kq[i, j].ToUpper() == "CONSIDER")
                            {
                                newRow.Cells[j].CellStyle = CONSIDERStyle;
                            }
                        }
                    }
                }

                // xong hết thì save file lại

                string pathFile = txtWS.Text + "//TB_Report.xlsx";
                if (File.Exists(@pathFile))
                {
                    File.Delete(@pathFile);
                }

                FileStream fs = new FileStream(@pathFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                wb.Write(fs);

                //Notify
                DialogResult dialogResult = MessageBox.Show("Export Successful at:" + @pathFile + "\nDo you want to view this report?", "Export Successful", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start(@pathFile);
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }

            }
            catch (Exception ex)
            {
                printLog("ExportExcel:" + ex.Message, "error");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            isDupWordNew("xin chào các bạn");
            //RemoveBlackListRepeat("thông báo album chia sẻ");
            //RemoveWholeBlackList("Gallery ai la 1 cai gi");
            //cropTBOptimize(@"D:\F_Folder\2025\TalkBack\TB_1504\Result\obj1",@"D:\F_Folder\2025\TalkBack\TB_1504\Result\obj1\1_obj1.png", @"D:\F_Folder\2025\TalkBack\TB_1504\Result\obj1\1_croptb_obj1.png", 1);
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\TBOCR_0904\Result", "screen1.png");
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\TBOCR_0904\Result", "screen2.png");
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\TBOCR_0904\Result", "screen3.png");
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\TBOCR_0904\Result", "screen4.png");
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\TBOCR_0904\Result", "screen5.png");

            //screenShot(@"D:\F_Folder\2025\TalkBack\TBOCR_0904", "Result");
            //SamePrevious("Nút,", "100 phần trăm pin,");
            //copyToMoreDetailFolder(@"D:\F_Folder\2025\TalkBack\TB_OCR_3103_Nga\Result\obj29","29");
            //SamePrevious("Pattern", "Pattern, Button");
            //string[] engWords1 = new string[1000];
            //engWords1[0] = "a";
            //engWords1[1] = "b";
            //int k = 5;
            //getAnotherObjectsFromStartEnd(0, 13);
            //ischangelang("Chào mừng bạn đến với TalkBack phiên bản mới_", "vi");
            //inBlackListUnlabelled("a");
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\Release", "obj1.png");
            //string uiFocusText = getTextOCR("en", @"D:\F_Folder\2025\TalkBack\TB_2803_1\Result\obj24\croptb_obj24.png", @"D:\F_Folder\2025\TalkBack\TB_2803_1\Result\obj24\no_toast.txt");
            //int a = 5;
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\TB_2803_1\Result", "obj1.png");
            //getCurrentScreen();
            //bool rs = isRepeatWordsStartEnd("Unlike,12 likes,unlike");
            //bool rs1 = isRepeatWordsStartEnd("Unlike, 12 likes,unlike");
            //bool rs2 = isRepeatWordsStartEnd("Unlike, 12 likes, unlike");
            //int a = 5;
            //string kq = getDiffLang("SP của người việt");
            //MessageBox.Show(kq);

            //TBHelper.checkEng();

            //cropTBOptimize(@"D:\F_Folder\2025\TalkBack\TBOCR10\Result\obj3\obj3.png", @"D:\F_Folder\2025\TalkBack\TBOCR10\Result\obj3\obj3_cropauto.png");
            //drawOnImage(@"D:\F_Folder\2025\TalkBack\TBOCR16_stresstest\Result\obj1\obj1.png", @"D:\F_Folder\2025\TalkBack\TBOCR16_stresstest\Result\obj1\obj1_canvas.png");
            //getAnotherObjectsFromStartEnd(40, 50);

            //string bounds1 = "[0,249][1080,1188]";
            //string[] array_bounds = bounds1.Split(new string[] { ",", "]", "[" }, StringSplitOptions.None);
            //int a = 5;


            //string ouput = getTextOCR("en", @"D:\F_Folder\2025\TalkBack\TBOCR6\Result\obj1\croptb_obj1.png");
            //int a = 5;
            //logs.SelectionStart = logs.Text.Length;
            //logs.ScrollToCaret();

            //string output = getTextOCR(@"D:\F_Folder\2025\AI_2025\demo2.jpg");
            //MessageBox.Show(output);

            //croptoSquare(@"D:\F_Folder\2025\TalkBack\Test1\Result\obj5", "obj5.png");
            // int a = 5;

            // Example use:
            //CropTalkBackImage(@"D:\F_Folder\2025\TalkBack\Test1\Result\obj5", "obj5.png");
            //string output2 = getTextOCR(@"D:\F_Folder\2025\TalkBack\TBOCR\Result\obj1\croptb_obj1.png");
            //string output1 = "Manage folders, Button  &lt;";
            //string output = Regex.Replace(output1, @"\s{2,}&lt;", "");
            //int a = 5;

        }

        //private void croptFocusImage(string folderpath, string sourceFile, int x, int y, int w, int h)
        //{
        //    try
        //    {
        //        Bitmap source = new Bitmap(@folderpath + "//" + sourceFile);
        //        Rectangle section = new Rectangle(new Point(x, y), new Size(w, h));

        //        Bitmap CroppedImage = CropImage(source, section);
        //        CroppedImage.Save(@folderpath + "//cropfocus_" + sourceFile, ImageFormat.Jpeg);
        //        //int a = 5;
        //    }
        //    catch (Exception ex)
        //    {
        //        printLog("croptFocusImage:" + ex.Message, "error");
        //    }

        //}

        private void CropTalkBackImage(string folderpath, string sourceFile, int it)
        {
            try
            {
                //CropTalkBackImage(folderResult, "obj" + fileObject + ".png");
                cropTBOptimize(@folderpath, @folderpath + "/" + sourceFile, @folderpath + "/croptb_" + sourceFile, it);
                // Example use:     
                //Bitmap source = new Bitmap(@folderpath +"//"+sourceFile);
                //Rectangle section = new Rectangle(new Point(0, source.Size.Height - 150), new Size(source.Size.Width, 150));

                //Bitmap CroppedImage = CropImage(source, section);
                //CroppedImage.Save(@folderpath + "//croptb_" + sourceFile, ImageFormat.Jpeg);
                //int a = 5;
            }
            catch (Exception ex)
            {
                printLog("CropTalkBackImage:" + ex.Message, "error");
            }

        }

        private bool isBlackCamera(Color p)
        {
            if (p.R == 0 && p.G == 0 && p.B == 0)
            {
                return true;
            }
            return false;
        }
        private string getTextOCRMultipleFiles(string language, string folderPath, string fileObject)
        {
            try
            {
                if (File.Exists(@PY_OCR_PATH))
                {
                    string tbContent = "";

                    int previous_width = 0;
                    int previous_height = 0;
                    string previous_content = "";
                    for (int it = 1; it <= MAX_SCREENSHOT; it++)
                    {
                        string NoToastPath = @folderPath + "/" + it + "_no_toast.txt";
                        string imageCropPath = @folderPath + "/croptb_" + it + "_obj" + fileObject + ".png";
                        string imagePath = @folderPath + "/" + it + "_obj" + fileObject + ".png";
                        if (File.Exists(@NoToastPath))//No Toast case
                        {
                            continue;
                        }
                        if (!File.Exists(imageCropPath))
                        {
                            continue;
                        }

                        //Bitmap mypic = new Bitmap(@imageCropPath);
                        Image image;
                        using (Stream stream = File.OpenRead(@imageCropPath))
                        {
                            image = System.Drawing.Image.FromStream(stream);
                        }
                        Bitmap mypic = new Bitmap(image);


                        Image image1;
                        using (Stream stream = File.OpenRead(imagePath))
                        {
                            image1 = System.Drawing.Image.FromStream(stream);
                        }
                        Bitmap mypic1 = new Bitmap(image);

                        int width = mypic.Width;
                        int height = mypic.Height;

                        bool blackCamera = isBlackCamera(mypic.GetPixel(0, 0));
                        if (blackCamera == true || mypic.Width == mypic1.Width)
                        {
                            //nothing
                            //continue;
                        }
                        else if (width == previous_width && height == previous_height)
                        {
                            continue;
                        }

                        string command = "python \"" + @PY_OCR_PATH + "\" -i \"" + @imageCropPath + "\" -l \"" + language + "\" -p \"" + AI_MODEL_PATH + "\"";
                        string output = RunCommandOCR(command, 4000);

                        output = output.TrimEnd(' ', '>', ')');

                        if (output == "0" || output == "o" || output == "O" || output == "t")
                        {
                            printLog("OCR_OUPUT:" + output);
                            continue;
                        }

                        if (output != "")
                        {

                            if (blackCamera == true || mypic.Width == mypic1.Width)
                            {
                                if (EqualsIgnoreDiacritics(output, previous_content) == true)
                                {
                                    continue;//nothing
                                }
                            }

                            previous_width = width;
                            previous_height = height;
                            previous_content = output;
                            tbContent += ", " + output;

                            if (output.Contains(",,") || output.StartsWith(", "))
                            {
                                HAVECOMMA2 = true;
                            }
                        }

                    }

                    tbContent = tbContent.TrimStart(' ', ',');
                    return tbContent;
                }
                else
                {
                    return TRYCATCH_NA;
                }
            }
            catch (Exception ex)
            {
                printLog("getTextOCR5Files:" + ex.Message, "error");
                return TRYCATCH_NA;
            }
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private bool EqualsIgnoreDiacritics(string str1, string str2)
        {
            return string.Equals(
                RemoveDiacritics(str1).ToLowerInvariant(),
                RemoveDiacritics(str2).ToLowerInvariant()
            );
        }

        private string getTextOCR(string language, string imagePath, string NoToastPath)
        {
            return "";
            try
            {
                if (File.Exists(@PY_OCR_PATH))
                {
                    //en-US
                    //vi-VN
                    //ko-KR
                    //nghien cuu
                    if (File.Exists(@NoToastPath))//No Toast case
                    {
                        return "";
                    }

                    string command = "python \"" + @PY_OCR_PATH + "\" -i \"" + @imagePath + "\" -l \"" + language + "\" -p \"" + AI_MODEL_PATH + "\"";
                    string output = RunCommandOCR(command, 4000);
                    if ((output == "0" || output == "o" || output == "O" || output == "t") && File.Exists(@NoToastPath))
                    {
                        return "";
                    }

                    //nghien cuu
                    if (File.Exists(@NoToastPath))//No Toast case
                    {
                        return "";
                    }
                    return output;
                }
                else
                {
                    return TRYCATCH_NA;
                }
            }
            catch (Exception ex)
            {
                printLog("getTextOCR:" + ex.Message, "error");
                return TRYCATCH_NA;
            }
        }


        //hook proc
        protected override void WndProc(ref Message m)
        {

            try
            {
                base.WndProc(ref m);
                if (m.Msg == UsbNotification.WmDevicechange)
                {
                    switch ((int)m.WParam)
                    {
                        case UsbNotification.DbtDeviceremovecomplete:
                            getDeviceInformation();
                            break;
                        case UsbNotification.DbtDevicearrival:
                            getDeviceInformation();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                printLog("WndProc:" + ex.Message, "error");
            }

        }
        //end hook

        //draw on image
        private void drawOnImage(string imagePath, string imageNewPath, int x, int y, int w, int h)
        {
            //return;
            Image image = null;
            try
            {

                using (Stream stream = File.OpenRead(imagePath))
                {
                    image = System.Drawing.Image.FromStream(stream);
                }


                //Image image = Image.FromFile(imagePath);
                using (Graphics g = Graphics.FromImage(image))
                {
                    // Modify the image using g here... 
                    // Create a brush with an alpha value and use the g.FillRectangle function
                    //Color customColor = Color.FromArgb(100, Color.Red);
                    //SolidBrush shadowBrush = new SolidBrush(customColor);
                    //RectangleF rectFToFill = new RectangleF(200, 200, 200, 200);
                    // g.DrawRectangle(shadowBrush, new RectangleF[] { rectFToFill });
                    Pen redPen = new Pen(Color.Red, 10);
                    g.DrawRectangle(redPen, x, y, w, h);

                    using (Font arialFont = new Font("Arial", 30))
                    {
                        int newX = x + 20, newY = y + 20;
                        if (y - 40 > 10)
                        {
                            newY = y - 40;
                        }
                        else if (y + h + 40 < image.Height)
                        {
                            newY = y + h + 40;
                        }

                        if (x - 40 > 10)
                        {
                            newX = x - 40;
                        }
                        else if (x + w + 40 < image.Width)
                        {
                            newX = x + w + 40;
                        }

                        g.DrawString("Please check red object", arialFont, Brushes.Red, newX, newY);


                        //if (y - 40 > 10)
                        //{
                        //    g.DrawString("Please check red object", arialFont, Brushes.Red, x, y - 40);
                        //}
                        //else if (y + h + 40 < image.Height)
                        //{
                        //    g.DrawString("Please check red object", arialFont, Brushes.Red, x, y + h + 40);
                        //}
                        //else
                        //{
                        //    g.DrawString("Please check red object", arialFont, Brushes.Red, x + 20, y + 20);
                        //}
                    }
                }
                image.Save(imageNewPath);
                image.Dispose();
            }
            catch (Exception ex)
            {
                printLog("drawOnImage:" + ex.Message, "error");
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        private bool isInRange(int x, int min, int max)
        {
            if (x >= min && x <= max) return true;
            return false;
        }

        private bool isBlackLauncher(Color p, int min, int max)
        {
            if (p.R >= min && p.R <= max && p.G >= min && p.G <= max && p.B >= min && p.B <= max) return true;
            return false;
        }

        private void cropTBOptimize(string folderPath, string imagePath, string cropImagePath, int it)
        {
            string fileNoToast = folderPath + "/" + it + "_no_toast.txt";//ok
            //gfhgfh
            Image image;

            using (Stream stream = File.OpenRead(@imagePath))
            {
                image = System.Drawing.Image.FromStream(stream);
            }
            Bitmap mypic = new Bitmap(image);
            int imwid = mypic.Width;
            int imhei = mypic.Height;

            int x = 0, y = 0, w = 0, h = 0;
            //int min = 76, max = 88;
            //int min = 68, max = 88;
            int min = 0, max = 93;

            bool haveFirstPoint = false;//camera case

            bool haveFirstLancherPoint = false;//launcher case

            int OFFSET_TOP = 80;
            int OFSET_Z = 100;
            int OFSET_ZRight1 = 10;
            int OFSET_ZRight2 = 20;
            int OFSET_ZRight3 = 100;
            int BASE_OFSET_W = 40;
            int BASE_OFSET_H = 40;
            int CUT_TOP = 120;
            int deviceDPI = getDPI();

            if (deviceDPI >= 400)
            {
                OFFSET_TOP = 80;
                OFSET_Z = 100;
                OFSET_ZRight1 = 10;
                OFSET_ZRight2 = 20;
                OFSET_ZRight3 = 100;
                BASE_OFSET_W = 40;
                BASE_OFSET_H = 40;
                CUT_TOP = 120;
            }
            else if (deviceDPI < 400 && deviceDPI >= 300)
            {
                OFFSET_TOP = 50;
                OFSET_Z = 50;
                OFSET_ZRight1 = 10;
                OFSET_ZRight2 = 20;
                OFSET_ZRight3 = 80;
                BASE_OFSET_W = 40;
                BASE_OFSET_H = 40;
                CUT_TOP = 100;
            }


            //for (int z = imhei - 1; z >= 0; z--)
            for (int z = imhei - 1; z >= imhei / 2; z--)
            {
                bool continueZ = false;
                for (int i = 0; i < imwid / 2; i++)
                {
                    Color pixelColor = mypic.GetPixel(i, z);
                    Color pixelColorTop = mypic.GetPixel(i, z - OFFSET_TOP);//needcheck du de <toast và lon hon app icon
                    //if (z >= imhei - 150 && (isInRange(pixelColor.R, min, max) && isInRange(pixelColor.G, min, max) && isInRange(pixelColor.B, min, max)) || (pixelColor.R == 200 && pixelColor.G == 25 && pixelColor.B == 21))
                    if (z >= imhei - OFSET_Z && (isInRange(pixelColor.R, min, max) && isInRange(pixelColor.G, min, max) && isInRange(pixelColor.B, min, max) && isInRange(pixelColorTop.R, min, max) && isInRange(pixelColorTop.G, min, max) && isInRange(pixelColorTop.B, min, max)) || (pixelColor.R == 200 && pixelColor.G == 25 && pixelColor.B == 21))
                    {
                        //int a = 5;
                        //khi co diem i,z roi tim 3 diem con lai theo duong che
                        //bottom, left

                        //case camrera
                        //if (pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0 && mypic.GetPixel(0, 0).R == 0 && mypic.GetPixel(0, 0).G == 0 && mypic.GetPixel(0, 0).B == 0)
                        //if (isBlackCamera(pixelColor) && isBlackCamera(mypic.GetPixel(0, 0)))
                        if (isBlackCamera(pixelColor) && z - OFSET_ZRight1 >= 0 && isBlackCamera(mypic.GetPixel(0, z - OFSET_ZRight1)) && z - OFSET_ZRight2 >= 0 && isBlackCamera(mypic.GetPixel(0, z - OFSET_ZRight2)))
                        {
                            haveFirstPoint = true;
                        }

                        if (isBlackLauncher(pixelColor, min, max) && z - OFSET_ZRight1 >= 0 && isBlackLauncher(mypic.GetPixel(0, z - OFSET_ZRight1), min, max) && z - OFSET_ZRight2 >= 0 && isBlackLauncher(mypic.GetPixel(0, z - OFSET_ZRight2), min, max))
                        {
                            haveFirstLancherPoint = true;
                        }
                        int c = 6;
                        //for (int z1 = z; z1 >= imhei - 150; z1--)
                        for (int z1 = z; z1 >= imhei / 2; z1--)
                        {
                            Color pixelColor1 = mypic.GetPixel(i, z1);

                            //if (!((isInRange(pixelColor1.R, min, max) && isInRange(pixelColor1.G, min, max) && isInRange(pixelColor1.B, min, max)) || (pixelColor1.R == 200 && pixelColor1.G == 25 && pixelColor1.B == 21)))
                            if (!((isInRange(pixelColor1.R, min, max) && isInRange(pixelColor1.G, min, max) && isInRange(pixelColor1.B, min, max)) || (pixelColor1.R == 200 && pixelColor1.G == 25 && pixelColor1.B == 21)))
                            {
                                //int b1 = 5;
                                //top left

                                x = i;
                                y = z1;
                                break;
                            }
                        }


                        for (int i1 = i; i1 < imwid; i1++)
                        {
                            Color pixelColor1 = mypic.GetPixel(i1, z);



                            //if (!((isInRange(pixelColor1.R, min, max) && isInRange(pixelColor1.G, min, max) && isInRange(pixelColor1.B, min, max)) || (pixelColor1.R == 200 && pixelColor1.G == 25 && pixelColor1.B == 21)))
                            if (!((isInRange(pixelColor1.R, min, max) && isInRange(pixelColor1.G, min, max) && isInRange(pixelColor1.B, min, max)) || (pixelColor1.R == 200 && pixelColor1.G == 25 && pixelColor1.B == 21)))
                            {
                                //int b1 = 5;
                                //bottom right


                                w = i1 - i;
                                h = z - y;



                                // if (w <= 25)
                                if (i1 + OFSET_ZRight3 < imwid)
                                {
                                    Color pixelColorRight = mypic.GetPixel(i1 + OFSET_ZRight3, z);

                                    if ((isInRange(pixelColorRight.R, min, max) && isInRange(pixelColorRight.G, min, max) && isInRange(pixelColorRight.B, min, max)))
                                    {
                                        //continueZ = true;
                                        //break;
                                        continue;
                                    }
                                    else
                                    {
                                        //continueZ = true;

                                        if (i1 < imwid / 2)//ko vao dkien break ben tren
                                        {
                                            continueZ = true;
                                        }

                                        break;
                                    }
                                }

                                if (i1 + OFSET_ZRight3 >= imwid)//ko vao dkien break ben tren
                                {
                                    continueZ = true;
                                }

                                //continueZ = true;
                                break;
                            }
                            if (i1 == imwid - 1)//cham ben phai
                            {
                                w = i1 - i;
                                h = z - y;
                                break;
                            }
                        }

                        if (continueZ == true)
                        {

                            break;//check lai i
                        }


                        //xac dinh lai 1 lan nua haveFirstPoint
                        if (haveFirstLancherPoint == true && x == 0 && y == 0)
                        {
                            haveFirstPoint = true;
                        }




                        if (w == 0 && h == 0)
                        {
                            w = imwid;
                            h = z - y;
                        }

                        if (x == 0 && y == 0)// khong xac dinh duoc layout, cut theo default
                        {
                            //Rectangle section = new Rectangle(new Point(0, source.Size.Height - 150), new Size(source.Size.Width, 150));
                            x = 0;
                            y = imhei - CUT_TOP;
                            w = imwid;
                            h = CUT_TOP;

                            if (haveFirstPoint == false)
                            {
                                using (File.Create(@fileNoToast)) ;
                            }
                        }

                        if (w < BASE_OFSET_W || h < BASE_OFSET_H)// khong xac dinh duoc layout, cut theo default
                        {
                            ////Rectangle section = new Rectangle(new Point(0, source.Size.Height - 150), new Size(source.Size.Width, 150));
                            //x = 0;
                            //y = imhei - 120;
                            //w = imwid;
                            //h = 120;
                            //using (File.Create(@fileNoToast)) ;
                        }


                        //top right da biet
                        //int a = 5;

                        if (haveFirstPoint == true && x == 0)//nghi ngo la camera hoac cac case dac biet
                        {
                            x = 0;
                            y = imhei - CUT_TOP;
                            w = imwid;
                            h = CUT_TOP;
                        }

                        try
                        {
                            // Example use:     
                            //Bitmap source = new Bitmap(@imagePath);
                            Rectangle section = new Rectangle(new Point(x, y), new Size(w, h));

                            Bitmap CroppedImage = CropImage(mypic, section);
                            CroppedImage.Save(@cropImagePath, ImageFormat.Jpeg);
                            //int a = 5;
                        }
                        catch (Exception ex)
                        {
                            printLog("cropTBOptimize:" + ex.Message, "error");
                        }
                        return;


                    }

                }


                if (continueZ == true)
                {
                    //continue Z
                    continue;
                }
            }




            if (x == 0 && y == 0)// khong xac dinh duoc layout, cut theo default
            {
                //Rectangle section = new Rectangle(new Point(0, source.Size.Height - 150), new Size(source.Size.Width, 150));
                x = 0;
                y = imhei - CUT_TOP;
                w = imwid;
                h = CUT_TOP;
                if (haveFirstPoint == false)
                {
                    using (File.Create(@fileNoToast)) ;
                }
            }

            try
            {
                // Example use:     
                Bitmap source = new Bitmap(@imagePath);
                Rectangle section = new Rectangle(new Point(x, y), new Size(w, h));

                Bitmap CroppedImage = CropImage(source, section);
                CroppedImage.Save(@cropImagePath, ImageFormat.Jpeg);
                //int a = 5;
            }
            catch (Exception ex)
            {
                printLog("cropTBOptimize2: " + ex.Message, "error");
            }
            return;

        }

        private void txtPkg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (renderDone == true)
            {
                try
                {
                    TalkBackAutoTest.Properties.Settings.Default.txtPkg = txtPkg.Text;
                    stressTestPkg = txtPkg.Text;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Try Catch: " + ex.Message);
                    printLog("txtPkg_SelectedIndexChanged: " + ex.Message, "error");
                }
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void txtList_language_word_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word = txtList_language_word.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtList_language_word_TextChanged: " + ex.Message, "error");
            }
        }

        private void txtList_repeat_text_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text = txtList_repeat_text.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtList_repeat_text_TextChanged: " + ex.Message, "error");
            }
        }

        private void txtList_dup_word_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.blacklist_dup_word = txtList_dup_word.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtList_dup_word_TextChanged: " + ex.Message, "error");
            }
        }

        private void txtList_unlabelled_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.blacklist_unlabelled = txtList_unlabelled.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtList_unlabelled_TextChanged: " + ex.Message, "error");
                //}
            }

            //end draw on image

            //other function


            //end



        }

        private void addToBlackList(int idx)
        {
            try
            {

                if (lvResult.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem item in lvResult.SelectedItems)
                    {
                        string talkbackText = item.SubItems[3].Text.ToString();
                        if (talkbackText == "TalkbackText_NT")
                        {
                            talkbackText = item.SubItems[4].Text.ToString();//xml text
                        }

                        //TalkBackAutoTest.Properties.Settings.Default.txtWS = txtWS.Text;
                        //TalkBackAutoTest.Properties.Settings.Default.Save();

                        if (idx == 0)
                        {
                            string content = TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word;
                            if (content == "")
                            {
                                content += talkbackText.Trim();
                            }
                            else
                            {
                                content += "\r\n" + talkbackText.Trim();
                            }
                            txtList_language_word.Text = content;
                            TalkBackAutoTest.Properties.Settings.Default.blacklist_language_word = content;
                            TalkBackAutoTest.Properties.Settings.Default.Save();

                        }
                        else if (idx == 1)
                        {
                            string content = TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text;
                            if (content == "")
                            {
                                content += talkbackText.Trim();
                            }
                            else
                            {
                                content += "\r\n" + talkbackText.Trim();
                            }
                            txtList_repeat_text.Text = content;
                            TalkBackAutoTest.Properties.Settings.Default.blacklist_repeat_text = content;
                            TalkBackAutoTest.Properties.Settings.Default.Save();
                        }
                        else if (idx == 2)
                        {
                            string content = TalkBackAutoTest.Properties.Settings.Default.blacklist_dup_word;
                            if (content == "")
                            {
                                content += talkbackText.Trim();
                            }
                            else
                            {
                                content += "\r\n" + talkbackText.Trim();
                            }
                            txtList_dup_word.Text = content;
                            TalkBackAutoTest.Properties.Settings.Default.blacklist_dup_word = content;
                            TalkBackAutoTest.Properties.Settings.Default.Save();
                        }
                        else if (idx == 3)
                        {
                            string content = TalkBackAutoTest.Properties.Settings.Default.blacklist_unlabelled;
                            if (content == "")
                            {
                                content += talkbackText.Trim();
                            }
                            else
                            {
                                content += "\r\n" + talkbackText.Trim();
                            }
                            txtList_unlabelled.Text = content;
                            TalkBackAutoTest.Properties.Settings.Default.blacklist_unlabelled = content;
                            TalkBackAutoTest.Properties.Settings.Default.Save();
                        }
                    }
                }
                // return;
                //int p = lvResult.FocusedItem.Index;



            }
            catch (Exception ex)
            {
                printLog("addToBlackList:" + ex.Message, "error");
            }
        }


        private void languageWordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addToBlackList(0);
        }

        private void repeatTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addToBlackList(1);
        }

        private void dupWordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addToBlackList(2);
        }

        private void unlabelledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addToBlackList(3);
        }

        private string isDupWordNew(string s)
        {
            string input = s;
            string[] words = input.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            var wordCounts = words.GroupBy(w => w.Trim())
                                  .ToDictionary(g => g.Key, g => g.Count());
            var duplicateWords = wordCounts.Where(w => w.Value > 1)
                                           .Select(w => w.Key)
                                           .ToList();

            //Console.WriteLine("Duplicate words:");
            string output = "";
            foreach (var word in duplicateWords)
            {
                output += word + ", ";
            }

            if (output == "")//nothing
            {
                string input1 = ConvertToNoTone(s);
                string[] words1 = input1.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                var wordCounts1 = words1.GroupBy(w => w.Trim())
                                      .ToDictionary(g => g.Key, g => g.Count());
                var duplicateWords1 = wordCounts1.Where(w => w.Value > 1)
                                               .Select(w => w.Key)
                                               .ToList();

                //Console.WriteLine("Duplicate words:");
                foreach (var word1 in duplicateWords1)
                {
                    output += word1 + ", ";
                }
            }

            return output == "" ? output : output.TrimEnd(',', ' ');
        }

        private void btnPyEnv_Click(object sender, EventArgs e)
        {
            RunCommandShow("pip install opencv-python numpy easyocr");
            MessageBox.Show("Installed ok! OR you can try again with command on CMD Window:\r\n pip install opencv-python numpy easyocr");
        }

        private void cb_NoOutputAfterTab_CheckedChanged(object sender, EventArgs e)
        {

            try
            {
                if (cb_NoOutputAfterTab.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NoOutputAfterTab = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NoOutputAfterTab = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cb_NoOutputAfterTab_CheckedChanged: " + ex.Message, "error");
            }
        }

        private void cbDifflang_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbDifflang.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_DiffLang = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_DiffLang = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cbDifflang_CheckedChanged: " + ex.Message, "error");
            }
        }

        private void cb_RepeatPreviousObject_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cb_RepeatPreviousObject.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_RepeatPreviousObject = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_RepeatPreviousObject = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cb_RepeatPreviousObject_CheckedChanged: " + ex.Message, "error");
            }
        }

        private void cb_DupWord_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cb_DupWord.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_DupWord = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_DupWord = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cb_DupWord_CheckedChanged: " + ex.Message, "error");
            }
        }

        private void cb_NoOutput_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cb_NoOutput.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NoOutput = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NoOutput = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cb_NoOutput_CheckedChanged: " + ex.Message, "error");
            }
        }

        private void cb_NAF_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cb_NAF.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NAF = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NAF = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("cb_NAF_CheckedChanged: " + ex.Message, "error");
            }
        }

        private void copyToMoreDetailFolder(string folderResult, string numberOfObject)
        {
            try
            {
                string moreDetailFolder = @folderResult + "//more_details";
                if (!Directory.Exists(@moreDetailFolder))
                {
                    System.IO.Directory.CreateDirectory(@moreDetailFolder);
                }

                if (File.Exists(@folderResult + "/cropfocus_obj" + numberOfObject + ".png"))
                {
                    File.Copy(@folderResult + "/cropfocus_obj" + numberOfObject + ".png", @moreDetailFolder + "/cropfocus_obj" + numberOfObject + ".png", true);
                    File.Delete(@folderResult + "/cropfocus_obj" + numberOfObject + ".png");
                }
                if (File.Exists(@folderResult + "/croptb_obj" + numberOfObject + ".png"))
                {
                    File.Copy(@folderResult + "/croptb_obj" + numberOfObject + ".png", @moreDetailFolder + "/croptb_obj" + numberOfObject + ".png", true);
                    File.Delete(@folderResult + "/croptb_obj" + numberOfObject + ".png");

                }
                if (File.Exists(@folderResult + "/no_toast.txt"))
                {
                    File.Copy(@folderResult + "/no_toast.txt", @moreDetailFolder + "/no_toast.txt", true);
                    File.Delete(@folderResult + "/no_toast.txt");
                }
                if (File.Exists(@folderResult + "/obj" + numberOfObject + ".png"))
                {
                    File.Copy(@folderResult + "/obj" + numberOfObject + ".png", @moreDetailFolder + "/obj" + numberOfObject + ".png", true);
                    File.Delete(@folderResult + "/obj" + numberOfObject + ".png");
                }


                for (int it = 1; it <= MAX_SCREENSHOT; it++)
                {
                    try
                    {
                        string NoToastPath = @folderResult + "/" + it + "_no_toast.txt";
                        string NoToastPathMoreDetails = @folderResult + "/more_details/" + it + "_no_toast.txt";
                        string imageCropPath = @folderResult + "/croptb_" + it + "_obj" + numberOfObject + ".png";
                        string imageCropPathMoreDetails = @folderResult + "/more_details/croptb_" + it + "_obj" + numberOfObject + ".png";
                        string imagePath = @folderResult + "/" + it + "_obj" + numberOfObject + ".png";
                        string imagePathMoreDetails = @folderResult + "/more_details/" + it + "_obj" + numberOfObject + ".png";


                        if (File.Exists(@NoToastPath))
                        {
                            File.Copy(@NoToastPath, @NoToastPathMoreDetails, true);
                            File.Delete(@NoToastPath);
                        }
                        if (File.Exists(imageCropPath))
                        {
                            File.Copy(imageCropPath, imageCropPathMoreDetails, true);
                            File.Delete(imageCropPath);
                        }
                        if (File.Exists(@imagePath))
                        {
                            File.Copy(@imagePath, @imagePathMoreDetails, true);
                            File.Delete(@imagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        printLog("copyToMoreDetailFolder trycat:" + ex.Message, "error");
                    }
                }
            }
            catch (Exception ex)
            {
                printLog("copyToMoreDetailFolder:" + ex.Message, "error");
            }
            //move to moreDetailFolder

        }

        private void cb_Unlabelled_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cb_Unlabelled.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_Unlabelled = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_Unlabelled = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("Try Catch: " + ex.Message, "error");
            }
        }

        ToolTip mTooltip;
        Point mLastPos = new Point(-1, -1);
        private void lvResult_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                ListViewHitTestInfo info = lvResult.HitTest(e.X, e.Y);

                if (mTooltip == null)
                    mTooltip = new ToolTip();

                if (mLastPos != e.Location)
                {
                    if (info.Item != null && info.SubItem != null && (info.SubItem.Text == info.Item.SubItems[2].Text || info.SubItem.Text == info.Item.SubItems[1].Text || info.SubItem.Text == info.Item.SubItems[1].Text || info.SubItem.Text == info.Item.SubItems[8].Text))
                    {
                        if (info.Item.SubItems[2].Text.Contains("Re-Check"))
                        {
                            //mTooltip.ToolTipTitle = info.Item.Text;
                            //mTooltip.Show(info.SubItem.Text, info.Item.ListView, e.X, e.Y, 20000);

                            mTooltip.ToolTipTitle = "Guide to check again Re-check case";
                            mTooltip.Show("You should recheck Talback Text content and focused object on screenshot/video or manual check to confirm it", info.Item.ListView, e.X, e.Y, 20000);
                        }
                        else if (info.Item.SubItems[8].Text == "Consider")
                        {
                            if (info.Item.SubItems[2].Text == "DIFF_LANG")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again DIFF_LANG case";
                                mTooltip.Show("There has some words that are not translated. Please check the word list and kindly decide if it is issue or not.", info.Item.ListView, e.X, e.Y, 20000);
                            }
                            else if (info.Item.SubItems[2].Text == "NO_CHANGE_FOCUS_AFTER_TAB")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again NO_CHANGE_FOCUS_AFTER_TAB case";
                                mTooltip.Show("This issue occurs using Tab key. Please check again with Alt > keys. If the issue still occurs, register it on PLM. If not, please skip it.", info.Item.ListView, e.X, e.Y, 20000);
                            }
                            else if (info.Item.SubItems[2].Text == "NO_OUTPUT")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again NO_OUPUT case";
                                mTooltip.Show("This issue occurs using Tab key. Please check again with Alt > keys. If the issue still occurs, register it on PLM. If not, please skip it.", info.Item.ListView, e.X, e.Y, 20000);
                            }
                            else if (info.Item.SubItems[2].Text == "NAF")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again NAF case";
                                mTooltip.Show("This issue occurs using Tab key. Please check again with Alt > keys. If the issue still occurs, register it on PLM. If not, please skip it.", info.Item.ListView, e.X, e.Y, 20000);
                            }
                            else if (info.Item.SubItems[2].Text == "DUP_WORD")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again DUP_WORD case";
                                mTooltip.Show("Should manual check again. If not issue please add to Blacklist", info.Item.ListView, e.X, e.Y, 20000);
                            }
                            else if (info.Item.SubItems[2].Text == "REPEAT_PREVIOUS_OBJECT")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again REPEAT_PREVIOUS_OBJECT case";
                                mTooltip.Show("Should manual check again. If not issue please add to Blacklist", info.Item.ListView, e.X, e.Y, 20000);
                            }
                            else if (info.Item.SubItems[2].Text == "NO_FOCUSABLE_HEADER")
                            {
                                mTooltip.ToolTipTitle = "Guide to check again NO_FOCUSABLE_HEADER case";
                                mTooltip.Show("This issue occurs using Tab key. Please check again with Alt > keys. If the issue still occurs, register it on PLM. If not, please skip it.", info.Item.ListView, e.X, e.Y, 20000);
                            }

                            else
                            {
                                mTooltip.SetToolTip(lvResult, string.Empty);
                            }

                        }
                        else
                        {
                            mTooltip.SetToolTip(lvResult, string.Empty);
                        }
                    }
                    else
                    {
                        mTooltip.SetToolTip(lvResult, string.Empty);
                    }
                }

                mLastPos = e.Location;
            }
            catch (Exception ex)
            {
                printLog("Try Catch: " + ex.Message, "error");
            }
        }

        private void cb_NoFocusableHeader_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cb_NoFocusableHeader.Checked)
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NoFocusableHeader = true;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
                else
                {
                    TalkBackAutoTest.Properties.Settings.Default.set_NoFocusableHeader = false;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("Try Catch: " + ex.Message, "error");
            }
        }

        private string ConvertToNoTone(string input)
        {
            string[] vietnameseChars = new string[] { "đ", "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "í", "ì", "ỉ", "ĩ", "ị", "ý", "ỳ", "ỷ", "ỹ", "ỵ" };
            string[] noToneChars = new string[] { "d", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "i", "i", "i", "i", "i", "y", "y", "y", "y", "y" };

            StringBuilder result = new StringBuilder(input);
            for (int i = 0; i < vietnameseChars.Length; i++)
            {
                result.Replace(vietnameseChars[i], noToneChars[i]);
            }

            return result.ToString();
        }

        private void btnTABKey_Click(object sender, EventArgs e)
        {
            TAB_COMMAND();
        }

        private void btnDpadUp_Click(object sender, EventArgs e)
        {
            //getAnotherObjectsFromStartEnd(0, 9);
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell input keyevent DPAD_UP", 0);
            }
        }

        private void btnDpadDown_Click(object sender, EventArgs e)
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell input keyevent DPAD_DOWN", 0);
            }
        }

        private void btnDpadLeft_Click(object sender, EventArgs e)
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell input keyevent DPAD_LEFT", 0);
            }
        }

        private void btnDpadRight_Click(object sender, EventArgs e)
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell input keyevent DPAD_RIGHT", 0);
            }
        }

        private void viewXMLScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int p = lvResult.FocusedItem.Index;
                string objId = lvResult.Items[p].SubItems[1].Text.ToString();
                string logFile = txtWS.Text + "/Result/" + objId.ToLower() + "/" + objId.ToLower() + ".xml";
                var f = Process.Start(logFile);
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
            }
        }

        private void panel_left_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lbPass_Click(object sender, EventArgs e)
        {

        }

        private void lbTotal_Click(object sender, EventArgs e)
        {

        }

        private void lbFail_Click(object sender, EventArgs e)
        {

        }

        private void lbConsider_Click(object sender, EventArgs e)
        {

        }

        private void lbNT_Click(object sender, EventArgs e)
        {

        }

        private void txtMaximumScreenShot_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int maxScreenShots = Int16.Parse(txtMaximumScreenShot.Text);
                maxScreenShots = maxScreenShots > 0 && maxScreenShots < 11 ? maxScreenShots : 5;
                TalkBackAutoTest.Properties.Settings.Default.txtMaximumScreenShot = maxScreenShots;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtMaximumScreenShot_TextChanged: " + ex.Message, "error");
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                cleanLogcat();
                textBox1.Text = "";
                textBox2.Text = "";
                printLog("Clean Logcat ok");
            }
            catch (Exception ex)
            {
                printLog("TryCatch_CleanLogcat:" + ex.Message, "error");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
        }


        private long convertToMs(string line)
        {
            try
            {
                //string timeString = "09:02:33.078";
                string[] words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string s = words[1];
                //TimeSpan timeSpan = TimeSpan.ParseExact(s, "HH:mm:ss.fff", CultureInfo.InvariantCulture);
                //long totalMilliseconds = (long)timeSpan.TotalMilliseconds;
                string[] parts = s.Split(new[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
                int hours = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);
                int milliseconds = int.Parse(parts[3]);

                //TimeSpan timeSpan = new TimeSpan(hours, minutes, seconds, milliseconds);
                //long totalMilliseconds = (long)timeSpan.TotalMilliseconds;
                long totalMilliseconds = hours * 60 * 60 * 1000 + minutes * 60 * 1000 + seconds * 1000 + milliseconds;

                return totalMilliseconds;
            }
            catch (Exception ex)
            {
                printLog("TRYCAT_convertToMs:" + ex.Message, "error");
                return 0;
            }
        }

        private bool isnotTTSInLog()
        {
            if (device.serial != null && device.serial != "")
            {
                Process p1 = new Process();
                // Redirect the output stream of the child process.
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                // RunCommand("adb -s " + current_serial + " shell logcat -v threadtime -t 1000 >" + pathName + "/LOG_FC.txt");
                //string command = "/c adb -s " + current_serial + " shell \"logcat -v threadtime -t 10000|grep 'FATAL EXCEPTION'\"";
                //string command = "/c adb  shell \"logcat -v threadtime -t 300000|grep -E '\\[Synthesize\\] text =|\\[TtsSpan\\]'\"";
                string command = "/c adb -s " + device.serial + " shell \"logcat -v threadtime -t 300000|grep 'SamsungTTS:'\"";
                p1.StartInfo.Arguments = command;
                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p1.Start();
                string output = p1.StandardOutput.ReadToEnd();
                p1.WaitForExit();
                if (output == "")
                {
                    return true;
                }
            }

            return false;
        }

        private string[] getTTSText(string characterCOMBINE = "\r\n", string type = "manual")
        {
            string[] kq = { "", "" };
            if (device.serial != null)
            {
                try
                {
                    Process p1 = new Process();
                    // Redirect the output stream of the child process.
                    p1.StartInfo.UseShellExecute = false;
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                    // RunCommand("adb -s " + current_serial + " shell logcat -v threadtime -t 1000 >" + pathName + "/LOG_FC.txt");
                    //string command = "/c adb -s " + current_serial + " shell \"logcat -v threadtime -t 10000|grep 'FATAL EXCEPTION'\"";
                    //string command = "/c adb  shell \"logcat -v threadtime -t 300000|grep -E '\\[Synthesize\\] text =|\\[TtsSpan\\]'\"";
                    string command = "/c adb -s " + device.serial + " shell \"logcat -v threadtime -t 300000|grep 'SamsungTTS:'\"";
                    p1.StartInfo.Arguments = command;
                    p1.StartInfo.CreateNoWindow = true;
                    p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p1.Start();
                    string output = p1.StandardOutput.ReadToEnd();
                    p1.WaitForExit();
                    bool haveSynatect = false;

                    //output = File.ReadAllText(@"D:/AA/AA/fakelogcat.txt");

                    if (output == "")//ko co cai nao thi ko FC
                    {
                        printLog("No Output");
                        return kq;
                    }
                    else
                    {

                        printLog(output);



                        var op = output.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        string ccurrentScreen = getCurrentScreen();
                        string s = "";
                        string s1 = "";
                        string s1Temp = "";

                        string lastS1Temp = "";

                        int idx = -1;
                        bool stillAppend = false;


                        long timeGeneration = 0;
                        long timeCallStop = 0;
                        foreach (string line in op)
                        {
                            idx++;
                            if (type == "auto")
                            {
                                if (line.Contains("text = TalkBack off") || line.Contains("text = TalkBack tắt"))
                                {
                                    break;
                                }
                            }

                            //if (line.Contains("SamsungTTS: [IsAvailable]") || line.Contains("SamsungTTS: onGet") || line.Contains("[Synthesize] Empty input. ") || line.Contains("[Synthesize] request") || line.EndsWith("SamsungTTS: ") || line.EndsWith("[Synthesize] upload done.") || line.Contains("[LoadLanguage]") || line.Contains("StreamType"))
                            //{
                            //    s += line + "\r\n";
                            //    //continue;
                            //}
                            s += line + characterCOMBINE;
                            if (line.Contains("[Synthesize]"))
                            {
                                haveSynatect = true;
                            }



                            if (haveSynatect == true)
                            {
                                if (line.Contains("Called onStop()"))
                                {
                                    s1Temp = "";
                                    //s no thing
                                }
                                //else if (!(line.Contains("[Synthesize] generation done") || line.Contains("SamsungTTS: [IsAvailable]") || line.Contains("SamsungTTS: onGet") || line.Contains("[Synthesize] Empty input. ") || line.Contains("[Synthesize] request") || line.EndsWith("SamsungTTS: ") || line.EndsWith("[Synthesize] upload done.") || line.Contains("[LoadLanguage]") || line.Contains("StreamType")))
                                else
                                {
                                    bool rs = InType(line);
                                    if (line.Contains("[Synthesize] text =") || (rs == false && stillAppend == true))
                                    {
                                        if (line.Contains("[Synthesize] text ="))
                                        {
                                            stillAppend = true;
                                        }

                                        //s += line + "\r\n";
                                        string textOnly = getOnlyText(line);
                                        if (textOnly.Trim() != "")
                                        {
                                            s1Temp += getOnlyText(line) + characterCOMBINE;
                                        }
                                    }
                                    else
                                    {
                                        stillAppend = false;
                                    }

                                    //bool rs = InType(line);
                                    //if (!rs)
                                    //{
                                    //    s1Temp += getOnlyText(line) + characterCOMBINE;
                                    //}
                                }


                                //if ((line.Contains("[Synthesize] generation done") && idx == op.Count() - 1) || (line.Contains("[Synthesize] generation done") && idx == op.Count() - 2 && !op[idx + 1].Contains("Called onStop()")) || (line.Contains("[Synthesize] generation done") && idx < op.Count() - 2 && !op[idx + 1].Contains("Called onStop()") && !op[idx + 2].Contains("Called onStop()")))                                //if (line.Contains("[Synthesize] generation done"))
                                if ((line.Contains("[Synthesize] generation done")))
                                {

                                    long timeMsGeneration = convertToMs(line);
                                    long timeMsCallStop = 0;
                                    long timeMsGenerationNext = 0;

                                    int idxidx = 1;
                                    while (idx + idxidx < op.Length - 1)
                                    {
                                        if (op[idx + idxidx].Contains("Called onStop()"))
                                        {
                                            timeMsCallStop = convertToMs(op[idx + idxidx]);
                                            break;
                                        }

                                        if (op[idx + idxidx].Contains("[Synthesize] text"))
                                        {
                                            break;
                                        }
                                        idxidx++;
                                    }

                                    //if(idx+1 < op.Length -1 && op[idx + 1].Contains("Called onStop()"))
                                    //{
                                    //    timeMsCallStop = convertToMs(op[idx+1]);
                                    //}



                                    //if(idx+2 < op.Length -1 && op[idx + 2].Contains("Called onStop()"))
                                    //{
                                    //    timeMsCallStop = convertToMs(op[idx+2]);
                                    //}
                                    //if (idx + 3 < op.Length - 1 && op[idx + 3].Contains("Called onStop()"))
                                    //{
                                    //    timeMsCallStop = convertToMs(op[idx + 3]);
                                    //}
                                    //if (idx + 4 < op.Length - 1 && op[idx + 4].Contains("Called onStop()"))
                                    //{
                                    //    timeMsCallStop = convertToMs(op[idx + 4]);
                                    //}
                                    //if (idx + 5 < op.Length - 1 && op[idx + 5].Contains("Called onStop()"))
                                    //{
                                    //    timeMsCallStop = convertToMs(op[idx + 5]);
                                    //}

                                    //get generation done ben duoi

                                    for (int i = idx + 1; i < op.Length; i++)
                                    {
                                        if (op[i].Contains("[Synthesize] text = "))
                                        {
                                            timeMsGenerationNext = convertToMs(op[i]);
                                            break;
                                        }
                                    }


                                    if (timeMsGeneration == 0 || timeMsCallStop == 0 || timeMsCallStop - timeMsGeneration > TIMEOUT_COMMAND)//200
                                    {
                                        if (timeMsCallStop == 0 && timeMsGenerationNext - timeMsGeneration < TIMEOUT_COMMAND && timeMsGenerationNext - timeMsGeneration > 0)//khong xu ly gi
                                        {
                                            s1Temp = "";
                                            continue;
                                        }

                                        if (lastS1Temp != s1Temp)//10 characters tro len Hi, Button, theo doi
                                        {
                                            if (s1Temp.TrimEnd(',', ' ').Contains(",,") || s1Temp.StartsWith(", "))
                                            {
                                                HAVECOMMA2 = true;
                                            }

                                            s1 += s1Temp;

                                            lastS1Temp = s1Temp;
                                            s1Temp = "";
                                        }
                                        else
                                        {
                                            s1Temp = "";
                                            printLog("lastS1Temp same s1Temp:" + lastS1Temp);
                                        }
                                    }
                                }

                                //if (ccurrentScreen.Contains("com.sec.android.app.launcher"))
                                //{
                                //    if ((line.Contains("[Synthesize] generation done") && idx == op.Count() - 1) || (line.Contains("[Synthesize] generation done") && idx == op.Count() - 2 && !op[idx + 1].Contains("Called onStop()")) || (line.Contains("[Synthesize] generation done") && idx < op.Count() - 2 && !op[idx + 1].Contains("Called onStop()") && !op[idx + 2].Contains("Called onStop()")))
                                //    //if (line.Contains("[Synthesize] generation done"))
                                //    {
                                //        if (s1Temp.TrimEnd(',', ' ').Contains(",,") || s1Temp.StartsWith(", "))
                                //        {
                                //            HAVECOMMA2 = true;
                                //        }

                                //        s1 += s1Temp;
                                //        s1Temp = "";
                                //    }
                                //}
                                //else
                                //{
                                //    if (line.Contains("[Synthesize] generation done"))
                                //    {
                                //        if (s1Temp.TrimEnd(',',' ').Contains(",,") || s1Temp.StartsWith(", "))
                                //        {
                                //            HAVECOMMA2 = true;
                                //        }

                                //        s1 += s1Temp;
                                //        s1Temp = "";
                                //    }
                                //}
                                //if ((line.Contains("[Synthesize] generation done") && idx == op.Count() - 1) || (line.Contains("[Synthesize] generation done") && idx == op.Count() - 2 && !op[idx + 1].Contains("Called onStop()")) || (line.Contains("[Synthesize] generation done") && idx < op.Count() - 2 && !op[idx + 1].Contains("Called onStop()") && !op[idx + 2].Contains("Called onStop()")))

                            }
                        }

                        s = s.TrimEnd('\r', '\n', ',', ' ');
                        s1 = s1.TrimEnd('\r', '\n', ',', ' ');
                        kq[0] = s;
                        kq[1] = s1;
                        return kq;
                    }
                }
                catch (Exception ex)
                {
                    //return false;
                    printLog("No Output");
                    return kq;
                }
            }
            else
            {
                printLog("Please connect at least 1 device", "error");
                return kq;
            }


            return kq;
        }

        private bool InType(string s)
        {
            //else if (!(line.Contains("[Synthesize] generation done") || line.Contains("SamsungTTS: [IsAvailable]") || line.Contains("SamsungTTS: onGet") || line.Contains("[Synthesize] Empty input. ") || line.Contains("[Synthesize] request") || line.EndsWith("SamsungTTS: ") || line.EndsWith("[Synthesize] upload done.") || line.Contains("[LoadLanguage]") || line.Contains("StreamType")))

            string[] blacklist = { "JNI:", "SamsungTTS: [Engine]", "SamsungTTS: [setLanguage", "SamsungTTS: [check_allowed_package]", "[Synthesize] generation done", "SamsungTTS: [IsAvailable]", "SamsungTTS: onGet", "[Synthesize] Empty input. ", "[Synthesize] request", "[LoadLanguage]", "StreamType", "SamsungTTS: Service unbound.", "unloadEngine :", "JNI: [closeEngine]", "[SmtTTS]", "Loading vData files:", "Loading is succeed.", "TalkBack tắt", "TalkBack off", "Bật TalkBack", "TalkBack on", "SamsungTTS: [NATIVE] " };
            //string[] blacklistEndWith = { "SamsungTTS: ", "[Synthesize] upload done." };
            string[] blacklistEndWith = { "[Synthesize] upload done." };
            foreach (string keyword in blacklist)
            {
                if (s.Contains(keyword))
                {
                    return true;
                }
            }
            foreach (string keyword in blacklistEndWith)
            {
                if (s.EndsWith(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";

            string[] kq = getTTSText("\r\n", "manual");
            string s = kq[0];
            string s1 = kq[1];

            textBox1.Text = s;
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();

            textBox2.Text = s1;
            textBox2.SelectionStart = textBox1.TextLength;
            textBox2.ScrollToCaret();


        }

        private string getOnlyText(string s)
        {
            string pattern = @"^.*?SamsungTTS: ";
            string result = Regex.Replace(s, pattern, "");

            string pattern1 = @"^.*?\[Synthesize\] text = ";
            string result1 = Regex.Replace(result, pattern1, "");


            return result1.Trim();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            //updatelistview
            updateListViewByKey(txtSearch.Text);

        }

        private void viewScreenVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                int p = lvResult.FocusedItem.Index;
                string objId = lvResult.Items[p].SubItems[1].Text.ToString();
                string logFile = txtWS.Text + "/Result/" + objId.ToLower() + "/" + objId.ToLower() + ".mp4";
                var f = Process.Start(logFile);
                //f.Close();
            }
            catch (Exception ex)
            {
                printLog("viewScreenVideoToolStripMenuItem_Click:" + ex.Message, "error");
            }
        }

        private void btnGuide_Click(object sender, EventArgs e)
        {
            //getTTSText();
            //SamePrevious("Not checked, Check box, memory","Not checked, Check box, memory");

            

            GuideForm f1 = new GuideForm();
            f1.StartPosition = FormStartPosition.CenterParent;
            f1.ShowDialog();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    //SamePrevious("hom nay, toi, di, hoc, On, Switch", "hom nay, anh, di, hoc, On, Switch");
        //    //MessageBox.Show(globalSamePrevious);
        //    RemoveBlackListLanguage("English Qwerty Hiển thị, Đang hiển thị Bàn phím Samsung, Samsung Notes");
        //}


        private List<AppName> getListAppNameSearch()
        {
            if (txtSearchPkg.Text == "")
            {
                return listAppName;
            }
            else
            {
                List<AppName> searchList = new List<AppName>();
                foreach (AppName x in listAppName)
                {
                    if(x.pkgName.ToLower().Contains(txtSearchPkg.Text.ToLower()) || x.appName.ToLower().Contains(txtSearchPkg.Text.ToLower()))
                    {
                        searchList.Add(x);
                    }
                }
                return searchList;
            }
        }

        private List<AppName> getListAppName()
        {
            List<AppName> listAppName = new List<AppName>();

            string path = txtWS.Text + "\\list_pkgs.txt";
            List<AppName> listAppNames1 = getPackageToList(@path);
            if (listAppNames1 != null)
            {
                listAppNames1.Insert(0, new AppName(ALL_APPS, ALL_APPS, true));
                return listAppNames1;
            }


            listAppName.Add(new AppName(ALL_APPS, ALL_APPS, true));
            listAppName.Add(new AppName("Settings", "com.android.settings", true));
            listAppName.Add(new AppName("Contacts", "com.samsung.android.app.contacts", true));
            listAppName.Add(new AppName("Calendar", "com.samsung.android.calendar", true));
            listAppName.Add(new AppName("Phone", "com.samsung.android.dialer", true));
            listAppName.Add(new AppName("Messages", "com.samsung.android.messaging", true));
            listAppName.Add(new AppName("Camera", "com.sec.android.app.camera", true));
            listAppName.Add(new AppName("Samsung Internet", "com.sec.android.app.sbrowser", true));
            listAppName.Add(new AppName("AR Zone", "com.samsung.android.arzone", true));
            listAppName.Add(new AppName("Smart​Things", "com.samsung.android.oneconnect", true));
            listAppName.Add(new AppName("Samsung Health", "com.sec.android.app.shealth", true));
            listAppName.Add(new AppName("Reminder", "com.samsung.android.app.reminder", true));
            listAppName.Add(new AppName("Gaming Hub", "com.samsung.android.game.gamehome", true));

            listAppName.Add(new AppName("My Files", "com.sec.android.app.myfiles", true));
            listAppName.Add(new AppName("Galaxy Store", "com.sec.android.app.samsungapps", true));
            listAppName.Add(new AppName("Samsung Notes", "com.samsung.android.app.notes", true));
            listAppName.Add(new AppName("Clock", "com.sec.android.app.clockpackage", true));
            listAppName.Add(new AppName("Calculator", "com.sec.android.app.popupcalculator", true));

            listAppName.Add(new AppName("Samsung Members", "com.samsung.android.voc", true));
            listAppName.Add(new AppName("Tasks", "com.samsung.android.app.taskedge", true));
            listAppName.Add(new AppName("Gallery", "com.sec.android.gallery3d", true));
            listAppName.Add(new AppName("Voice Recorder", "com.sec.android.app.voicenote", true));
            listAppName.Add(new AppName("Voice Recorder", "com.samsung.android.intellivoiceservice", true));
            listAppName.Add(new AppName("PEN UP", "com.sec.penup", true));
            listAppName.Add(new AppName("Samsung Music", "com.sec.android.app.music", true));
            listAppName.Add(new AppName("Routine", "com.samsung.android.app.routines", true));


            listAppName.Add(new AppName("Samsung TV Plus", "com.samsung.android.tvplus", true));


            listAppName.Add(new AppName("Smart Switch", "com.sec.android.easyMover", false));
            listAppName.Add(new AppName("Accessibility", "com.samsung.android.accessibility.talkback", false));
            listAppName.Add(new AppName("Accessibility", "com.samsung.accessibility", false));
            listAppName.Add(new AppName("Accessibility", "com.google.android.apps.accessibility.voiceaccess", false));
            listAppName.Add(new AppName("Accessibility", "com.google.audio.hearing.visualization.accessibility.scribe", false));
            return listAppName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TALKBACK_ON();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TALKBACK_OFF();
        }

        private void btnGetPackage_Click(object sender, EventArgs e)
        {
            init_install_apk();
            List<AppName> list = getAllPackagesName("all");
            if (list != null)
            {
                list.Insert(0, new AppName(ALL_APPS, ALL_APPS, true));

                List<AppName> monkeyList = new List<AppName>();

                foreach (AppName x in list)
                {
                    if (x.supportMonkey == true)
                    {
                        monkeyList.Add(new AppName(x.appName,x.pkgName,x.supportMonkey));
                    }
                }

                monkeyList.Sort((a, b) => string.Compare(a.pkgName, b.pkgName, StringComparison.Ordinal));

                //biding data
                txtPkg.DataSource = monkeyList;
                txtPkg.DisplayMember = "pkgName";
                txtPkg.ValueMember = "pkgName";
                renderDone = true;

                printLog("Get Packages OK");
            }
            else
            {
                printLog("Nothing to get");
            }
            //herre

        }

        private void init_install_apk()
        {
            if (device.serial == null || device.serial == "")
            {
                printLog("Need plug at least 1 device for testing");
                return;
            }

            bool check = checkApkExisted();
            if (check == false)
            {

                printLog("This is First Time.Please wait a little bit to install Apk service for device");
                install_apk_leak_activity();
                System.Threading.Thread.Sleep(6000);
            }
            else
            {
                bool check_old_version = checkApkOldVersion();
                if (check_old_version == true)
                {
                    printLog("Service Apk on your device is old version. Please wait to install new version");
                    install_apk_leak_activity();
                    System.Threading.Thread.Sleep(6000);
                }
            }
        }

        private void install_apk_leak_activity()
        {
            if (device.serial == null || device.serial == "")
            {
                printLog("Need plug at least 1 device for testing");
                return;
            }

            string root_file = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\checking_activity.apk";
            // string root_file = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\checking_activity.apk";

            // Redirect the output stream of the child process.
            string packageName = "leakacitivity.svmc.com.checkingactivity";
            Process p0 = new Process();
            p0.StartInfo.UseShellExecute = false;
            p0.StartInfo.RedirectStandardOutput = true;
            p0.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p0.StartInfo.Arguments = "/c adb uninstall " + packageName;
            p0.StartInfo.CreateNoWindow = true;
            p0.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p0.Start();

            p0.WaitForExit();

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = "/c adb -s " + device.serial + " install -r -g " + "\"" + @root_file + "\"";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();

            p.WaitForExit();
            printLog("[Info] adb -s " + device.serial + " install -r -g " + @root_file, "info");
            printLog("Installed Service Apk successfuly", "info");
            //System.Threading.Thread.Sleep(10000);
        }

        private bool checkApkExisted()
        {
            Process p1 = new Process();
            //MessageBox.Show(serial.ToString());
            // Redirect the output stream of the child process.
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p1.StartInfo.Arguments = @"/c adb -s " + device.serial + " shell pm path leakacitivity.svmc.com.checkingactivity";
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p1.Start();
            string output = p1.StandardOutput.ReadToEnd();
            if (output != "")
            {
                p1.Close();
                return true;
            }
            else
            {
                p1.Close();
                return false;
            }
        }

        private bool checkApkOldVersion()
        {
            float current_apk_version = 2.0f;
            Process p1 = new Process();
            //MessageBox.Show(serial.ToString());
            // Redirect the output stream of the child process.
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            //leakacitivity.svmc.com.checkingactivity
            p1.StartInfo.Arguments = @"/c adb -s " + device.serial + " shell dumpsys package leakacitivity.svmc.com.checkingactivity | grep versionName";
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p1.Start();
            string output = p1.StandardOutput.ReadToEnd();
            if (output != "" && output.Contains("versionName"))
            {
                output = output.Trim();
                string[] split = output.Split('=');
                float device_apk = float.Parse(split[1], CultureInfo.InvariantCulture);
                if (current_apk_version > device_apk)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            return false;
        }



        private List<AppName> getAllPackagesName(string type)
        {
            if (device.serial == null || device.serial == "")
            {
                return null;
            }


            //string path = folder_path + "\\list_pkgs.txt";

            string path = txtWS.Text + "\\list_pkgs.txt";
            //File.Delete(@path);

            //init_install_apk();


            Process p1 = new Process();
            //MessageBox.Show(serial.ToString());
            // Redirect the output stream of the child process.
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            //leakacitivity.svmc.com.checkingactivity
            if (type == "all")
            {
                p1.StartInfo.Arguments = @"/c adb -s " + device.serial + " shell am start -n leakacitivity.svmc.com.checkingactivity/leakacitivity.svmc.com.checkingactivity.MainActivity";
            }
            else
            {
                p1.StartInfo.Arguments = @"/c adb -s " + device.serial + " shell am start -n leakacitivity.svmc.com.checkingactivity/leakacitivity.svmc.com.checkingactivity.GetUIPackages";
            }
            p1.StartInfo.CreateNoWindow = true;
            p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p1.Start();

            System.Threading.Thread.Sleep(3000);
            //string arg = "/c adb pull //data//data//getpackages.kidpro1412.com.kidpro1412_getpackages//files//trungtuan_packages_info.txt " + path;
            //string arg = "/c adb pull //sdcard//svmc_checking_activity//svmc_get_all_packages.txt \"" + path + "\"";
            string arg = "/c adb -s " + device.serial + " pull //sdcard//svmc_checking_activity//svmc_get_all_packages.txt \"" + path + "\"";
            //MessageBox.Show("Test");

            printLog(@arg, "info");

            Process p = new Process();
            //MessageBox.Show(serial.ToString());
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            p.StartInfo.Arguments = @arg;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();

            p.WaitForExit();
            p1.WaitForExit();


            //return null;

            List<AppName> listAppNames = getPackageToList(@path);
            return listAppNames;
        }

        List<AppName> getPackageToList(string @path)
        {
            if (File.Exists(@path))
            {
                List<AppName> list = new List<AppName>();
                string[] lines = File.ReadAllLines(@path);

                for (int i = 0; i < lines.Count(); i++)
                {
                    string s = lines[i];
                    if (s.Contains(';'))
                    {
                        string[] words = s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        list.Add(new AppName(words[0], words[1], true));
                    }

                }
                //end fix
                return list;
            }
            else
            {
                return null;
            }
        }


        //GET ACTIVITIES

        //GRANT PERMISSION
        private void GrantPermission(string packageName,string type)
        {
            string[] permissionsList = getPermissionList(packageName);
            if (permissionsList != null)
            {

                string adbCommand = @"/c adb -s " + device.serial + " shell \"";
                int count = 0;
                foreach (string line in permissionsList)
                {
                    try
                    {
                        count++;
                        if (count % 50 == 0 || count == permissionsList.Count())
                        {
                            adbCommand += "\"";

                            Process p1 = new Process();

                            p1.StartInfo.UseShellExecute = false;
                            p1.StartInfo.RedirectStandardOutput = true;
                            p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                            p1.StartInfo.Arguments = adbCommand;


                            printLog(adbCommand, "info");

                            p1.StartInfo.CreateNoWindow = true;
                            p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            p1.Start();

                            string output = p1.StandardOutput.ReadToEnd();
                            //if (output.Contains("Bad argument"))
                            //{
                            //    p1.Dispose();
                            //    printLog("[ERROR] Cant gant permisson for: " + rs[1], "error");
                            //    continue;
                            //}

                            p1.WaitForExit();

                            adbCommand = @"/c adb -s " + device.serial + " shell \"";
                            continue;

                        }

                        string[] rs = line.Split(new string[] { ";" }, StringSplitOptions.None);
                        adbCommand+="pm " + type + " " + rs[0] + " " + rs[1]+";";
                       


                    }
                    catch (Exception ex)
                    {

                        printLog("[ERROR] " + ex.Message, "error");
                    }

                }
                
                //here
            }
        }


        private string[] getActivityList(string package_name)
        {
            try
            {
                string path = txtWS.Text + "\\list_activity.txt";

                if (File.Exists(path))
                {
                    File.Delete(path);
                    if (File.Exists(path))
                    {
                        printLog("Do no existed this pah", "info");
                    }
                    else
                    {
                        printLog("Delete File Ok", "info");
                    }
                    var myfile = File.Create(path);
                    myfile.Close();

                }
                else
                {
                    var myfile = File.Create(path);
                    myfile.Close();
                    printLog("Create File Ok", "info");
                }


                Process p1 = new Process();
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                p1.StartInfo.Arguments = @"/c adb -s " + device.serial + " shell am start -n leakacitivity.svmc.com.checkingactivity/leakacitivity.svmc.com.checkingactivity.Get_List_Activity -e mode n -e packagename " + package_name;

                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p1.Start();
                string output = p1.StandardOutput.ReadToEnd();
                if (output.Contains("Error") == true)
                {
                    if (device.serial != "")
                    {
                        //GlobalFunction.ShowNotification("Leak Activity Notification", "This is First Time.Please wait some time to install Apk service for device", 4);
                        //install_apk_leak_activity();
                        init_install_apk();
                    }
                    p1.Close();
                    p1.Start();
                }
                p1.WaitForExit();


                System.Threading.Thread.Sleep(1500);
                //string arg = "/c adb pull //data//data//getpackages.kidpro1412.com.kidpro1412_getpackages//files//list_activity.txt " + path+" & "+"adb pull //data//data//getpackages.kidpro1412.com.kidpro1412_getpackages//files//list_activity.txt " + path;
                //string arg = "/c adb pull //sdcard//svmc_checking_activity//svmc_list_activity.txt " + path + " & " + "adb pull //sdcard//svmc_checking_activity//svmc_list_activity.txt \"" + path + "\"";
                string arg = "/c adb -s " + device.serial + " pull //sdcard//svmc_checking_activity//svmc_list_activity.txt " + path + " & " + "adb -s " + device.serial + " pull //sdcard//svmc_checking_activity//svmc_list_activity.txt \"" + path + "\"";
                //MessageBox.Show("Test");

                printLog(@arg, "info");


                Process p = new Process();
                //MessageBox.Show(serial.ToString());
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                p.StartInfo.Arguments = @arg;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();





                p.WaitForExit();
                    

                string[] lines = File.ReadAllLines(@path);
                return lines;
            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
                return null;
            }
        }

        private string[] getPermissionList(string packageName)
        {
            try
            {
                if (device.serial == null || device.serial == "")
                {
                    return null;
                }



                string path = txtWS.Text + "\\list_permission.txt";

                if (File.Exists(path))
                {
                    File.Delete(path);
                    if (File.Exists(path))
                    {
                        printLog("Cannot delete file", "info");
                    }
                    else
                    {
                        printLog("Delete File Ok", "info");
                    }
                    var myfile = File.Create(path);
                    myfile.Close();

                }
                else
                {
                    var myfile = File.Create(path);
                    myfile.Close();
                    printLog("Create File Ok", "info");
                }
                init_install_apk();
                Process p1 = new Process();
                p1.StartInfo.UseShellExecute = false;
                p1.StartInfo.RedirectStandardOutput = true;
                p1.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                p1.StartInfo.Arguments = @"/c adb -s " + device.serial + " shell am start -n leakacitivity.svmc.com.checkingactivity/leakacitivity.svmc.com.checkingactivity.Get_List_Permission -e mode a -e packagename " + packageName;
                p1.StartInfo.CreateNoWindow = true;
                p1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p1.Start();

                string output = p1.StandardOutput.ReadToEnd();
                if (output.Contains("Error") == true)
                {
                    if (device.serial != "")
                    {
                        //GlobalFunction.ShowNotification("Leak Activity Notification", "This is First Time.Please wait some time to install Apk service for device", 4);
                        //install_apk_leak_activity();
                        init_install_apk();
                    }
                    p1.Close();
                    p1.Start();
                }
                p1.WaitForExit();

                System.Threading.Thread.Sleep(2000);
                //string arg = "/c adb pull //data//data//getpackages.kidpro1412.com.kidpro1412_getpackages//files//list_activity.txt " + path+" & "+"adb pull //data//data//getpackages.kidpro1412.com.kidpro1412_getpackages//files//list_activity.txt " + path;
                //string arg = "/c adb pull //sdcard//svmc_checking_activity//svmc_list_permission.txt " + path + " & " + "adb pull //sdcard//svmc_checking_activity//svmc_list_permission.txt \"" + path + "\"";
                string arg = "/c adb -s " + device.serial + " pull //sdcard//svmc_checking_activity//svmc_list_permission.txt " + path + " & " + "adb -s " + device.serial + " pull //sdcard//svmc_checking_activity//svmc_list_permission.txt \"" + path + "\"";
                //MessageBox.Show("Test");

                printLog(@arg, "info");


                Process p = new Process();
                //MessageBox.Show(serial.ToString());
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                p.StartInfo.Arguments = @arg;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                p.WaitForExit();
                //p1.WaitForExit();
                //string output = p.StandardOutput.ReadToEnd();
                string[] lines = File.ReadAllLines(@path);

                //File.Copy(@folder_path + "//list_permission.txt", @base_rap + "//list_permission.txt", true);

                //MessageBox.Show(output);
                return lines;
            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
                return null;
            }

        }
        private bool isInContent(string content, string screenName)
        {

            string[] lines = TalkBackAutoTest.Properties.Settings.Default.blacklist_screen.Split(
                 new string[] { "\r\n", "\r", "\n" },
                 StringSplitOptions.RemoveEmptyEntries
             );

            foreach(string line in lines)
            {
                if (line.Trim() == screenName)
                {
                    return true;
                }
            }

            return false;
        }
        private void addScreenToBlacklistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvResult.SelectedItems.Count > 0)
                {
                    string content = TalkBackAutoTest.Properties.Settings.Default.blacklist_screen;
                    foreach (ListViewItem item in lvResult.SelectedItems)
                    {
                        string screenName = item.SubItems[5].Text.ToString();
                        //TalkBackAutoTest.Properties.Settings.Default.txtWS = txtWS.Text;
                        //TalkBackAutoTest.Properties.Settings.Default.Save();                  
                        if(!isInContent(content,screenName))
                        {
                            if (content == "")
                            {
                                content += screenName.Trim();
                            }
                            else
                            {
                                content += "\r\n" + screenName.Trim();
                            }
                        }
                        
                    }
                    txtList_screen.Text = content;
                    TalkBackAutoTest.Properties.Settings.Default.blacklist_screen = content;
                    TalkBackAutoTest.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                printLog(ex.Message, "error");
            }
        }

        private void txtList_screen_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TalkBackAutoTest.Properties.Settings.Default.blacklist_screen = txtList_screen.Text;
                TalkBackAutoTest.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Try Catch: " + ex.Message);
                printLog("txtList_screen_TextChanged: " + ex.Message, "error");
                //}
            }
        }

        private void txtSearchPkg_TextChanged(object sender, EventArgs e)
        {
            string changedText = txtSearchPkg.Text;
            updatecomboBoxPkgSearch();

        }

        private void btnEnterKey_Click(object sender, EventArgs e)
        {
            SendKeyevent("66");
        }

        private void SendKeyevent(string ketevent)
        {
            if (device.serial != null)
            {
                RunCommand("adb -s " + device.serial + " shell input keyevent " + ketevent, 0);
            }
        }

        private void btnEsc_Click(object sender, EventArgs e)
        {
            SendKeyevent("111");
        }

        private void bntBack_Click(object sender, EventArgs e)
        {
            SendKeyevent("4");
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            SendKeyevent("67");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SendKeyevent("84");
        }

        private void rbAndroidMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAndroidMode.Checked == true)
            {
                TalkBackAutoTest.Properties.Settings.Default.envMode = 1;//android mode
                TalkBackAutoTest.Properties.Settings.Default.Save();
                SwapMode(1);
            }
        }

        private void rbWindowMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbWindowMode.Checked == true)
            {
                TalkBackAutoTest.Properties.Settings.Default.envMode = 2;//android mode
                TalkBackAutoTest.Properties.Settings.Default.Save();
                SwapMode(2);
            }
        }

        private void SwapMode(int index)
        {
            if (index == 1)
            {
                if (!tabControl1.TabPages.Contains(tabPageAndrodiManual))
                {
                    tabControl1.TabPages.Add(tabPageAndrodiManual);
                }
                if (!tabControl1.TabPages.Contains(tabPageAndroidSetting))
                {
                    tabControl1.TabPages.Add(tabPageAndroidSetting);
                }
                if (tabControl1.TabPages.Contains(tabPageWindowApp))
                {
                    tabControl1.TabPages.Remove(tabPageWindowApp);
                }

                lvResult.ContextMenuStrip = contextMenuStrip1;
            }
            else if (index == 2)//wwin
            {
                if (tabControl1.TabPages.Contains(tabPageAndrodiManual))
                {
                    tabControl1.TabPages.Remove(tabPageAndrodiManual);
                }
                if (tabControl1.TabPages.Contains(tabPageAndroidSetting))
                {
                    tabControl1.TabPages.Remove(tabPageAndroidSetting);
                }
                if (!tabControl1.TabPages.Contains(tabPageWindowApp))
                {
                    tabControl1.TabPages.Add(tabPageWindowApp);
                }

                lvResult.ContextMenuStrip = contextMenuStrip2;

            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PO5 Team ^^");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PO5 + Window Team ^^");
        }
        

        
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TalkBackAutoTest
{
    class TBHelper
    {
        static HashSet<string> englishWords;
        static string ENG_LIST_PATH = Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\en_word_list.txt";

        private Tuple<Boolean,String> checkviFull(string text)
        {
            //var result = DetectAbbreviationsNonVietnamese(text);
            //string viettat = "Từ viết tắt: " + string.Join(", ", result.Item1);
            //string nonvi = "Từ không phải tiếng Việt: " + string.Join(", ", result.Item2);
            //return viettat + "\r\n" + nonvi;
            var result = DetectAbbreviationsNonVietnamese(text);
            List<string> list1 = result.Item1;
            List<string> list2 = result.Item2;
            if (list1.Count == 0 && list2.Count == 0)//ko co thang nao
            {
                return Tuple.Create(true,"");
            }
            else
            {
                string s = string.Join(", ", result.Item1);
                s = string.Join(", ", result.Item1);
                return Tuple.Create(false, s);
            }
        }

        public static Tuple<Boolean, String> checkEngFull(string inputText)
        {
            // Đọc danh sách từ tiếng Anh từ file
            LoadEnglishWords(@ENG_LIST_PATH);

            // Chuỗi cần kiểm tra
            //string inputText = "Hôm nay tôi đến công ty để gặp CEO của tập đoàn IT.";
            //string inputText = text;
            //string inputText = "Hello everyone chào nhé.";

            // Kiểm tra từ không phải tiếng Anh
            List<string> nonEnglishWords = DetectNonEnglishWords(inputText);

            // Hiển thị kết quả
            //Console.WriteLine("Các từ không phải tiếng Anh:");
            //Console.WriteLine(string.Join(", ", nonEnglishWords));
            if (nonEnglishWords.Count == 0)
            {
                return Tuple.Create(true, "");
            }
            else
            {
                return Tuple.Create(false, nonEnglishWords.ToString());
            }
        }

        static void LoadEnglishWords(string filePath)
        {
            try
            {
                englishWords = new HashSet<string>(File.ReadAllLines(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tải danh sách từ: " + ex.Message);
                englishWords = new HashSet<string>();
            }
        }

        static List<string> DetectNonEnglishWords(string text)
        {
            // Tách từ bằng khoảng trắng, loại bỏ dấu câu
            char[] delimiters = new char[] { ' ', ',', '.', '!', '?', ';', ':', '(', ')', '[', ']', '"', '\'' };
            string[] words = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            //List<string> result = new List<string>();
            //foreach (string word in words)
            //{
            //    if(!englishWords.Contains(word.ToLower()))
            //    {
            //        result.Add(word);
            //    }
            //}
            //return result;
            // Kiểm tra từ nào không có trong danh sách từ tiếng Anh
            return words.Where(word => !englishWords.Contains(word.ToLower())).ToList();
        }



        

        private Tuple<List<string>, List<string>> DetectAbbreviationsNonVietnamese(string text)
        {
            // Regex nhận diện từ viết tắt (viết hoa toàn bộ, có thể có số)
            var abbreviations = Regex.Matches(text, @"\b[A-Z0-9]{2,}\b")
                                     .Cast<Match>()
                                     .Select(m => m.Value)
                                     .ToList();

            // Danh sách ký tự tiếng Việt có dấu
            string vietnameseChars = "áàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđ" +
                                     "ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬÉÈẺẼẸÊẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÚÙỦŨỤƯỨỪỬỮỰÝỲỶỸỴĐ";

            // Tách từ và kiểm tra từ không chứa ký tự tiếng Việt nào
            string[] words = text.Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var nonVietnameseWords = words.Where(word => !word.Any(c => vietnameseChars.Contains(c))).ToList();

            return Tuple.Create(abbreviations, nonVietnameseWords);
        }
    }
}

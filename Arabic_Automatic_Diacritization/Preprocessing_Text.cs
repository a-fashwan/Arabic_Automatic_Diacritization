using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.Mail;

namespace Arabic_Automatic_Diacritization
{
    class Preprocessing_Text
    {
        #region variables and constructor
        bool Is_Email = false;
        bool Is_URL = false;
        int Check_int = 0;
        float Check_float = 0.0F;
        bool is_Negativenumber = false;
        #endregion

        public Preprocessing_Text()
        {
        }
        public void Preproccess_Raw_Text(string Raw_Text, out string Proccessed_Text)
        {

            string[] InvalidChars = { "/", "'", "\'", "...", "\"", "#", "&", "$", "%", "?", "؟", "=", "+", "«", "»", "%", "_", "^", "*", "•", "`", ".", "‘", "’", ";", "؛", ":", "»", "«", "”", "“", "{", "}", "[", "]", "\\", "|", ">", "<", "(", ")", "!", ".", "،", ",", "-", "—", "¯", "…" };
            
            # region Before_Spliting_Text
            foreach (string Value in InvalidChars)
            {
                Is_URL = false;
                Is_URL = Regex.IsMatch(Raw_Text, @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$", RegexOptions.IgnoreCase);
                Is_Email = false;
                Is_Email = Regex.IsMatch(Raw_Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                if (Raw_Text.Contains(Value) && Value != "-" && Value != "," && Value != "\'" && Value != "..." && Value != "." && Value != "،" && Is_URL == false && Is_Email == false)
                {
                    Raw_Text = Raw_Text.Replace(Value, " " + Value + " ");
                }
                else
                    if (Raw_Text.Contains(Value) && Value == "\'")
                    {
                        Raw_Text = Raw_Text.Replace(Value, "\"");
                    }
            }
            #endregion

            #region Spliting_Text_By_Space
            string Text_Value = Raw_Text; Text_Value = Text_Value.Replace("  ", " "); string[] Words_List = Text_Value.Split(' '); Text_Value = ""; int i = 0;
            foreach (string Each_Word in Words_List)
            {
                string Word = Each_Word;
                string [] Removing = Word.Split(char.Parse(" "));
                string [] Cleared_Word = RemoveHiddenChars(Removing);
                Word = Cleared_Word[0];
                Is_URL = false;
                Is_URL = Regex.IsMatch(Word, @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$", RegexOptions.IgnoreCase);
                Is_Email = false;
                Is_Email = Regex.IsMatch(Word, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                is_Negativenumber = false;
                is_Negativenumber = Regex.IsMatch(Word, @"(?<=\s|^)-?[0-9]+(?:\.[0-9]+)?(?=\s|$)");

                if ((Word.Contains("/") || Word.Contains("-") || Word.Contains(".") || Word.Contains("،") || Word.Contains(",") || Word.Contains(":")) && Is_Email == false && Is_URL == false && is_Negativenumber == false && !(int.TryParse(Word, out Check_int) || float.TryParse(Word, out Check_float)))
                {
                    Word = Word.Replace("-", " - ");
                    Word = Word.Replace(".", " . ");
                    Word = Word.Replace("،", " ، ");
                    Word = Word.Replace(",", " , ");
                }
                
                #region Removing_Diacritics
                Word = Word.Replace("ّ", "");  // الشده
                Word = Word.Replace("ْ", "");  // السكون
                Word = Word.Replace("ٌ", "");  //  تنوين بالضمة
                Word = Word.Replace("ً", "");  // تنوين بالفتحه
                Word = Word.Replace(" ً", "");   //  تنوين بالفتحه with hidden characters 
                Word = Word.Replace("ٍ", "");  // تنوين بالكسرة
                Word = Word.Replace("ُ", "");  // الضمة 
                Word = Word.Replace("ِ", "");  // الكسرة
                Word = Word.Replace("َ", "");  // الفتحه
                #endregion

                Word = Word.Replace("  ", " ");
                Word = Word.Replace(". .", "..");
                Word = Word.Replace("- -", "--");
                Word = Word.Replace("… …", "……");
                Word = Word.Replace("\t", " ");
                
                #region Word/Number or Number/Word
                Is_Email = false;
                Is_Email = Regex.IsMatch(Word, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                Is_URL = false;
                Is_URL = Is_UrlValid(Word);
                is_Negativenumber = false;
                is_Negativenumber = Regex.IsMatch(Word, @"(?<=\s|^)-?[0-9]+(?:\.[0-9]+)?(?=\s|$)");
                decimal[] substrings = intRemover(Word);
                if (Word != "" && char.IsNumber(Word[0]) && !char.IsNumber(Word[Word.Length - 1]) && Is_Email == false && Is_URL == false && is_Negativenumber == false)  // Ex: number+word
                {
                    if (substrings.Count() > 0) { Word = Word.Replace(substrings[0].ToString(), substrings[0] + " "); }
                }
                if (Word!= "" && !char.IsNumber(Word[0]) && char.IsNumber(Word[Word.Length - 1]) && Is_Email == false && Is_URL == false && is_Negativenumber == false)  // Ex: word+number
                {
                    if (substrings.Count() > 0) { Word = Word.Replace(substrings[0].ToString(), " " + substrings[0]); }
                }
                #endregion

                #region English & Arabic words attached together in writing mistakes
                if (Regex.IsMatch(Word, @"[a-zA-Z]") && Regex.IsMatch(Word, @"[ء-ي]"))
                {
                    char[] test_array = Word.ToCharArray();
                    if (Regex.IsMatch(test_array[0].ToString(), @"[a-zA-Z]"))
                    {
                        string first_arabic_letter = "";
                        foreach (var item in test_array)
                        {
                            if (!Regex.IsMatch(item.ToString(), @"[a-zA-Z]"))
                            {
                                first_arabic_letter = item.ToString();
                                break;
                            }
                        }
                        Word = ReplaceFirstOccurrance(Word, first_arabic_letter, " " + first_arabic_letter);
                    }
                    else
                    {
                        if (Regex.IsMatch(test_array[0].ToString(), @"[ء-ي]"))
                        {
                            string first_english_letter = "";
                            foreach (var item in test_array)
                            {
                                if (!Regex.IsMatch(item.ToString(), @"[ء-ي]"))
                                {
                                    first_english_letter = item.ToString();
                                    break;
                                }
                            }
                            Word = ReplaceFirstOccurrance(Word, first_english_letter, " " + first_english_letter);
                        }
                    }
                }
                #endregion

                #region Kasheda Problem in the middle of the word
                if (Word.Contains("ـ") && Word != "ـ" && !Word.Contains("ـ ") && !Word.Contains(" ـ") && Word.Substring(0, 1) != "ـ" && Word.Substring(Word.Length - 1) != "ـ") // (Shift + ت)
                {
                    Word = Word.Replace("ـ", "");
                }
                #endregion

                i++;
                if (i == Words_List.Count())
                {
                    Text_Value = Text_Value + Word;
                }
                else
                {
                    Text_Value = Text_Value + Word + " ";
                }
            }
            #endregion

            #region After_Spliting_Text
            Text_Value = Text_Value.Replace("= =", "==");
            Text_Value = Text_Value.Replace("- -", "--");
            Text_Value = Text_Value.Replace("… …", "……");
            Text_Value = Text_Value.Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Replace(". .", "..").Replace("= =", "==").Replace("… …", "……");
            Text_Value = Text_Value.Replace("\n \n", " \r \r ");
            Text_Value = Text_Value.Replace("\r \r", " \rS/\r\r/S\r ");
            Text_Value = Text_Value.Replace("\n\n", " \rS/\r/S\r ");
            Text_Value = Text_Value.Replace("\n"," \nS/\n/S\n ");
            Text_Value ="/D\n/S\n " + Text_Value + " \nS/\nD/";
            Proccessed_Text = Text_Value;
            return;
            #endregion
        }

        private bool Is_UrlValid(string url)
        {
            string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }

        public static decimal[] intRemover(string input)
        {
            return Regex.Matches(input, @"[+-]?\d+(\.\d+)?")//this returns all the matches in input
                        .Cast<Match>()//this casts from MatchCollection to IEnumerable<Match>
                        .Select(x => decimal.Parse(x.Value))//this parses each of the matched string to decimal
                        .ToArray();//this converts IEnumerable<decimal> to an Array of decimal
        }

        public string ReplaceFirstOccurrance(string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;
            int loc = original.IndexOf(oldValue);
            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        private string[] RemoveHiddenChars(string[] Cleared_Text)
        {
           //int gettgett = (char)'‪';
            char[] trim = { (char)13, (char)28, (char)29, (char)30, (char)31, (char)157, (char)158,
            (char)160, (char)253, (char)254, (char)240, (char)241, (char)242, (char)243, (char)245, (char)246, (char)248,
            (char)250, (char)1611, (char)1612, (char)1613, (char)1614, (char)1616, (char)1615, (char)1617, (char)1618, 
            (char)15,(char)3,(char)17,(char)22,(char)27,(char)2,(char)26,
            (char)1,(char)4,(char)21,(char)16,(char)14,(char)8204,(char)8234,(char)8205,(char)8206,(char)8207,}; //you can put here your chars (char)10, 
            int pos;
            for (int i = 0; i < Cleared_Text.Length; i++)
            {

                while ((pos = Cleared_Text[i].ToString().IndexOfAny(trim)) >= 0)
                {
                    Cleared_Text[i] = Cleared_Text[i].Remove(pos, 1);
                }
            }
            return Cleared_Text;
        }

    }
}

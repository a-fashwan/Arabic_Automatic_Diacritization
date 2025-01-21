using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Arabic_Automatic_Diacritization
{
    class Editing_Stage
    {
        public Editing_Stage()
        {
        }
        public void Editing_Raw_Text(string Proccessed_Text, out string Edited_Text, Form1 obj)
        {
            string Text_Value = Proccessed_Text; string[] Words_List = Text_Value.Split(' '); Text_Value = ""; int i = 0;
            foreach (string Each_Word in Words_List)
            {
                string Word = Each_Word;

                if (Regex.IsMatch(Word, @"[ء-ي]"))
                {
                    if (Word == "الخ")
                    {
                        Word = Word.Replace("الخ", "إلخ");
                    }
                    #region "أبو"
                    if (Word.StartsWith("أبو") || Word.StartsWith("ابو"))
                    {
                        Word = Word.Replace("أبوال", "أبو ال"); // ابو , أبو 
                        Word = Word.Replace("ابوال", "ابو ال"); // ابو , أبو 
                        Word = Word.Replace("بوردين", "بو ردين");// أبوردينه , ابوردينه , أبوردينة , ابوردينة
                        Word = Word.Replace("بومازن", "بو مازن"); // أبومازن , ابومازن
                        Word = Word.Replace("بوظب", "بو ظب");   //ابو ظبي , أبو ظبي , ابوظبى , أبوظبى
                        Word = Word.Replace("بوتريك", "بو تريك");  //  ابوتريكه , أبوتريكه , أبوتريكة , ابوتريكة 
                        Word = Word.Replace("بوزيد", "بو زيد"); // ابو , أبو 
                        Word = Word.Replace("بوعلم", "بو علم"); // ابو , أبو
                        Word = Word.Replace("بوذكر", "بو ذكر");   // أبوذكرى
                        Word = Word.Replace("بوهرير", "بو هرير"); // ابوهريرة
                        Word = Word.Replace("بوجعفر", "بو جعفر");
                        Word = Word.Replace("بوداود", "بو داود");
                        Word = Word.Replace("بوبكر", "بو بكر");
                        Word = Word.Replace("بوعبد", "بو عبد");
                        Word = Word.Replace("بوعمر", "بو عمر");
                        Word = Word.Replace("بوكرم", "بو كرم");
                        Word = Word.Replace("بوعثمان", "بو عثمان");
                    }
                    #endregion

                    #region Random_Words
                    if (!Word.Contains("انعزال"))
                    {
                        Word = Word.Replace("عزال", "عز ال");
                    }
                    Word = Word.Replace("اوال", "او ال");
                    Word = Word.Replace("أوال", "أو ال");
                    if (Word.StartsWith("هو"))
                    {
                        Word = Word.Replace("هوال", "هو ال");
                        Word = Word.Replace("هوحت", "هو حت");  // هو حتى , هو حتي 
                    }
                    Word = Word.Replace("فيما", "في ما");
                    Word = Word.Replace("احداهم", "أحد أهم");
                    Word = Word.Replace("جيجاطن", "جيجا طن");
                    //Word = Word.Replace("كيلومتر", "كيلو متر");
                    Word = Word.Replace("عبدال", "عبد ال");
                    Word = Word.Replace("فخرال", "فخر ال");  // فخرالدين 
                    Word = Word.Replace("جنكيزخان", "جنكيز خان");
                    Word = Word.Replace("بنبرك", "بن برك"); // بن بركه , بن بركة                       
                    Word = Word.Replace("لاتفعل", "لا تفعل");
                    Word = Word.Replace("نصرالله", "نصر الله");
                    Word = Word.Replace("بورسعيد", "بور سعيد");
                    if (Word.Equals("لابد") || Word.Equals("ولابد") || Word.Equals("فلابد"))
                    {
                        Word = Word.Replace("لابد", "لا بد");
                        Word = Word.Replace("ولابد", "ولا بد");
                        Word = Word.Replace("فلابد", "فلا بد");
                    }
                    #endregion
                }

                #region "يا"
                if (Word.Contains("يا"))
                {
                    Word = Word.Replace("يالها", "يا لها");
                    Word = Word.Replace("ياريت", "يا ريت");
                    Word = Word.Replace("يامهلبية", "يا مهلبية");
                    Word = Word.Replace("يارب", "يا رب");
                    Word = Word.Replace("ياالله", "يا الله");
                }
                #endregion

                #region "أا" - "اأ" - "اا" ... etc.
                if ((Word.Contains("أا") && !Word.StartsWith("أا") && !Word.EndsWith("أا")))
                {
                    Word = Word.Replace("أا", "أ ا");
                }
                if (Word.Contains("اأ") && !Word.StartsWith("اأ") && !Word.EndsWith("اأ"))
                {
                    Word = Word.Replace("اأ", "ا أ");
                }
                if (Word.Contains("اا") && !Word.StartsWith("اا") && !Word.EndsWith("اا"))
                {
                    Word = Word.Replace("اا", "ا ا");
                }
                if (Word.Contains("اإ") && !Word.StartsWith("اإ") && !Word.EndsWith("اإ"))
                {
                    Word = Word.Replace("اإ", "ا إ");
                }
                if (Word.Contains("إا") && !Word.StartsWith("إا") && !Word.EndsWith("إا"))
                {
                    Word = Word.Replace("إا", "إ ا");
                }
                #endregion

                #region " ة , ى " Within The Word
                if (Word.Contains("ة") && Word.IndexOf("ة") != Word.Length - 1)
                {
                    Word = Word.Replace("ة", "ة ");
                }
                if (!Word.StartsWith("ى") && !Word.EndsWith("ى") && Word.Contains("ى"))
                {
                    Word = Word.Replace("ى", "ى ");
                    if (Word.EndsWith("ى ء"))
                    {
                        Word = Word.Replace("ى ء", "ىء");
                    }
                }
                #endregion

                #region  "لا شيء"
                if (Word.EndsWith("لاشئ") || Word.EndsWith("لاشيء"))
                {
                    Word = Word.Replace("لاشئ", "لا شيء");
                    Word = Word.Replace("لاشيء", "لا شيء");
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

            Edited_Text = Text_Value;
            return;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace Arabic_Automatic_Diacritization
{
    class Morphological_Analysis_Stage
    {
        #region Variabls and Constructors
        public string Rule_Number;
        public int IDs;
        public class Best_Solution_List
        {
            public string word { set; get; }
            public string lemmaID { set; get; }
            public string stem { set; get; }
            public string pr { set; get; }
            public string suf { set; get; }
            public string spattern { set; get; }
            public string affectedBy { set; get; }
            public double Prop { set; get; }
        }

        public List<Analyzed_Text> Text_Analysis = new List<Analyzed_Text>();
        public List<Text_Solutions> Text_Solutions = new List<Text_Solutions>();
        public List<Best_Solution_List> For_Best_Solution_Selection_List = new List<Best_Solution_List>();
        public static string buckWalter = "Diac_Bucwalter\\";
        List<string> Text_Solutions_Values = new List<string>();
        public List<Text_Solutions> Text_Solutions_List = new List<Text_Solutions>();
        public AraMorph_output Diac_Output = new AraMorph_output();
        public XmlDocument Diac_Output_Table = new XmlDocument();
        #endregion

        public void Get_Morphologically_Analyzed_Text(string Edited_Text, List<Text_Solutions> Text_Solutions, out List<Analyzed_Text> Analyzed_Text, Form1 obj)
        {
            #region From_Raw_To_Analyzed_Text
            Text_Analysis.Clear();
            string Text_Value = Edited_Text; string[] Words_List = Text_Value.Split(' '); Text_Value = ""; int w = 0;
            foreach (string Each_Word in Words_List)
            {
                string Word = Each_Word;
                if (Word != "")
                {
                    string ID = (w + 1).ToString();
                    Analyzed_Text Solution_List = new Analyzed_Text();
                    Solution_List.ID = ID;
                    Solution_List.word = Word;
                    Solution_List.lemmaID = "";
                    Solution_List.pr = "";
                    Solution_List.stem = "";
                    Solution_List.suf = "";
                    Solution_List.spattern = "";
                    Solution_List.def = "";
                    Solution_List.ecase = "";
                    Solution_List.affectedBy = "";
                    Text_Analysis.Add(Solution_List);
                    w++;
                }
            }
            #endregion

            #region 1. Non_Arabic_Words_And_Sentences_Boundaries
            var Not_Arabic_Words = Text_Analysis.Where(s => s.pr == "" && s.stem == "" && s.affectedBy == "" && !Regex.IsMatch(s.word, @"[ء-ي]")).ToList();
            for (int r = 0; r < Not_Arabic_Words.Count(); r++)
            {
                if (Not_Arabic_Words[r].word.Contains("/D\n/S\n"))
                {
                    IDs = int.Parse(Not_Arabic_Words[r].ID) - 1;
                    Text_Analysis[IDs].stem = "<s>";
                    Text_Analysis[IDs].affectedBy = "sent_boundry";
                }
                else
                {
                    if (Not_Arabic_Words[r].word.Contains("\nS/\nD/"))
                    {
                        IDs = int.Parse(Not_Arabic_Words[r].ID) - 1;
                        Text_Analysis[IDs].stem = "</s>";
                        Text_Analysis[IDs].affectedBy = "sent_boundry";
                    }
                    else
                    {
                        if (Not_Arabic_Words[r].word.Contains("\nS/\n/S\n") || Not_Arabic_Words[r].word.Contains("\rS/\r/S\r") || Not_Arabic_Words[r].word.Contains("\rS/\r\r/S\r"))
                        {
                            IDs = int.Parse(Not_Arabic_Words[r].ID) - 1;
                            Text_Analysis[IDs].stem = "</s> <s>";
                            Text_Analysis[IDs].affectedBy = "sent_boundry";
                        }
                        else
                        {
                            IDs = int.Parse(Not_Arabic_Words[r].ID) - 1;
                            Text_Analysis[IDs].stem = "e";
                            Text_Analysis[IDs].affectedBy = "not_Arabic";
                        }
                    }
                }
            }
            #endregion

            #region 2. Words_That_Have_One_Solution_Or_No_Solutions
            var Row_Solutions = Text_Analysis.Where(s => s.pr == "" && s.stem == "" && Regex.IsMatch(s.word, @"[ء-ي]")).ToList();
            for (int r = 0; r < Row_Solutions.Count(); r++)
            {
                //var Solutions = Text_Solutions.Where(p => p.word.Any(i => word_Count.Any(w => i.))
                //update o set o.Name='NewName'
                //from Organization o
                //Inner join Guardian g on o.OrgRowId=g.OrgRowId
                //where g.IsEnabled=1 and g.OrgRowId=1 
                string new_word = Row_Solutions[r].word;
                var word_Count = Text_Solutions.GroupBy(a => new { words = a.word}).Select(s => new { s.Key.words, NumResults = s.Count() }).Where (s => s.NumResults.Equals(1)).ToList();
                var solutions_list = (from o in Text_Solutions
                                            join g in word_Count on o.word equals g.words
                                            select new
                                            {
                                                o.word,
                                                o.lemmaID,
                                                o.pr,
                                                o.stem,
                                                o.suf,
                                                o.spattern,
                                                o.affectedBy
                                            }).ToList();

                foreach (var item in solutions_list)
                {
                    new_word = item.word;
                    var Word_Solutions = Text_Analysis.Select(s => new { s.ID, s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == new_word).ToList();
                    foreach (var sol in Word_Solutions)
                    {
                        IDs = int.Parse(sol.ID) - 1;
                        Text_Analysis[IDs].lemmaID = item.lemmaID;
                        Text_Analysis[IDs].pr = item.pr;
                        Text_Analysis[IDs].stem = item.stem;
                        Text_Analysis[IDs].suf = item.suf;
                        Text_Analysis[IDs].spattern = item.spattern;
                        Text_Analysis[IDs].affectedBy = item.affectedBy + "_one";
                    }
                }
            }
            #endregion

            #region 3. Words_That_Have_Rules_For_Disambiguiation_Process [Context_Based_Level (Vertical_Rules)]

            var Empty_POS = Text_Analysis.Where(s => s.pr == "" && s.stem == "" && Regex.IsMatch(s.word, @"[ء-ي]")).ToList();
            for (int e = 0; e < Empty_POS.Count(); e++)
            {
                #region 3.1 Analyzing_Word_Depending_On_Surrounding_Words_Or_Tags
                For_Best_Solution_Selection_List.Clear();
                var Previous_Word_Or_Tage = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(Empty_POS[e].ID) - 1).ToList();
                var Next_Stem = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(Empty_POS[e].ID) + 1).ToList();
                var Next_Next_Stem = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(Empty_POS[e].ID) + 2).ToList();
                
                #region Rule: 1.1
                if (Empty_POS[e].word == "أن" || Empty_POS[e].word == "وأن" || Empty_POS[e].word == "بأن" || Empty_POS[e].word == "وبأن" || Empty_POS[e].word == "لأن" || Empty_POS[e].word == "ولأن" || Empty_POS[e].word == "فأن" ||
                    Empty_POS[e].word == "ان" || Empty_POS[e].word == "وان" || Empty_POS[e].word == "بان" || Empty_POS[e].word == "وبان" || Empty_POS[e].word == "لان" || Empty_POS[e].word == "ولان" || Empty_POS[e].word == "فان")
                {
                    if (Next_Stem[0].stem.Contains("PV") || Next_Stem[0].stem.Contains("IV"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains(">an/CONJ"))).ToList();
                        if (True_Solutions.Count() == 1)
                        {
                            IDs = int.Parse(Empty_POS[e].ID) - 1;
                            Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                            Text_Analysis[IDs].pr = True_Solutions[0].pr;
                            Text_Analysis[IDs].stem = True_Solutions[0].stem;
                            Text_Analysis[IDs].suf = True_Solutions[0].suf;
                            Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                            Text_Analysis[IDs].spattern = True_Solutions[0].affectedBy;
                            Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.1.1";
                        }
                    }
                    var Distinct = Text_Solutions.Select(s => new { s.word, s.stem}).Distinct().Where(a => a.word == Next_Stem[0].word).ToList();
                    string Tags = "True";
                    Distinct.ToList().ForEach(u =>
                    {
                        var Dis_Tags = u.stem.Split('/')[1];
                        if (!Dis_Tags.Contains("IV") && !Dis_Tags.Contains("PV"))
                        {
                            Tags = "False";
                        }
                    });
                    if (Tags.Equals("True"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains(">an/CONJ"))).ToList();
                        if (True_Solutions.Count() == 1)
                        {
                            IDs = int.Parse(Empty_POS[e].ID) - 1;
                            Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                            Text_Analysis[IDs].pr = True_Solutions[0].pr;
                            Text_Analysis[IDs].stem = True_Solutions[0].stem;
                            Text_Analysis[IDs].suf = True_Solutions[0].suf;
                            Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                            Text_Analysis[IDs].spattern = True_Solutions[0].affectedBy;
                            Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.1.2";
                        }
                    }
                }
                #endregion

                #region Rule: 1.2
                else if ((Next_Stem[0].stem.Contains("PV") || Next_Stem[0].stem.Contains("IV")) && (Empty_POS[e].word == "إن" || Empty_POS[e].word == "وإن" || Empty_POS[e].word == "فإن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("<in/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.2";
                    }

                }
                #endregion

                #region Rule: 1.3
                else if ((Next_Stem[0].stem.Contains("NOUN")) && (Empty_POS[e].word == "أن" || Empty_POS[e].word == "وأن" || Empty_POS[e].word == "بأن" || Empty_POS[e].word == "وبأن" || Empty_POS[e].word == "لأن" || Empty_POS[e].word == "ولأن" || Empty_POS[e].word == "فأن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains(">an~a/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.3";
                    }
                }
                #endregion

                #region Rule: 1.4
                else if ((Next_Stem[0].stem.Contains("NOUN")) && (Empty_POS[e].word == "إن" || Empty_POS[e].word == "وإن" || Empty_POS[e].word == "فإن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("<in~a/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.4";
                    }
                }
                #endregion

                #region Rule: 1.5
                else if ((Next_Stem[0].stem.Contains("PV") || Next_Stem[0].stem.Contains("IV") || Next_Stem[0].lemmaID.Equals(">ayoD")) && (Empty_POS[e].word == "لكن" || Empty_POS[e].word == "ولكن" || Empty_POS[e].word == "فلكن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("l`kin/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.5";
                    }
                }
                #endregion

                #region Rule: 1.6
                else if ((Next_Stem[0].stem.Contains("NOUN") && !Next_Stem[0].lemmaID.Equals(">ayoD")) && (Empty_POS[e].word == "لكن" || Empty_POS[e].word == "ولكن" || Empty_POS[e].word == "فلكن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("l`kin~a/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.6";
                    }
                }
                #endregion

                #region Rule: 1.7
                else if ((Next_Stem[0].stem.Contains("Hayov/NOUN")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "فمن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "ولمن" || Empty_POS[e].word == "فلمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("min/PREP"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.7";
                    }
                }
                #endregion

                #region Rule: 1.8
                else if ((Next_Stem[0].stem.Contains("PV") || Next_Stem[0].stem.Contains("IV")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "فمن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "ولمن" || Empty_POS[e].word == "فلمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("man/PRON"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.8";
                    }
                }
                #endregion

                #region Rule: 1.9
                else if ((Next_Stem[0].stem.Contains("NOUN")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "فمن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "ولمن" || Empty_POS[e].word == "فلمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("min/PREP"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.9";
                    }
                }
                #endregion

                #region Rule: 1.10
                else if ((Next_Stem[0].stem.Contains("Hayov/NOUN") || Next_Stem[0].stem.Contains("<i*/NOUN")) && (Empty_POS[e].word == "إن" || Empty_POS[e].word == "أن" || Empty_POS[e].word == "ان"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("<in~a/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.10";
                    }
                }
                #endregion

                #region Rule: 1.11
                else if ((Next_Stem[0].stem.Contains("Hayov/NOUN") || Next_Stem[0].stem.Contains("<i*/NOUN")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "أن" || Empty_POS[e].word == "ان"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("<in~a/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.11";
                    }
                }
                #endregion

                #region Rule: 1.12
                else if ((Next_Stem[0].stem.Contains("gayor/PART")) && (Empty_POS[e].word == "أن" || Empty_POS[e].word == "ان"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains(">an~a/CONJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.12";
                    }
                }
                #endregion

                #region Rule: 1.13
                else if ((Next_Stem[0].word.Equals("غير")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "فمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("min/PREP"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.13";
                    }
                }
                #endregion

                #region Rule: 1.14
                else if ((Next_Stem[0].stem.Equals("lA/PART")) && (Next_Stem[0].pr.Equals("Al/DET") || Next_Next_Stem[0].stem.Equals("$ayo'/NOUN")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "فمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("min/PREP"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.14";
                    }
                }

                #endregion

                #region Rule: 1.15
                else if ((Next_Stem[0].stem.Equals("lA/PART") || Next_Stem[0].stem.Equals("lam/PART")) && (Next_Stem[0].pr.Equals("")) && (!Next_Next_Stem[0].stem.Equals("$ayo'/NOUN")) && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "فمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("man/PRON"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.15";
                    }
                }
                #endregion

                #region Rule: 1.16
                else if (Next_Stem[0].stem.Contains("IV") && (Empty_POS[e].word == "لم" || Empty_POS[e].word == "ولم" || Empty_POS[e].word == "فلم"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("lam/PART"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.16";
                    }
                }
                #endregion

                #region Rule: 1.17
                else if ((Empty_POS[e].word == "لم" || Empty_POS[e].word == "ولم" || Empty_POS[e].word == "فلم") && (Empty_POS[e + 1].pr.Equals("") && Empty_POS[e + 1].stem.Equals("")))
                {
                    var Dis_Next_PR = Text_Solutions.Select(s => new { s.word, s.pr }).Distinct().Where(a => a.word == Empty_POS[e + 1].word).ToList();
                    if (Dis_Next_PR.Count() == 1 && Dis_Next_PR[0].pr.Contains("IV"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("lam/PART"))).ToList();
                        if (True_Solutions.Count() == 1)
                        {
                            IDs = int.Parse(Empty_POS[e].ID) - 1;
                            Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                            Text_Analysis[IDs].pr = True_Solutions[0].pr;
                            Text_Analysis[IDs].stem = True_Solutions[0].stem;
                            Text_Analysis[IDs].suf = True_Solutions[0].suf;
                            Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                            Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.17";
                        }
                    }
                }
                #endregion

                #region Rule: 1.18
                else if (!Next_Next_Stem.Count.Equals(0) && !Next_Stem.Count.Equals(0) && (Next_Next_Stem[0].stem.Contains("PV") || Next_Next_Stem[0].stem.Contains("IV")) && Next_Stem[0].word.Equals("لا") && (Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "فمن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "ولمن" || Empty_POS[e].word == "فلمن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("man/PRON"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.18";
                    }
                }
                #endregion

                #region Rule: 1.19
                else if (!Next_Stem.Count.Equals(0) && (Next_Stem[0].word.Equals("أن") || Next_Stem[0].word.Equals("ان")) && (Empty_POS[e].word == "بعد" || Empty_POS[e].word == "وبعد" || Empty_POS[e].word == "فبعد"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("baEod/NOUN"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.19";
                    }
                }
                #endregion

                #region Rule: 1.20
                else if ((Next_Stem[0].stem.Equals("gad/NOUN") && (Empty_POS[e].word == "بعد" || Empty_POS[e].word == "وبعد" || Empty_POS[e].word == "فبعد")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("baEod/NOUN"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.20";
                    }
                }
                #endregion

                #region Rule: 1.21
                else if (Next_Stem[0].word.Equals("الذين") && Empty_POS[e].word.EndsWith("ين"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.suf.Equals("iyna/N_SUF"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.21";
                    }
                }
                #endregion

                #region Rule: 1.22
                else if (Next_Stem[0].word.Equals("الذين") && (Empty_POS[e].word.Equals("هم") || Empty_POS[e].word.Equals("وهم")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.EndsWith("/PRON"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.22";
                    }
                }
                #endregion

                #region Rule: 1.23
                else if (Next_Stem[0].word.Equals("اللذين") && Empty_POS[e].word.EndsWith("ين"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.suf.Equals("ayoni/N_SUF"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.23";
                    }
                }
                #endregion

                #region Rule: 1.24
                else if ((Next_Stem[0].word.Equals("هم") || Next_Stem[0].word.Equals("هؤلاء")) && Empty_POS[e].word.EndsWith("ين"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.suf.Equals("iyna/N_SUF"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.24";
                    }
                }
                #endregion

                #region Rule: 1.25
                else if (Previous_Word_Or_Tage[0].word == "من" || Previous_Word_Or_Tage[0].word == "ومن" || Previous_Word_Or_Tage[0].word == "فمن" || Previous_Word_Or_Tage[0].word == "لمن" || Previous_Word_Or_Tage[0].word == "ولمن")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("ADJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.25";
                    }
                }
                #endregion

                #region Rule: 1.26
                else if (((Empty_POS[e].word.Equals("سنة") || Empty_POS[e].Equals("سنه")) && (Regex.IsMatch(Previous_Word_Or_Tage[0].word, @"^\d+$"))) || ((Empty_POS[e].word.Equals("سنة") || Empty_POS[e].word.Equals("بسنة") || Empty_POS[e].word.Equals("لسنة") || Empty_POS[e].word.Equals("وسنة")) && (Regex.IsMatch(Next_Stem[0].word, @"^\d+$"))))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("san/NOUN") && a.suf.Equals("ap/N_SUF"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.26";
                    }
                }
                #endregion

                #region Rule: 1.27
                else if (((Empty_POS[e].word.Equals("عام") || Empty_POS[e].Equals("عاما")) && (Regex.IsMatch(Previous_Word_Or_Tage[0].word, @"^\d+$"))) || ((Empty_POS[e].word.Equals("عام") || Empty_POS[e].Equals("وعام") || Empty_POS[e].Equals("لعام") || Empty_POS[e].Equals("فعام")) && (Regex.IsMatch(Next_Stem[0].word, @"^\d+$"))))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("EAm/NOUN"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.27";
                    }
                }
                #endregion

                #region Rule: 1.28
                else if (Previous_Word_Or_Tage[0].word.Equals("ان") || Previous_Word_Or_Tage[0].word.Equals("وان") || Previous_Word_Or_Tage[0].word.Equals("بان"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("/ADJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.28";
                    }
                }
                #endregion

                #region Rule: 1.29
                else if (Previous_Word_Or_Tage[0].word.Equals("لكن") || Previous_Word_Or_Tage[0].word.Equals("فلكن") || Previous_Word_Or_Tage[0].word.Equals("ولكن"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("/ADJ"))).ToList();
                    if (True_Solutions.Count() == 1)
                    {
                        IDs = int.Parse(Empty_POS[e].ID) - 1;
                        Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                        Text_Analysis[IDs].pr = True_Solutions[0].pr;
                        Text_Analysis[IDs].stem = True_Solutions[0].stem;
                        Text_Analysis[IDs].suf = True_Solutions[0].suf;
                        Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                        Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.29";
                    }
                }
                #endregion

                #region Rule: 1.30
                else if (e + 1 < Empty_POS.Count() && ((Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "فمن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "ولمن") && (Empty_POS[e + 1].pr.Equals("") && Empty_POS[e + 1].stem.Equals(""))))
                {
                    var Dis_Next_PR = Text_Solutions.Select(s => new { s.word, s.pr }).Distinct().Where(a => a.word == Empty_POS[e + 1].word).ToList();
                    if ((Previous_Word_Or_Tage[0].stem.Contains("Hayov/NOUN") || Previous_Word_Or_Tage[0].stem.Contains("<i*/NOUN")) && (Empty_POS[e].word == "من"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals("min/PREP"))).ToList();
                        if (True_Solutions.Count() == 1)
                        {
                            IDs = int.Parse(Empty_POS[e].ID) - 1;
                            Text_Analysis[IDs].lemmaID = True_Solutions[0].lemmaID;
                            Text_Analysis[IDs].pr = True_Solutions[0].pr;
                            Text_Analysis[IDs].stem = True_Solutions[0].stem;
                            Text_Analysis[IDs].suf = True_Solutions[0].suf;
                            Text_Analysis[IDs].spattern = True_Solutions[0].spattern;
                            Text_Analysis[IDs].affectedBy = True_Solutions[0].affectedBy + "_r:1.30";
                        }
                    }
                }
                #endregion
                #endregion
            }

            Empty_POS = Text_Analysis.Where(s => s.pr == "" && s.stem == "" && Regex.IsMatch(s.word, @"[ء-ي]")).ToList();
            for (int e = 0; e < Empty_POS.Count(); e++)
            {
                For_Best_Solution_Selection_List.Clear();

                #region 3.2 Analyzing_Word_Depending_On_Previous_Stem
                var Previous_Stem = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(Empty_POS[e].ID) - 1).ToList();
                var Next_Stem = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(Empty_POS[e].ID) + 1).ToList();
                #region Rule:2.1
                if (Previous_Stem[0].stem.Contains("Hat~aY/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word &&
                        ((a.stem.Contains("/IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") || a.suf.Contains("uwA/V_SUF"))) ||
                        (a.stem.Contains("lA/PART")) ||
                        (a.stem.Contains("NOUN")) ||
                        (a.stem.Contains("PV")) ||
                        (a.stem.Contains("PRON")) ||
                        (a.stem.Contains("CONJ")))).ToList();
                    Rule_Number = "_r:2.1";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule:2.2
                else if (Previous_Stem[0].stem.Contains("/PREP") && Previous_Stem[0].suf.Equals(""))
                {
                    if (Previous_Stem[0].stem.Equals("عن") && Empty_POS[e + 1].word.Equals("بعد"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e + 1].word && a.stem.Equals("buEod/NOUN")).ToList();
                        Rule_Number = "_r:2.2.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    else
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (
                            (a.stem.Contains("NOUN") && !a.suf.Contains("hu")) || (a.stem.Contains("A/") && !a.pr.Contains("PREP")) ||
                            (a.stem.Contains("CONJ") && !a.pr.Contains("wa/CONJ") && !a.pr.Contains("fa/CONJ")) ||
                            (a.stem.Contains("PART") && !a.pr.Contains("la/PART") && !a.pr.Contains(">a/PART")) ||
                            (a.stem.Contains("PRON"))
                            ) && !a.pr.Contains("PREP")).ToList();
                        Rule_Number = "_r:2.2.2";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                        IDs = int.Parse(Empty_POS[e].ID);
                        Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                    }
                }
                #endregion

                #region Rule: 2.3
                else if (Previous_Stem[0].stem.Contains("lan/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") ||
                        a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.3";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule:2.4
                else if (Previous_Stem[0].stem.Contains("kay/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") ||
                        a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) || a.stem.Contains("lA/PART") || a.stem.Contains("mA/PRON"))).ToList();
                    Rule_Number = "_r:2.4";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.5
                else if (Previous_Stem[0].stem.Contains(">an/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") ||
                        a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) || a.stem.Contains("/PV") || (a.stem.Contains("PART") && !a.stem.Contains("mA/PART") && !a.stem.Contains("lam/PART")))).ToList();
                    Rule_Number = "_r:2.5";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.6
                else if (Previous_Stem[0].stem.Contains("<in/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") ||
                        a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) || a.stem.Contains("/PV") || a.stem.Contains("lA/PART") || a.stem.Contains("lam/PART"))).ToList();
                    Rule_Number = "_r:2.6";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.7
                else if (Previous_Stem[0].stem.Contains("lam/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") ||
                        a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") || a.suf.Contains("uwA/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.7";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.8
                else if (Next_Stem[0].stem.Equals("layota/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Contains("PRON") || a.stem.Contains("PREP") || (!a.pr.Equals("") && a.stem.Equals("")))).ToList();
                    Rule_Number = "_r:2.8";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.9
                else if (Previous_Stem[0].stem.Contains(">an~a/CONJ") && Previous_Stem[0].suf == "")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("layos/PV") ||
                        a.stem.Contains("PRON") || a.stem.Contains("mim~A/CONJ") || a.stem.Contains("PREP") || a.stem.Contains("lA/PART") || a.stem.Contains("mA/PART") || a.stem.Contains("lam/PART") || (!a.pr.Equals("") && a.stem.Equals("")))).ToList();
                    Rule_Number = "_r:2.9";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.10
                else if (Previous_Stem[0].stem.Contains("<in~a/CONJ") && Previous_Stem[0].suf == "")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Contains("PRON") || a.stem.Contains("PREP") || a.stem.Contains("lA/PART") || a.stem.Contains("mA/PART"))).ToList();
                    Rule_Number = "_r:2.10";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.11
                else if ((Previous_Stem[0].stem.Contains("laEal~a/CONJ") || Previous_Stem[0].stem.Contains("layota/CONJ")) && Previous_Stem[0].suf == "")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Contains("PRON") || a.stem.Contains("PREP") || (!a.pr.Equals("") && a.stem.Equals("")))).ToList();
                    Rule_Number = "_r:2.11";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.12
                else if (Previous_Stem[0].stem.Contains("l`kin~a/CONJ") && Previous_Stem[0].suf == "")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Contains("PRON") || a.stem.Contains("PREP"))).ToList();
                    Rule_Number = "_r:2.12";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.13
                else if (Previous_Stem[0].stem.Contains("l`kin/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                          (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                          a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.suf.Contains("PV") ||
                          a.suf.Contains("PART") || a.suf.Contains("PREP") || (a.stem.Contains("/NOUN") && (a.pr.Contains("/PREP") || a.pr.Contains("fa/CONJ"))))).ToList();
                    Rule_Number = "_r:2.13";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.14
                else if (Previous_Stem[0].stem.Contains(">al~A/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") ||
                        a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") || a.suf.Contains("uwA/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.14";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.15
                else if (Previous_Stem[0].stem.Contains("<il~A/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") ||
                        a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) || a.stem.Contains(">an~a/CONJ") || a.stem.Contains("PV") || a.stem.Contains("PRON") || a.stem.Contains("PART"))).ToList();
                    Rule_Number = "_r:2.15";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.16
                else if (Previous_Stem[0].stem.Contains("<il~A/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Contains("PRON") || a.stem.Contains("PREP") || a.stem.Contains("PV"))).ToList();
                    Rule_Number = "_r:2.16";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.17
                else if (Previous_Stem[0].stem.Contains("<i*/NOUN") && Previous_Stem[0].suf == "")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                        a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                        a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.stem.Contains("NOUN") || a.stem.Contains("PRON") ||
                        a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PV"))).ToList();
                    Rule_Number = "_r:2.17";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.18
                else if (Previous_Stem[0].stem.Contains("Hayov/NOUN") && Previous_Stem[0].suf == "")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                        a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                        a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.stem.Contains("NOUN") ||
                        a.stem.Contains("PREP") || a.stem.Contains("PV"))).ToList();
                    Rule_Number = "_r:2.18";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.19
                else if (Previous_Stem[0].pr.Contains("ka/PREP") && Previous_Stem[0].stem == "mA/PRON")
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                        a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                        a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.stem.Contains("NOUN") ||
                        a.stem.Contains("PREP") || a.stem.Contains("PV") || a.stem.Contains("sawofa/PART") || a.stem.Contains("lam/PART") || a.stem.Contains("lA/PART") ||
                        a.stem.Contains("/PRON") || a.stem.Contains("qad/PART") || a.stem.Contains("law/CONJ") || a.stem.Contains(">an~a/CONJ") || (!a.pr.Equals("") && (a.stem.Equals(""))))).ToList();
                    Rule_Number = "_r:2.19";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.20
                else if (Previous_Stem[0].stem.Contains("PRON"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("IV") || a.stem.Contains("NOUN") || a.stem.Contains("PV") ||
                        a.stem.Contains("CV") || a.stem.Contains("PREP") || a.stem.Contains("PRON") || a.stem.Contains("CONJ") || a.stem.Contains("PART"))).ToList();
                    Rule_Number = "_r:2.20";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.21
                else if (Previous_Stem[0].stem.Contains("laqad/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV"))).ToList();
                    Rule_Number = "_r:2.21";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.22
                else if (Previous_Stem[0].stem.Equals("<ilAma/PART") || Previous_Stem[0].stem.Equals(">ayona/PART") || Previous_Stem[0].stem.Equals("EalAma/PART") || Previous_Stem[0].stem.Equals("fiyma/PART") || Previous_Stem[0].stem.Equals("hal/PART") || Previous_Stem[0].stem.Equals("hal~A/PART") || Previous_Stem[0].stem.Equals("kam/PART") || Previous_Stem[0].stem.Equals("kayofa/PART") || Previous_Stem[0].stem.Contains("mA*A/PART") || Previous_Stem[0].stem.Contains("mA/PART") || Previous_Stem[0].stem.Equals("man/PART") || Previous_Stem[0].stem.Equals("mataY/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                        a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                        a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.stem.Contains("NOUN") ||
                        a.stem.Contains("PV") || a.stem.Contains("PRON") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("CONJ"))).ToList();
                    Rule_Number = "_r:2.22";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.23
                else if (Previous_Stem[0].stem.Equals(">ay~/PART"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN"))).ToList();
                    Rule_Number = "_r:2.23";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.24
                else if (Previous_Stem[0].stem.Equals("lA/PART") && Previous_Stem[0].pr.Equals("bi/PREP"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN"))).ToList();
                    Rule_Number = "_r:2.24";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.25
                else if (Previous_Stem[0].stem.Equals("bud~/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Equals("min/PREP") || a.stem.Equals(">an/CONJ") || (a.stem.Equals(">an~a/CONJ") && !a.suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.25";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.26
                else if (Previous_Stem[0].stem.Equals("siy~amA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Equals("fiy/PREP") || a.stem.Equals("law/CONJ") || a.stem.Equals(">an~a/CONJ") || a.stem.Equals("qad/PART"))).ToList();
                    Rule_Number = "_r:2.26";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.27
                else if (Previous_Stem[0].stem.Equals("taHot/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON") || a.stem.Contains("PART")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV")) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.27";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.28
                else if (Previous_Stem[0].stem.Equals("duwn/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("NOUN") && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV")) && !Previous_Stem[0].suf.Equals("")) || a.stem.Contains("PRON") || a.stem.Equals(">an/CONJ"))).ToList();
                    Rule_Number = "_r:2.28";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.29
                else if (Previous_Stem[0].stem.Equals("maE/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PV") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PRON") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                        a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") &&
                        (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") ||
                        a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") || a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) &&
                        !Previous_Stem[0].suf.Equals("")) || a.stem.Contains("PRON") || a.stem.Equals(">an/CONJ"))).ToList();
                    Rule_Number = "_r:2.29";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.30
                else if (Previous_Stem[0].stem.Equals("muqAbil/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")) && (Previous_Stem[0].suf.Equals("")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") ||
                        a.stem.Equals(">an/CONJ") || a.stem.Equals(">an~a/CONJ") || a.stem.Equals("lA/PART") || a.stem.Contains("PRON"))).ToList();
                    Rule_Number = "_r:2.30";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.31
                else if (Previous_Stem[0].stem.Equals("bayon/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PRON") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PV") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.31";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.32
                else if (Previous_Stem[0].stem.Equals("xalof/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PRON") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PV") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.32";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.33
                else if (Previous_Stem[0].stem.Equals("Did~/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PRON") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PV") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.33";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.34
                else if (Previous_Stem[0].stem.Equals("Einod/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PRON") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PV") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.34";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.35
                else if (Previous_Stem[0].stem.Equals("xArij/NOUN") && (Previous_Stem[0].pr.Equals("")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PREP") || a.stem.Contains("PV") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.35";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.36
                else if (Previous_Stem[0].stem.Equals(">amAm/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PRON") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("PV") ||
                        (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.36";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.37
                else if (Previous_Stem[0].stem.Equals("Eabor/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("NOUN") && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.37";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.38
                else if (Previous_Stem[0].stem.Equals("tijAh/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        (((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.38";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.39
                else if (Previous_Stem[0].stem.Equals("waSaT/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.39";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.40
                else if (Previous_Stem[0].stem.Equals("warA'/NOUN") && (!Previous_Stem[0].pr.Contains("Al/DET")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("CONJ") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.40";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.41
                else if ((Previous_Stem[0].stem.Equals("hunA/NOUN") || Previous_Stem[0].stem.Equals("hunAk/NOUN")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("NOUN") || a.stem.Contains("PRON") ||
                        a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("CONJ") || a.stem.Contains("PV") || (!a.pr.Equals("") && a.stem.Equals("")) || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.41";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.42
                else if ((Previous_Stem[0].stem.Equals("ladaY/NOUN") || Previous_Stem[0].stem.Equals("laday/NOUN")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("CONJ") || a.stem.Contains("PRON") || a.stem.Contains("PV") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                        a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                        a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                        a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.42";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.43
                else if ((Previous_Stem[0].stem.Equals("HawAlay/NOUN") || Previous_Stem[0].stem.Equals("Sawob/NOUN") || Previous_Stem[0].stem.Equals("qud~Am/NOUN") || Previous_Stem[0].stem.Equals("HiyAl/NOUN") ||
                        Previous_Stem[0].stem.Equals("januwbiy~/NOUN") || Previous_Stem[0].stem.Equals("$aroqiy~/NOUN") || Previous_Stem[0].stem.Equals("garobiy~/NOUN") || Previous_Stem[0].stem.Equals("$amAliy~/NOUN") ||
                        Previous_Stem[0].stem.Equals("tilow/NOUN") || Previous_Stem[0].stem.Equals("madaY/NOUN") || Previous_Stem[0].stem.Equals("buEayod/NOUN") || ((Previous_Stem[0].stem.Equals("|nA'/NOUN") || Previous_Stem[0].stem.Equals("maTolaE/NOUN")) && !Previous_Stem[0].pr.Contains("Al/DET"))) && Previous_Stem[0].suf.Equals(""))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN"))).ToList();
                    Rule_Number = "_r:2.43";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.44
                else if (Previous_Stem[0].stem.Equals("<izA'/NOUN") || Previous_Stem[0].stem.Equals("xilAl/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("")) ||
                         ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PREP") || a.stem.Contains("PRON") || a.stem.Contains("CONJ") || a.stem.Contains("PV") || (!a.pr.Equals("") && a.stem.Equals("")) ||
                         (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && !Previous_Stem[0].suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.44";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.45
                else if (Previous_Stem[0].stem.Equals(">ayon/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PRON") || a.stem.Contains("PV") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/")))).ToList();
                    Rule_Number = "_r:2.45";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.46
                else if ((Previous_Stem[0].stem.Equals("vam~/NOUN") || (Previous_Stem[0].stem.Equals("saHAb/NOUN") && !Previous_Stem[0].pr.Contains("Al/DET"))) && Previous_Stem[0].suf.Equals("ap/N_SUF"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PRON"))).ToList();
                    Rule_Number = "_r:2.46";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.47
                else if (Previous_Stem[0].stem.Equals(">ayonamA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("/IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF")))) || a.stem.Contains("/PV"))).ToList();
                    Rule_Number = "_r:2.47";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.48
                else if (Previous_Stem[0].stem.Equals("HayovumA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF")))) || a.stem.Contains("PV"))).ToList();
                    Rule_Number = "_r:2.48";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.49
                else if (Previous_Stem[0].stem.Equals("qubAl/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (((a.stem.Contains("NOUN") || a.stem.Contains("PRON")) && Previous_Stem[0].suf.Equals("ap/N_SUF")) ||
                        ((a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PV") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                          a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                          a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                          a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && Previous_Stem[0].suf.Equals("at/NSUF")))).ToList();
                    Rule_Number = "_r:2.49";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.50
                else if (Previous_Stem[0].stem.Equals("Dimon/NOUN") || Previous_Stem[0].stem.Equals("<ivor/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("NOUN") && (a.suf.Equals("") || a.suf.Equals("At/N_SUF") || a.suf.Equals("ap/N_SUF"))) || a.stem.Contains("PRON"))).ToList();
                    Rule_Number = "_r:2.50";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.51
                else if (Previous_Stem[0].stem.Equals("bayonamA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PRON") || a.stem.Contains("PV") || a.stem.Contains("PREP") || a.stem.Contains("PART") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/")))).ToList();
                    Rule_Number = "_r:2.51";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.52
                else if (Previous_Stem[0].stem.Equals("Hiyn/NOUN") && !Previous_Stem[0].stem.Contains("Al/DET"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PV") || a.stem.Contains("CONJ") ||
                         (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.52";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.53
                else if (Previous_Stem[0].stem.Equals("muno*/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PV") || a.stem.Equals(">an/CONJ") || a.stem.Contains("PRON") || a.stem.Contains("mataY/PART") || (a.pr.Equals("Al/DET") && a.suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.53";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.54
                else if (Previous_Stem[0].stem.Equals("Al|n/NOUN") || Previous_Stem[0].stem.Equals(">amos/NOUN"))
                {
                    if (Empty_POS[e].word.Equals("الاول") || Empty_POS[e].word.Equals("الأول"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Equals(">aw~al/ADJ"))).ToList();
                        Rule_Number = "_r:2.54.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    else
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PRON") || a.stem.Contains("PV") || a.stem.Contains("PREP") || a.stem.Contains("PART") || a.stem.Contains("CV") || a.stem.Contains("CONJ") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                             a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                             a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                             a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/")))).ToList();
                        Rule_Number = "_r:2.54.2";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.55
                else if (Previous_Stem[0].stem.Equals("EinodamA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") || a.stem.Contains("lA/PART") ||
                         (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.55";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.56
                else if (Previous_Stem[0].stem.Equals("<i*A/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PV") || a.stem.Contains("PART") || a.stem.Contains("PRON") ||
                         (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.56";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.57
                else if (Previous_Stem[0].stem.Equals("Hiyna}i*/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PV") || a.stem.Contains("PRON") ||
                         (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.57";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.58
                else if (Previous_Stem[0].stem.Equals("baEodamA/NOUN") || Previous_Stem[0].stem.Equals("HiynamA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") || a.stem.Contains("lam/PART") ||
                         (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.58";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.59
                else if (Previous_Stem[0].stem.Equals("lam~A/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PV") || a.stem.Contains("lam/PART") || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF")))))).ToList();
                    Rule_Number = "_r:2.59";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.60
                else if (Previous_Stem[0].stem.Equals("kul~amA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") ||
                         (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.60";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.61
                else if (Previous_Stem[0].stem.Equals("qubayol/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PRON") || a.stem.Contains("PREP") || a.stem.Contains("qad/PART"))).ToList();
                    Rule_Number = "_r:2.61";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.62
                else if (Previous_Stem[0].stem.Equals("TAlamA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") || a.stem.Equals("lA/PART") || a.stem.Equals(">an~a/CONJ") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                          a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                          a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                          a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && Previous_Stem[0].suf.Equals("at/NSUF")).ToList();
                    Rule_Number = "_r:2.62";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.63
                else if (Previous_Stem[0].stem.Equals("|nA'/NOUN") && !Previous_Stem[0].suf.Contains("Al/DET"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") || a.stem.Equals("lA/PART") || a.stem.Equals(">an~a/CONJ") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                          a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                          a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                          a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && Previous_Stem[0].suf.Equals("at/NSUF")).ToList();
                    Rule_Number = "_r:2.63";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.64
                else if ((Previous_Stem[0].stem.Equals("TawAl/NOUN") || (Previous_Stem[0].stem.Equals("Tuwl/NOUN") && Previous_Stem[0].suf.Equals(""))) && !Previous_Stem[0].pr.Contains("Al/DET"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Equals("PRON") || (a.pr.Equals("Al/DET") && a.suf.Equals("")))).ToList();
                    Rule_Number = "_r:2.64";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.65
                else if (Previous_Stem[0].stem.Equals("waqotamA/NOUN") || Previous_Stem[0].stem.Equals("mataY/NOUN") || Previous_Stem[0].stem.Equals("rayovamA/NOUN") || Previous_Stem[0].stem.Equals("kayofamA/ADV"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") ||
                         (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))))).ToList();
                    Rule_Number = "_r:2.65";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.66
                else if (Previous_Stem[0].stem.Equals("Einoda}i*/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("CONJ") || a.stem.Contains("PV") ||
                         (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.66";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.67
                else if (Previous_Stem[0].stem.Equals("Hiyna*Ak/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PV") ||
                         (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.67";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.68
                else if (Previous_Stem[0].stem.Equals("gadA/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") || a.stem.Contains("NOUN"))).ToList();
                    Rule_Number = "_r:2.68";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.69
                else if (Previous_Stem[0].stem.Contains(">ay~An/NOUN"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                        a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                        a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.stem.Contains("NOUN") || a.stem.Contains("PRON") ||
                        a.stem.Contains("PREP") || (a.stem.Contains("PV") && Previous_Stem[0].pr.Contains("Al/DET")))).ToList();
                    Rule_Number = "_r:2.69";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.70
                else if (Previous_Stem[0].stem.Equals("kayof/ADV"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") || a.stem.Contains("CONJ") || (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                          a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                          a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                          a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) && Previous_Stem[0].suf.Equals("at/NSUF")).ToList();
                    Rule_Number = "_r:2.70";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.71
                else if (Previous_Stem[0].stem.Equals("EAd/ADV"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("PRON") ||
                         (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.71";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.72
                else if (Previous_Stem[0].stem.Equals("fajo>/ADV"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PV") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("PART") ||
                         (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                         a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && a.pr.Contains("li/")) || ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") ||
                         a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                         a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))))).ToList();
                    Rule_Number = "_r:2.72";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.73
                else if (Previous_Stem[0].stem.Contains("qad/PART"))
                {
                    if (Previous_Stem[0].stem.Equals("qad/PART"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") ||
                            (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") || a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") ||
                             a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))))).ToList();
                        Rule_Number = "_r:2.73.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    else
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV"))).ToList();
                        Rule_Number = "_r:2.73.2";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.74
                else if (Previous_Stem[0].stem.Equals("masA'/NOUN") && (Empty_POS[e].word.Equals("أمس")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("PV") ||
                        (a.stem.Contains(">amos/NOUN")))).ToList();
                    Rule_Number = "_r:2.74";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.75
                else if ((Previous_Stem[0].lemmaID.Equals("tam~-i") || Previous_Stem[0].lemmaID.Equals(">atam~")) && (Previous_Stem[0].stem.Contains("/PV") || (Previous_Stem[0].stem.Contains("/CV")) || (Previous_Stem[0].stem.Contains("/IV"))))
                {
                    if (Previous_Stem[0].lemmaID.Equals("tam~-i") || Previous_Stem[0].lemmaID.Equals(">atam~"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("/NOUN") || a.stem.Contains("PRON") || a.stem.Contains("/PREP") || a.stem.Contains("li>an~a/CONJ") || a.stem.Contains(">an/CONJ") || a.stem.Contains("<il~A/PART") || (!a.pr.Equals("") && a.stem.Equals("")))).ToList();
                        Rule_Number = "_r:2.75";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                }
                #endregion

                #region Rule: 2.76
                else if (Previous_Stem[0].stem.Contains("/PV"))
                {
                    if (Previous_Stem[0].stem.Equals("layos/PV"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("/NOUN") || a.stem.Contains("/PRON") || a.stem.Contains("PART") || a.stem.Contains("/PREP") || (!a.pr.Equals("") && a.stem.Equals("")) ||
                            (a.stem.Contains("CONJ") && (a.lemmaID.Equals(">an") || a.lemmaID.Equals(">an~a") || a.lemmaID.Equals("li>an~a") || a.lemmaID.Equals("li>an") || a.lemmaID.Equals("mim~A") || a.lemmaID.Equals("<i*A") || a.lemmaID.Equals("Hat~aY") || a.lemmaID.Equals("ka>an~a") || a.lemmaID.Equals("likay") || a.lemmaID.Equals("kay") || a.lemmaID.Equals("mim~an") || a.lemmaID.Equals("mivolamA"))))).ToList();
                        Rule_Number = "_r:2.76.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }

                    else if (!Previous_Stem[0].suf.Contains("+"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                            a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && !a.pr.Contains("li/")) || ((a.stem.Contains("IV") &&
                            (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                            a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) || (a.stem.Contains("/PV") && (a.pr.Contains("wa/CONJ") || a.lemmaID.Equals("kAn-u") || a.lemmaID.Equals("layosa") || a.lemmaID.Equals("SAr") || a.lemmaID.Equals("Sal~aY") || a.lemmaID.Equals("jal~-i") || a.lemmaID.Equals("qAl-u"))) ||
                           (a.stem.Contains("PV") && (Previous_Stem[0].lemmaID.Equals("kAn-u") || Previous_Stem[0].lemmaID.Equals("jal~-i") || Previous_Stem[0].lemmaID.Equals("taEAlaY"))) || a.stem.Contains("/NOUN") || a.stem.Contains("/ADV") || a.stem.Contains("CONJ") || a.stem.Contains("PRON") || a.stem.Contains("PART") || a.stem.Contains("PREP") || (!a.pr.Equals("") && a.stem.Equals("")))).ToList();
                        Rule_Number = "_r:2.76.2";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    else if (Previous_Stem[0].suf.Contains("+"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("CV") || a.stem.Contains("PRON") || a.stem.Contains("PART") || (!a.pr.Equals("") && a.stem.Equals("")) || (a.stem.Contains("IV") && (a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                            a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") || a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF")) && !a.pr.Contains("li/")) || ((a.stem.Contains("IV") &&
                            (a.suf.Contains("e/V_SUF") || a.suf.Contains("A/V_SUF") || a.suf.Contains("awo/V_SUF") || a.suf.Contains("awoA/V_SUF") || a.suf.Contains("ayo/V_SUF") || a.suf.Contains("iy/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uw/V_SUF") ||
                            a.suf.Contains("uwA/V_SUF"))) && a.pr.Contains("li/"))) || a.stem.Contains("/NOUN") || a.stem.Contains("/ADV") || a.stem.Contains("PREP") || a.stem.Contains("/PV") || a.stem.Contains("CONJ"))).ToList();
                        Rule_Number = "_r:2.76.3";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.77
                else if (Previous_Stem[0].stem.Contains("/IV"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("IV") || a.stem.Contains("/PV") ||
                        a.stem.Contains("/NOUN") || a.stem.Contains("/ADV") || a.stem.Contains("CONJ") || a.stem.Contains("PREP") || a.stem.Contains("CV") || a.stem.Contains("PART") || a.stem.Contains("PRON") || (!a.pr.Equals("") && a.stem.Equals("")))).ToList();
                    Rule_Number = "_r:2.77";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.78
                else if (Previous_Stem[0].stem.Contains("/CV"))
                {
                    if (!Previous_Stem[0].suf.Contains("+"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((!a.pr.Equals("") && a.stem.Equals("") ||
                            a.stem.Contains("CV") || a.stem.Contains("PRON") || a.stem.Contains("PART") || a.stem.Contains("IV") || a.stem.Contains("/NOUN") || a.stem.Contains("/ADV") || a.stem.Contains("PREP") || a.stem.Contains("CONJ")))).ToList();
                        Rule_Number = "_r:2.78.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    else if (Previous_Stem[0].suf.Contains("+"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("CV") || a.stem.Contains("PART") ||
                            a.stem.Contains("IV") || a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("PREP") || a.stem.Contains("PRON") || a.stem.Contains("CONJ"))).ToList();
                        Rule_Number = "_r:2.78.2";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.79
                else if ((Previous_Stem[0].word.Contains("هؤلاء") || Previous_Stem[0].word.Equals("هم")) && Empty_POS[e].word.EndsWith("ين") && !Empty_POS[e].word.Equals("الذين"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.EndsWith("/NOUN") && a.suf.Equals("iyna/N_SUF"))).ToList();
                    Rule_Number = "_r:2.79";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.80
                else if (Previous_Stem[0].word.EndsWith("الذين") && Empty_POS[e].word.Equals("هم"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.EndsWith("/PRON"))).ToList();
                    Rule_Number = "_r:2.80";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.81
                else if (e + 1 < Empty_POS.Count() && ((Empty_POS[e].word == "من" || Empty_POS[e].word == "ومن" || Empty_POS[e].word == "فمن" || Empty_POS[e].word == "لمن" || Empty_POS[e].word == "ولمن") && (Empty_POS[e + 1].pr.Equals("") && Empty_POS[e + 1].stem.Equals(""))))
                {
                    var Dis_Next_PR = Text_Solutions.Select(s => new { s.word, s.pr }).Distinct().Where(a => a.word == Empty_POS[e + 1].word).ToList();
                    if (Dis_Next_PR.Count() == 1 && (Dis_Next_PR[0].pr.Contains("IV") || Dis_Next_PR[0].pr.Contains("PV")))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("min/PREP"))).ToList();
                        Rule_Number = "_r:2.81.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                        IDs = int.Parse(Empty_POS[e].ID);
                        Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                    }
                    else
                    {
                        if ((Dis_Next_PR.Count() == 1 && Dis_Next_PR[0].pr.Contains("Al/DET")) || (Empty_POS[e + 1].word.Equals("ثم")))
                        {
                            var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("man/PRON"))).ToList();
                            Rule_Number = "_r:2.81.2";
                            for (int s = 0; s < True_Solutions.Count(); s++)
                            {
                                Best_Solution_List Best_Solution = new Best_Solution_List();
                                Best_Solution.word = True_Solutions[s].word;
                                Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                                Best_Solution.pr = True_Solutions[s].pr;
                                Best_Solution.stem = True_Solutions[s].stem;
                                Best_Solution.suf = True_Solutions[s].suf;
                                Best_Solution.spattern = True_Solutions[s].spattern;
                                Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                Best_Solution.spattern = True_Solutions[s].spattern;
                                Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                For_Best_Solution_Selection_List.Add(Best_Solution);
                            }
                            IDs = int.Parse(Empty_POS[e].ID);
                            Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                        }
                        else
                        {
                            if (Empty_POS[e + 1].word == "لا" || Empty_POS[e + 1].word == "لم")
                            {
                                if (Empty_POS[e + 2].word == "شيء" || Empty_POS[e + 2].word == "شئ" || Empty_POS[e + 2].word == "شيئ")
                                {
                                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("min/PREP"))).ToList();
                                    Rule_Number = "_r:2.81.3.1";
                                    for (int s = 0; s < True_Solutions.Count(); s++)
                                    {
                                        Best_Solution_List Best_Solution = new Best_Solution_List();
                                        Best_Solution.word = True_Solutions[s].word;
                                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                                        Best_Solution.pr = True_Solutions[s].pr;
                                        Best_Solution.stem = True_Solutions[s].stem;
                                        Best_Solution.suf = True_Solutions[s].suf;
                                        Best_Solution.spattern = True_Solutions[s].spattern;
                                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                        Best_Solution.spattern = True_Solutions[s].spattern;
                                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                        For_Best_Solution_Selection_List.Add(Best_Solution);
                                    }
                                    IDs = int.Parse(Empty_POS[e].ID);
                                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                                }
                                else
                                {
                                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Contains("man/PRON"))).ToList();
                                    Rule_Number = "_r:2.81.3.2";
                                    for (int s = 0; s < True_Solutions.Count(); s++)
                                    {
                                        Best_Solution_List Best_Solution = new Best_Solution_List();
                                        Best_Solution.word = True_Solutions[s].word;
                                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                                        Best_Solution.pr = True_Solutions[s].pr;
                                        Best_Solution.stem = True_Solutions[s].stem;
                                        Best_Solution.suf = True_Solutions[s].suf;
                                        Best_Solution.spattern = True_Solutions[s].spattern;
                                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                        Best_Solution.spattern = True_Solutions[s].spattern;
                                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                                        For_Best_Solution_Selection_List.Add(Best_Solution);
                                    }
                                    IDs = int.Parse(Empty_POS[e].ID);
                                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Rule: 2.82
                else if (Previous_Stem[0].stem.Equals("mahomA/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("/PV") || a.stem.Contains("/IV"))).ToList();
                    Rule_Number = "_r:2.82";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.83
                else if (Previous_Stem[0].stem.Equals("law/CONJ"))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("/PV") || a.stem.Contains("/IV") || a.stem.Contains("/NOUN") || a.stem.Contains("/PREP") || a.stem.Equals("lam/PART") || a.stem.Equals(">an~a/CONJ"))).ToList();
                    Rule_Number = "_r:2.83";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.84
                else if (Next_Stem[0].stem.Contains("/NOUN") && !Next_Stem[0].lemmaID.Equals(">ayoD") && (Empty_POS[e].word.Equals("لكن") || Empty_POS[e].word.Equals("ولكن") || Empty_POS[e].word.Equals("فلكن")))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (!a.stem.Equals("l`kin/CONJ"))).ToList();
                    Rule_Number = "_r:2.84";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.Prop = 0;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion

                #region Rule: 2.85
                else if (Previous_Stem[0].stem.Contains("kul~/NOUN") && Previous_Stem[0].suf == "" && Empty_POS[e].pr.Contains("fa/CONJ"))
                {
                    if (Previous_Stem[0].pr.Contains("Al/DET"))
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && ((a.stem.Contains("IV") && (a.suf.Contains("e/V_SUF") ||
                            a.suf.Contains("Ani/V_SUF") || a.suf.Contains("awona/V_SUF") || a.suf.Contains("ayona/V_SUF") ||
                            a.suf.Contains("iyna/V_SUF") || a.suf.Contains("na/V_SUF") || a.suf.Contains("uwna/V_SUF"))) || a.stem.Contains("NOUN") || a.stem.Contains("PRON") ||
                            a.stem.Contains("PREP") || a.stem.Contains("PV"))).ToList();
                        Rule_Number = "_r:2.85.1";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                        IDs = int.Parse(Empty_POS[e].ID);
                        Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                    }
                    else
                    {
                        var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word && (a.stem.Contains("NOUN") || a.stem.Contains("PRON") ||
                            a.stem.Contains("PREP"))).ToList();
                        Rule_Number = "_r:2.85.2";
                        for (int s = 0; s < True_Solutions.Count(); s++)
                        {
                            Best_Solution_List Best_Solution = new Best_Solution_List();
                            Best_Solution.word = True_Solutions[s].word;
                            Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                            Best_Solution.pr = True_Solutions[s].pr;
                            Best_Solution.stem = True_Solutions[s].stem;
                            Best_Solution.suf = True_Solutions[s].suf;
                            Best_Solution.spattern = True_Solutions[s].spattern;
                            Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                            Best_Solution.Prop = 0;
                            For_Best_Solution_Selection_List.Add(Best_Solution);
                        }
                        IDs = int.Parse(Empty_POS[e].ID);
                        Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                    }
                }
                #endregion

                #region Language_Model_Without_Rules
                if (Empty_POS[e].pr.Equals("") && Empty_POS[e].stem.Equals(""))
                {
                    var True_Solutions = Text_Solutions.Select(s => new { s.lemmaID, s.pr, s.stem, s.suf, s.word, s.spattern, s.affectedBy }).Distinct().Where(a => a.word == Empty_POS[e].word).ToList();
                    Rule_Number = "";
                    for (int s = 0; s < True_Solutions.Count(); s++)
                    {
                        Best_Solution_List Best_Solution = new Best_Solution_List();
                        Best_Solution.word = True_Solutions[s].word;
                        Best_Solution.lemmaID = True_Solutions[s].lemmaID;
                        Best_Solution.pr = True_Solutions[s].pr;
                        Best_Solution.stem = True_Solutions[s].stem;
                        Best_Solution.suf = True_Solutions[s].suf;
                        Best_Solution.spattern = True_Solutions[s].spattern;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        Best_Solution.affectedBy = True_Solutions[s].affectedBy;
                        For_Best_Solution_Selection_List.Add(Best_Solution);
                    }
                    IDs = int.Parse(Empty_POS[e].ID);
                    Runtime_Phase(Text_Analysis, IDs, For_Best_Solution_Selection_List, obj);
                }
                #endregion
                #endregion

                #region
                var After_Analysis = Text_Analysis.Where(s => s.stem == "l`kin~a/CONJ").ToList();
                for (int a = 0; a < After_Analysis.Count(); a++)
                {
                    Next_Stem = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(After_Analysis[a].ID) + 1).ToList();
                    if (Next_Stem[0].stem.EndsWith("/NOUN") && Next_Stem[0].pr.Contains("li/PREP") && !Next_Stem[0].lemmaID.Equals(">ayoD"))
                    {
                        IDs = int.Parse(After_Analysis[a].ID) - 1;
                        Text_Analysis[IDs].stem = "l`kin/CONJ";
                    }
                }
                After_Analysis = Text_Analysis.Where(s => s.stem == ">ay~/NOUN").ToList();
                for (int a = 0; a < After_Analysis.Count(); a++)
                {
                    Next_Stem = Text_Analysis.Where(s => int.Parse(s.ID) == int.Parse(After_Analysis[a].ID) + 1).ToList();
                    if (Next_Stem[0].stem.EndsWith(">an~a/CONJ"))
                    {
                        IDs = int.Parse(After_Analysis[a].ID) - 1;
                        Text_Analysis[IDs].stem = ">ay/PART";
                    }
                }
                #endregion
            }
            Analyzed_Text = Text_Analysis;
            #endregion
        }
        private void Runtime_Phase(List<Analyzed_Text> Text_Analysis, int IDs, List<Best_Solution_List> For_Best_Solution_Selection_List, Form1 obj)
        {

            #region if_Rules_Return_One_Solution
            if (For_Best_Solution_Selection_List.Count() == 1)
            {
                Text_Analysis[IDs - 1].lemmaID = For_Best_Solution_Selection_List[0].lemmaID;
                Text_Analysis[IDs - 1].pr = For_Best_Solution_Selection_List[0].pr;
                Text_Analysis[IDs - 1].stem = For_Best_Solution_Selection_List[0].stem;
                Text_Analysis[IDs - 1].suf = For_Best_Solution_Selection_List[0].suf;
                Text_Analysis[IDs - 1].spattern = For_Best_Solution_Selection_List[0].spattern;
                Text_Analysis[IDs - 1].affectedBy = For_Best_Solution_Selection_List[0].affectedBy + Rule_Number;
            }
            #endregion

            #region Runtime_Phase
            else
            {
                if (For_Best_Solution_Selection_List.Count > 1)
                {

                    #region RunTime_Phase_Variables
                    int k1 = 5; int k2 = 1; int m = 0;
                    string Previous_Seq = ""; string Next_Seq = ""; string Collected_Seq = ""; string Current_Seq = "";
                    string Previous_Previous_Seq = ""; string Next_Next_Seq = "";
                    string Previous_Previous_Previous_Seq = ""; string Next_Next_Next_Seq = "";
                    string Previous_Sequences = ""; string Next_Sequences = ""; string prf_value = ""; string suf_value = "";
                    string stem_value = ""; string next_value_m_minues_one = ""; 
                    List<string> dis_tags = new List<string>(); double word_count = 0; string current_word_tag = "";
                    string current_word_form = ""; double current_word_form_counts = 0;
                    #endregion

                    #region Stem_Value
                    for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                    {
                        if (For_Best_Solution_Selection_List[best].stem.Contains("/"))
                        {
                            stem_value = For_Best_Solution_Selection_List[best].stem.Split('/')[1].ToString();
                        }
                        if (!For_Best_Solution_Selection_List[best].stem.Contains("/"))
                        {
                            stem_value = For_Best_Solution_Selection_List[best].stem.ToString();
                        }
                        dis_tags.Add(stem_value);
                    }
                    List<string> distinct_tags = dis_tags.Distinct().ToList();
                    #endregion

                    #region Where_Distinct_Tags_Does_Not_Equals_One
                    if (distinct_tags.Count > 1)
                    {
                        #region Best_With_Relation_To_Previous_POS_Sequences
                        var previous_needed_seq = Text_Analysis.Where(s => int.Parse(s.ID) == IDs - 3 || int.Parse(s.ID) == IDs - 2 || int.Parse(s.ID) == IDs - 1 || int.Parse(s.ID) == IDs).ToList();
                        for (int s = 0; s < previous_needed_seq.Count(); s++)
                        {
                            if (!int.Parse(previous_needed_seq[s].ID).Equals(IDs))
                            {
                                if (!previous_needed_seq[s].stem.Equals("<s>") && !previous_needed_seq[s].stem.Equals("</s> <s>"))
                                {
                                    #region Prefix_Value
                                    string Prefixes = previous_needed_seq[s].pr; string[] Each_Prf = Prefixes.Split('+'); Prefixes = "";
                                    if (Each_Prf.Count() == 3)
                                    {
                                        prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "_" + Each_Prf[2].Split('/').ToString() + "_";
                                    }
                                    else if (Each_Prf.Count() == 2)
                                    {
                                        prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "__";
                                    }
                                    else if (Each_Prf.Count() == 1)
                                    {
                                        if (Each_Prf[0].Equals(""))
                                        {
                                            prf_value = "___";
                                        }
                                        else
                                        {
                                            prf_value = Each_Prf[0].Split('/')[1].ToString() + "___";
                                        }
                                    }
                                    #endregion
                                    #region Suffix_Value
                                    string Suffixes = previous_needed_seq[s].suf; string[] Each_Suf = Suffixes.Split('+'); Suffixes = "";
                                    if (Each_Suf.Count() == 2)
                                    {
                                        suf_value = Each_Suf[0].Split('/')[1].ToString() + "_" + Each_Suf[1].Split('/')[1].ToString();
                                    }
                                    else if (Each_Suf.Count() == 1)
                                    {
                                        if (Each_Suf[0].Equals(""))
                                        {
                                            suf_value = "_";
                                        }
                                        else
                                        {
                                            suf_value = Each_Suf[0].Split('/')[1].ToString() + "_";
                                        }
                                    }
                                    #endregion
                                    #region Stem_Value
                                    if (previous_needed_seq[s].stem.Contains("/"))
                                    {
                                        stem_value = previous_needed_seq[s].stem.Split('/')[1].ToString() + "_";
                                    }
                                    if (!previous_needed_seq[s].stem.Contains("/"))
                                    {
                                        stem_value = previous_needed_seq[s].stem.ToString() + "_";
                                    }
                                    #endregion
                                }
                                else if (previous_needed_seq[s].stem.Equals("<s>") || previous_needed_seq[s].stem.Equals("</s> <s>"))
                                {
                                    prf_value = ""; suf_value = ""; stem_value = "<s>";
                                }

                                #region Solutions_Value
                                if (int.Parse(previous_needed_seq[s].ID).Equals(IDs - 1))
                                {
                                    Previous_Seq = prf_value + stem_value + suf_value;
                                }
                                if (int.Parse(previous_needed_seq[s].ID).Equals(IDs - 2))
                                {
                                    Previous_Previous_Seq = prf_value + stem_value + suf_value;
                                }
                                if (int.Parse(previous_needed_seq[s].ID).Equals(IDs - 3))
                                {
                                    Previous_Previous_Previous_Seq = prf_value + stem_value + suf_value;
                                }
                                #endregion

                            }
                            else if (int.Parse(previous_needed_seq[s].ID).Equals(IDs))
                            {
                                #region Get_m_Value
                                if (Previous_Seq.Equals("_____"))
                                {
                                    m = 1;
                                }
                                else if (Previous_Seq.Equals("<s>"))
                                {
                                    m = 2;
                                    Previous_Sequences = Previous_Seq;
                                }
                                else if (!Previous_Seq.Equals("_____") && Previous_Previous_Seq.Equals("_____") && Previous_Previous_Previous_Seq.Equals("_____"))
                                {
                                    m = 2;
                                    Previous_Sequences = Previous_Seq;
                                }
                                else if (Previous_Seq.Equals("<s>") && !Previous_Previous_Seq.Equals("_____") && Previous_Previous_Previous_Seq.Equals("_____"))
                                {
                                    m = 3;
                                    Previous_Sequences = Previous_Previous_Seq + " " + Previous_Seq;
                                }
                                else if ((!Previous_Seq.Equals("_____")) && !Previous_Previous_Seq.Equals("_____") && Previous_Previous_Previous_Seq.Equals("_____"))
                                {
                                    m = 3;
                                    Previous_Sequences = Previous_Previous_Seq + " " + Previous_Seq;
                                }
                                else if (Previous_Seq.Equals("<s>") && !Previous_Previous_Seq.Equals("_____") && !Previous_Previous_Previous_Seq.Equals("_____"))
                                {
                                    m = 4;
                                    Previous_Sequences = Previous_Previous_Previous_Seq + " " + Previous_Previous_Seq + " " + Previous_Seq;
                                }
                                else if ((!Previous_Seq.Equals("_____") || Previous_Seq.Equals("<s>")) && !Previous_Previous_Seq.Equals("_____") && !Previous_Previous_Previous_Seq.Equals("_____"))
                                {
                                    m = 4;
                                    Previous_Sequences = Previous_Previous_Previous_Seq + " " + Previous_Previous_Seq + " " + Previous_Seq;
                                }
                                #endregion

                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    #region Prefix_Value
                                    string Prefixes = For_Best_Solution_Selection_List[best].pr; string[] Each_Prf = Prefixes.Split('+'); Prefixes = "";
                                    if (Each_Prf.Count() == 3)
                                    {
                                        prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "_" + Each_Prf[2].Split('/').ToString() + "_";
                                    }
                                    else if (Each_Prf.Count() == 2)
                                    {
                                        prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "__";
                                    }
                                    else if (Each_Prf.Count() == 1)
                                    {
                                        if (Each_Prf[0].Equals(""))
                                        {
                                            prf_value = "___";
                                        }
                                        else
                                        {
                                            prf_value = Each_Prf[0].Split('/')[1].ToString() + "___";
                                        }
                                    }
                                    #endregion
                                    #region Suffix_Value
                                    string Suffixes = For_Best_Solution_Selection_List[best].suf; string[] Each_Suf = Suffixes.Split('+'); Suffixes = "";
                                    if (Each_Suf.Count() == 2)
                                    {
                                        suf_value = Each_Suf[0].Split('/')[1].ToString() + "_" + Each_Suf[1].Split('/')[1].ToString();
                                    }
                                    else if (Each_Suf.Count() == 1)
                                    {
                                        if (Each_Suf[0].Equals(""))
                                        {
                                            suf_value = "_";
                                        }
                                        else
                                        {
                                            suf_value = Each_Suf[0].Split('/')[1].ToString() + "_";
                                        }
                                    }
                                    #endregion
                                    #region Stem_Value
                                    if (For_Best_Solution_Selection_List[best].stem.Contains("/"))
                                    {
                                        stem_value = For_Best_Solution_Selection_List[best].stem.Split('/')[1].ToString() + "_";
                                    }
                                    if (!For_Best_Solution_Selection_List[best].stem.Contains("/"))
                                    {
                                        stem_value = For_Best_Solution_Selection_List[best].stem.ToString() + "_";
                                    }
                                    #endregion
                                    #region Solution_Value
                                    double count = 0;
                                    Current_Seq = prf_value + stem_value + suf_value;
                                    #endregion

                                    Collected_Seq = (Previous_Sequences + " " + Current_Seq).TrimStart().TrimEnd();
                                    try
                                    {
                                        var pos_seq_count = obj.Offline_Phases.pos_seq.Where(a => a.tags_seq == Collected_Seq).ToList();
                                        count = double.Parse(pos_seq_count[0].count);
                                    }
                                    catch { };

                                    if (count > 1)
                                    {
                                        #region Bayes' Rule
                                        #region Writh the Gram Value in Input file
                                        string path = "Input.txt";
                                        StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                                        sw.WriteLine(Collected_Seq);
                                        sw.Flush();
                                        sw.Close();
                                        #endregion

                                        #region Running cmd
                                        string strCmdText = "/C ngram.exe POS_Seq_KN.lm Input.txt > Output.txt";
                                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.Arguments = strCmdText;
                                        process.StartInfo = startInfo;
                                        process.Start();
                                        process.WaitForExit();
                                        process.Close();
                                        #endregion

                                        #region Get the Output value
                                        var lastLine = File.ReadLines("Output.txt").Last();
                                        double Output_Value = double.Parse(lastLine.Substring(lastLine.IndexOf("-"), lastLine.Length - 2));
                                        For_Best_Solution_Selection_List[best].Prop = Math.Pow(10, Output_Value);
                                        #endregion
                                        #endregion
                                    }
                                    else if (m > 1 && count <= 1) 
                                    {
                                        #region Apply the back-off recursive procedure
                                        #region Writh the Gram Value in Input file
                                        string path = "Input.txt";
                                        StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                                        sw.WriteLine(Previous_Sequences);
                                        sw.Flush();
                                        sw.Close();
                                        #endregion

                                        #region Running cmd
                                        string strCmdText = "/C ngram.exe POS_Seq_KN.lm Input.txt > Output.txt";
                                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                        startInfo.FileName = "cmd.exe";
                                        startInfo.Arguments = strCmdText;
                                        process.StartInfo = startInfo;
                                        process.Start();
                                        process.WaitForExit();
                                        process.Close();
                                        #endregion

                                        #region Get the Output value
                                        var lastLine = File.ReadLines("Output.txt").Last();
                                        if (!lastLine.Contains("#INF"))
                                        {
                                            double Output_Value = double.Parse(lastLine.Substring(lastLine.IndexOf("-"), lastLine.Length - 2));
                                            For_Best_Solution_Selection_List[best].Prop = Math.Pow(10, Output_Value);
                                        }
                                        #endregion
                                        #endregion
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Best_With_Relation_To_Next_POS_Sequences
                        var next_needed_seq = Text_Analysis.Where(s => int.Parse(s.ID) == IDs + 1 || int.Parse(s.ID) == IDs + 2 || int.Parse(s.ID) == IDs + 3).ToList();
                        for (int s = 0; s < next_needed_seq.Count(); s++)
                        {
                            if (!int.Parse(next_needed_seq[s].ID).Equals(IDs))
                            {
                                #region Prefix_Value
                                string Prefixes = next_needed_seq[s].pr; string[] Each_Prf = Prefixes.Split('+'); Prefixes = "";
                                if (Each_Prf.Count() == 3)
                                {
                                    prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "_" + Each_Prf[2].Split('/').ToString() + "_";
                                }
                                else if (Each_Prf.Count() == 2)
                                {
                                    prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "__";
                                }
                                else if (Each_Prf.Count() == 1)
                                {
                                    if (Each_Prf[0].Equals(""))
                                    {
                                        prf_value = "___";
                                    }
                                    else
                                    {
                                        prf_value = Each_Prf[0].Split('/')[1].ToString() + "___";
                                    }
                                }
                                #endregion
                                #region Suffix_Value
                                string Suffixes = next_needed_seq[s].suf; string[] Each_Suf = Suffixes.Split('+'); Suffixes = "";
                                if (Each_Suf.Count() == 2)
                                {
                                    suf_value = Each_Suf[0].Split('/')[1].ToString() + "_" + Each_Suf[1].Split('/')[1].ToString();
                                }
                                else if (Each_Suf.Count() == 1)
                                {
                                    if (Each_Suf[0].Equals(""))
                                    {
                                        suf_value = "_";
                                    }
                                    else
                                    {
                                        suf_value = Each_Suf[0].Split('/')[1].ToString() + "_";
                                    }
                                }
                                #endregion
                                #region Stem_Value
                                if (next_needed_seq[s].stem.Contains("/"))
                                {
                                    stem_value = next_needed_seq[s].stem.Split('/')[1].ToString() + "_";
                                }
                                if (!next_needed_seq[s].stem.Contains("/"))
                                {
                                    stem_value = next_needed_seq[s].stem.ToString() + "_";
                                }
                                #endregion

                                #region Solutions_Value
                                if (int.Parse(next_needed_seq[s].ID).Equals(IDs + 1))
                                {
                                    if (next_needed_seq[s].stem.Equals("</s>") || next_needed_seq[s].stem.Equals("</s> <s>"))
                                    {
                                        Next_Seq = "</s>";
                                    }
                                    else
                                    {
                                        Next_Seq = prf_value + stem_value + suf_value;
                                    }
                                }
                                if (int.Parse(next_needed_seq[s].ID).Equals(IDs + 2))
                                {
                                    if (next_needed_seq[s].stem.Equals("</s>") || next_needed_seq[s].stem.Equals("</s> <s>"))
                                    {
                                        Next_Next_Seq = "</s>";
                                    }
                                    else
                                    {
                                        Next_Next_Seq = prf_value + stem_value + suf_value;
                                    }
                                }
                                if (int.Parse(next_needed_seq[s].ID).Equals(IDs + 3))
                                {
                                    if (next_needed_seq[s].stem.Equals("</s>") || next_needed_seq[s].stem.Equals("</s> <s>"))
                                    {
                                        Next_Next_Next_Seq = "</s>";
                                    }
                                    else
                                    {
                                        Next_Next_Next_Seq = prf_value + stem_value + suf_value;
                                    }
                                }
                                #endregion
                            }
                        }
                        #region Get_m_Value
                        if (Next_Seq.Equals("</s>"))
                        {
                            m = 1;
                        }
                        else if (Next_Seq.Equals("_____"))
                        {
                            m = 1;
                        }
                        else if (!Next_Seq.Equals("_____") && Next_Next_Seq.Equals("</s>"))
                        {
                            m = 2;
                            Next_Sequences = Next_Seq;
                        }
                        else if (!Next_Seq.Equals("_____") && Next_Next_Seq.Equals("_____"))
                        {
                            m = 2;
                            Next_Sequences = Next_Seq;
                        }
                        else if (!Next_Seq.Equals("_____") && !Next_Next_Seq.Equals("_____") && Next_Next_Next_Seq.Equals("</s>"))
                        {
                            m = 3;
                            Next_Sequences = Next_Seq + " " + Next_Next_Seq;
                        }
                        else if (!Next_Seq.Equals("_____") && !Next_Next_Seq.Equals("_____") && Next_Next_Next_Seq.Equals("_____"))
                        {
                            m = 3;
                            Next_Sequences = Next_Seq + " " + Next_Next_Seq;
                        }
                        else if (!Next_Seq.Equals("_____") && !Next_Next_Seq.Equals("_____") && !Next_Next_Next_Seq.Equals("</s>"))
                        {
                            m = 4;
                            Next_Sequences = Next_Seq + " " + Next_Next_Seq + " " + Next_Next_Next_Seq;
                        }
                        else if (!Next_Seq.Equals("_____") && !Next_Next_Seq.Equals("_____") && !Next_Next_Next_Seq.Equals("_____"))
                        {
                            m = 4;
                            Next_Sequences = Next_Seq + " " + Next_Next_Seq + " " + Next_Next_Next_Seq;
                        }
                        #endregion

                        if (m > 1)
                        {
                            // Average = Average + 1;
                            for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                            {
                                #region Prefix_Value
                                string Prefixes = For_Best_Solution_Selection_List[best].pr; string[] Each_Prf = Prefixes.Split('+'); Prefixes = "";
                                if (Each_Prf.Count() == 3)
                                {
                                    prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "_" + Each_Prf[2].Split('/').ToString() + "_";
                                }
                                else if (Each_Prf.Count() == 2)
                                {
                                    prf_value = Each_Prf[0].Split('/')[1].ToString() + "_" + Each_Prf[1].Split('/')[1].ToString() + "__";
                                }
                                else if (Each_Prf.Count() == 1)
                                {
                                    if (Each_Prf[0].Equals(""))
                                    {
                                        prf_value = "___";
                                    }
                                    else
                                    {
                                        prf_value = Each_Prf[0].Split('/')[1].ToString() + "___";
                                    }
                                }
                                #endregion
                                #region Suffix_Value
                                string Suffixes = For_Best_Solution_Selection_List[best].suf; string[] Each_Suf = Suffixes.Split('+'); Suffixes = "";
                                if (Each_Suf.Count() == 2)
                                {
                                    suf_value = Each_Suf[0].Split('/')[1].ToString() + "_" + Each_Suf[1].Split('/')[1].ToString();
                                }
                                else if (Each_Suf.Count() == 1)
                                {
                                    if (Each_Suf[0].Equals(""))
                                    {
                                        suf_value = "_";
                                    }
                                    else
                                    {
                                        suf_value = Each_Suf[0].Split('/')[1].ToString() + "_";
                                    }
                                }
                                #endregion
                                #region Stem_Value
                                if (For_Best_Solution_Selection_List[best].stem.Contains("/"))
                                {
                                    stem_value = For_Best_Solution_Selection_List[best].stem.Split('/')[1].ToString() + "_";
                                }
                                if (!For_Best_Solution_Selection_List[best].stem.Contains("/"))
                                {
                                    stem_value = For_Best_Solution_Selection_List[best].stem.ToString() + "_";
                                }
                                #endregion
                                #region Solution_Value
                                double count = 0; int next_count = 0;
                                Current_Seq = prf_value + stem_value + suf_value;
                                #endregion

                                Collected_Seq = Current_Seq + " " + Next_Sequences;
                                try
                                {
                                    var pos_seq_count = obj.Offline_Phases.pos_seq.Select(c => new { c.tags_seq, c.count }).Where(a => a.tags_seq.Equals(Collected_Seq)).ToList();
                                    count = double.Parse(pos_seq_count[0].count);
                                    #region To_Get_m-1_sequences
                                    if (m == 2)
                                    {
                                        next_value_m_minues_one = Current_Seq;
                                    }
                                    else
                                    {
                                        if (m == 3)
                                        {
                                            next_value_m_minues_one = Current_Seq + " " + Next_Seq;
                                        }
                                        else
                                        {
                                            if (m == 4)
                                            {
                                                next_value_m_minues_one = Current_Seq + " " + Next_Seq + " " + Next_Next_Seq;
                                            }
                                        }
                                    }
                                    var pos_seq_count_after = obj.Offline_Phases.pos_seq.Select(c => new { c.tags_seq, c.count }).Where(a => a.tags_seq.Equals(next_value_m_minues_one)).ToList();
                                    next_count = int.Parse(pos_seq_count_after[0].count);
                                    #endregion
                                }
                                catch { }

                                if (count > 1)
                                {
                                    #region Bayes' Rule
                                    #region Writh the Gram Value in Input file
                                    string path = "Input.txt";
                                    StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                                    sw.WriteLine(Collected_Seq);
                                    sw.Flush();
                                    sw.Close();
                                    #endregion

                                    #region Running cmd
                                    string strCmdText = "/C ngram.exe POS_Seq_KN.lm Input.txt > Output.txt";
                                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments = strCmdText;
                                    process.StartInfo = startInfo;
                                    process.Start();
                                    process.WaitForExit();
                                    process.Close();
                                    #endregion

                                    #region Get the Output value
                                    var lastLine = File.ReadLines("Output.txt").Last();
                                    double Output_Value = double.Parse(lastLine.Substring(lastLine.IndexOf("-"), lastLine.Length - 2));
                                    For_Best_Solution_Selection_List[best].Prop = +Math.Pow(10, Output_Value);
                                    #endregion
                                    #endregion
                                }

                                else
                                {
                                    #region Apply the back-off recursive procedure
                                    #region Writh the Gram Value in Input file
                                    string path = "Input.txt";
                                    StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                                    sw.WriteLine(Next_Sequences);
                                    sw.Flush();
                                    sw.Close();
                                    #endregion

                                    #region Running cmd
                                    string strCmdText = "/C ngram.exe POS_Seq_KN.lm Input.txt > Output.txt";
                                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                    startInfo.FileName = "cmd.exe";
                                    startInfo.Arguments = strCmdText;
                                    process.StartInfo = startInfo;
                                    process.Start();
                                    process.WaitForExit();
                                    process.Close();
                                    #endregion

                                    #region Get the Output value
                                    var lastLine = File.ReadLines("Output.txt").Last();
                                    if (!lastLine.Contains("#INF"))
                                    {
                                        double Output_Value = double.Parse(lastLine.Substring(lastLine.IndexOf("-"), lastLine.Length - 2));
                                        For_Best_Solution_Selection_List[best].Prop += Math.Pow(10, Output_Value);
                                    }
                                    #endregion
                                    #endregion
                                }
                            }
                        }
                        #endregion

                        if (!For_Best_Solution_Selection_List[0].affectedBy.Equals("OOV"))
                        {

                            #region Best_With_Relation_To_Its_Tag_Counts
                            current_word_tag = For_Best_Solution_Selection_List[0].word + "_";
                            string current_tag = ""; double word_tag_count = 0;
                            try
                            {
                                var current_word_tag_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(current_word_tag) && !a.word_form_variants.Contains(" ")).ToList();
                                current_word_tag_count.ToList().ForEach(u =>
                                {
                                    word_count = word_count + double.Parse(u.count);
                                });
                            }
                            catch { };

                            if (!word_count.Equals(0))
                            {
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    word_tag_count = 0;
                                    if (For_Best_Solution_Selection_List[best].stem.Contains("/"))
                                    {
                                        int tag_index = For_Best_Solution_Selection_List[best].stem.IndexOf("/");
                                        current_tag = For_Best_Solution_Selection_List[best].stem.Substring(tag_index + 1, For_Best_Solution_Selection_List[best].stem.Length - tag_index - 1);
                                    }
                                    else
                                    {
                                        current_tag = For_Best_Solution_Selection_List[best].stem;
                                    }
                                    try
                                    {
                                        var word_form_tag_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(current_word_tag) && a.word_form_variants.EndsWith(current_tag) && !a.word_form_variants.Contains(" ")).ToList();
                                        word_tag_count = double.Parse(word_form_tag_count[0].count);
                                    }
                                    catch { };
                                    //0.16430060760071027
                                    For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_tag_count / word_count)); /// Average;
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Previous_Word_Form_Lemma_Stem
                            current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                            current_word_form_counts = 0; string previous_word_lemma_stem = "";
                            if (Text_Analysis[IDs - 2].stem.Equals("<s>"))
                            {
                                previous_word_lemma_stem = "<s>";
                            }
                            else
                            {
                                previous_word_lemma_stem = Text_Analysis[IDs - 2].word + "_" + Text_Analysis[IDs - 2].lemmaID + "_" + Text_Analysis[IDs - 2].stem;
                            }
                            try
                            {
                                var current_word_form_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(Text_Analysis[IDs - 2].word + "_") && a.word_form_variants.Contains(" " + current_word_form)).ToList();
                                current_word_form_count.ToList().ForEach(u =>
                                {
                                    current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                });
                            }
                            catch { };

                            if (!current_word_form_counts.Equals(0))
                            {
                                //Average = Average + 1; 
                                double word_form_count_with_previous = 0; string word_froms_previous_values = "";
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    word_froms_previous_values = previous_word_lemma_stem + " " + For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem;
                                    word_form_count_with_previous = 0;
                                    try
                                    {
                                        var count_with_previous_word_lemma_stem = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_froms_previous_values)).ToList();
                                        word_form_count_with_previous = double.Parse(count_with_previous_word_lemma_stem[0].count);
                                    }
                                    catch { };

                                    For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count_with_previous / current_word_form_counts)); /// Average;
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Next_Word_Form_Lemma_Stem_Or_Word_Form_Only
                            current_word_form_counts = 0; current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                            try
                            {
                                var current_word_vriants_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(current_word_form) && a.word_form_variants.Contains(" " + Text_Analysis[IDs].word)).ToList();
                                current_word_vriants_count.ToList().ForEach(u =>
                                {
                                    current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                });
                            }
                            catch { };

                            if (!current_word_form_counts.Equals(0))
                            {
                                // Average = Average + 1;
                                string next_word = ""; double word_form_count_with_next = 0; string word_froms_next_values = "";
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    if (Text_Analysis[IDs].lemmaID.Equals("") && Text_Analysis[IDs].stem.Equals(""))
                                    {
                                        if (Text_Analysis[IDs].stem.Equals("</s>"))
                                        {
                                            next_word = "</s>";
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        }
                                        else
                                        {
                                            next_word = Text_Analysis[IDs].word + "_";
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        }
                                        word_form_count_with_next = 0;
                                        try
                                        {
                                            var count_with_next_word_lemma_stem = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Contains(word_froms_next_values)).ToList();
                                            count_with_next_word_lemma_stem.ToList().ForEach(u =>
                                            {
                                                word_form_count_with_next = word_form_count_with_next + double.Parse(u.count);
                                            });
                                        }
                                        catch { };
                                    }
                                    else if (!Text_Analysis[IDs].stem.Equals("") || (!Text_Analysis[IDs].pr.Equals("") && Text_Analysis[IDs].stem.Equals("")))
                                    {
                                        if (Text_Analysis[IDs].stem.Equals("</s>"))
                                        {
                                            next_word = "</s>";
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        }
                                        else
                                        {
                                            next_word = Text_Analysis[IDs].word + "_" + Text_Analysis[IDs].lemmaID + "_" + Text_Analysis[IDs].stem;
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word + "_";
                                        }
                                        word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        word_form_count_with_next = 0;
                                        try
                                        {
                                            var count_with_next_word_lemma_stem = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_froms_next_values)).ToList();
                                            word_form_count_with_next = double.Parse(count_with_next_word_lemma_stem[0].count);
                                        }
                                        catch { };
                                    }
                                    For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count_with_next / current_word_form_counts)); /// Average;
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Lemma_Stem_Of_Word_Itself
                            var best_solution = For_Best_Solution_Selection_List.Select(c => new { c.lemmaID, c.word, c.pr, c.stem, c.suf, c.spattern, c.Prop, c.affectedBy }).OrderBy(a => a.Prop).ToList();
                            if (best_solution[best_solution.Count - 1].Prop == best_solution[best_solution.Count - 2].Prop)
                            {
                                string word_lemma_stem = ""; double word_form_counts = 0; double word_form_count = 0;// Average = Average + 1;
                                string word_form = For_Best_Solution_Selection_List[0].word + "_";
                                var all_word_form_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(word_form) && !a.word_form_variants.Contains(" ")).ToList();
                                all_word_form_count.ToList().ForEach(u =>
                                {
                                    word_form_counts = word_form_counts + double.Parse(u.count);
                                });
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    word_form_count = 0;
                                    word_lemma_stem = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem;
                                    try
                                    {
                                        var word_lemma_stem_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_lemma_stem)).ToList();
                                        word_form_count = double.Parse(word_lemma_stem_count[0].count);
                                    }
                                    catch { };
                                    if (word_form_counts == 0)
                                    {
                                        For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (0)); /// Average;
                                    }

                                    else
                                    {
                                        For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count / word_form_counts)); /// Average;
                                    }
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Suf_Or_LemmaID
                            best_solution = For_Best_Solution_Selection_List.Select(c => new { c.lemmaID, c.word, c.pr, c.stem, c.suf, c.spattern, c.Prop, c.affectedBy }).OrderBy(a => a.Prop).ToList();
                            if (best_solution[best_solution.Count - 1].Prop == best_solution[best_solution.Count - 2].Prop)
                            {
                                #region Best_With_Relation_To_Stem_Or_Suf_Of_Word_Itself
                                if (best_solution[best_solution.Count - 1].lemmaID == best_solution[best_solution.Count - 2].lemmaID)
                                {
                                    double best_prop = best_solution[best_solution.Count - 1].Prop;
                                    var dis_sufixes = For_Best_Solution_Selection_List.Select(c => new { c.suf, c.stem, c.Prop }).Where(a => a.Prop.Equals(best_prop)).Distinct().ToList();

                                    #region Best_With_Relation_To_Its_Suf_Counts
                                    string current_word_suf = For_Best_Solution_Selection_List[0].word + "_";
                                    word_count = 0; double word_suf_count = 0;
                                    string tag_value = ""; string current_suf = "";
                                    if (best_solution[best_solution.Count - 1].stem.Contains("/"))
                                    {
                                        tag_value = "/" + best_solution[best_solution.Count - 1].stem.Split('/')[1].ToString();
                                    }
                                    else if (!best_solution[best_solution.Count - 1].stem.Contains("/"))
                                    {
                                        tag_value = "/" + best_solution[best_solution.Count - 1].stem.ToString();
                                    }
                                    try
                                    {
                                        var current_word_suf_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(current_word_suf) && a.word_stem_suf_vriants.EndsWith(tag_value) && !a.word_stem_suf_vriants.Contains(" ")).ToList();
                                        current_word_suf_count.ToList().ForEach(u =>
                                        {
                                            word_count = word_count + double.Parse(u.count);
                                        });
                                    }
                                    catch { };

                                    if (!word_count.Equals(0))
                                    {
                                        for (int best = 0; best < dis_sufixes.Count(); best++)
                                        {
                                            word_suf_count = 0;
                                            current_suf = dis_sufixes[best].suf;
                                            try
                                            {
                                                var word_form_suf_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(current_word_tag) && a.word_stem_suf_vriants.Contains(current_suf) && a.word_stem_suf_vriants.EndsWith(tag_value) && !a.word_stem_suf_vriants.Contains(" ")).ToList();
                                                word_form_suf_count.ToList().ForEach(u =>
                                                {
                                                    word_suf_count = word_suf_count + double.Parse(u.count);
                                                });
                                            }
                                            catch { };
                                            var update_suf = For_Best_Solution_Selection_List.Where(d => d.suf == current_suf).FirstOrDefault();
                                            For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop = For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop + (word_suf_count / word_count);
                                            //For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_tag_count / word_count)); /// Average;
                                        }
                                    }
                                    #endregion

                                    #region Best_With_Relation_To_Previous_Word_Form_Stem_Suf
                                    current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                                    current_word_form_counts = 0; string previous_word_stem_suf = "";
                                    if (Text_Analysis[IDs - 2].stem.Equals("<s>") || Text_Analysis[IDs - 2].stem.Equals("</s> <s>"))
                                    {
                                        previous_word_stem_suf = "<s>";
                                    }
                                    else
                                    {
                                        previous_word_stem_suf = Text_Analysis[IDs - 2].word + "_" + Text_Analysis[IDs - 2].suf + "_" + Text_Analysis[IDs - 2].stem;
                                    }
                                    try
                                    {
                                        var current_word_form_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(previous_word_stem_suf) && a.word_stem_suf_vriants.Contains(" " + current_word_form)).ToList();
                                        current_word_form_count.ToList().ForEach(u =>
                                        {
                                            current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                        });
                                    }
                                    catch { };

                                    if (!current_word_form_counts.Equals(0))
                                    {
                                        double word_form_count_with_previous = 0; string word_froms_previous_values = "";
                                        for (int best = 0; best < dis_sufixes.Count(); best++)
                                        {
                                            current_suf = dis_sufixes[best].suf;
                                            word_froms_previous_values = previous_word_stem_suf + " " + current_word_form + dis_sufixes[best].suf + "_" + dis_sufixes[best].stem;
                                            word_form_count_with_previous = 0;
                                            try
                                            {
                                                var count_with_previous_word_stem_suf = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.Equals(word_froms_previous_values)).ToList();
                                                word_form_count_with_previous = double.Parse(count_with_previous_word_stem_suf[0].count);
                                                var update_suf = For_Best_Solution_Selection_List.Where(d => d.suf == current_suf).FirstOrDefault();
                                                For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop = For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop + (word_form_count_with_previous / current_word_form_counts);
                                            }
                                            catch { };
                                        }
                                    }
                                    #endregion

                                    #region Best_With_Relation_To_Next_Word_Form_Suf_Stem_Or_Word_Form_Only
                                    current_word_form_counts = 0; current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                                    try
                                    {
                                        var current_word_vriants_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(current_word_form) && a.word_stem_suf_vriants.Contains(tag_value + " " + Text_Analysis[IDs].word + "_")).ToList();
                                        current_word_vriants_count.ToList().ForEach(u =>
                                        {
                                            current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                        });
                                    }
                                    catch { };

                                    if (!current_word_form_counts.Equals(0))
                                    {
                                        string next_word = ""; double word_form_count_with_next = 0; string word_froms_next_values = "";
                                        for (int best = 0; best < dis_sufixes.Count(); best++)
                                        {
                                            current_suf = dis_sufixes[best].suf;
                                            if (Text_Analysis[IDs].lemmaID.Equals("") && Text_Analysis[IDs].stem.Equals(""))
                                            {
                                                next_word = Text_Analysis[IDs].word + "_";
                                                word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].suf + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                                word_form_count_with_next = 0;
                                                try
                                                {
                                                    var count_with_next_word_lemma_stem = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.Contains(word_froms_next_values)).ToList();
                                                    count_with_next_word_lemma_stem.ToList().ForEach(u =>
                                                    {
                                                        word_form_count_with_next = word_form_count_with_next + double.Parse(u.count);
                                                    });
                                                }
                                                catch { };
                                            }
                                            else if (!Text_Analysis[IDs].stem.Equals("") || (!Text_Analysis[IDs].pr.Equals("") && Text_Analysis[IDs].stem.Equals("")))
                                            {
                                                if (Text_Analysis[IDs].stem.Equals("</s>") || Text_Analysis[IDs].stem.Equals("</s> <s>"))
                                                {
                                                    next_word = "</s>";
                                                    word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                                }
                                                else
                                                {
                                                    next_word = Text_Analysis[IDs].word + "_" + Text_Analysis[IDs].suf + "_" + Text_Analysis[IDs].stem;
                                                    word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].suf + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word + "_";
                                                }
                                                word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].suf + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                                word_form_count_with_next = 0;
                                                try
                                                {
                                                    var count_with_next_word_lemma_stem = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.Equals(word_froms_next_values)).ToList();
                                                    word_form_count_with_next = double.Parse(count_with_next_word_lemma_stem[0].count);
                                                }
                                                catch { };
                                            }
                                            var update_suf = For_Best_Solution_Selection_List.Where(d => d.suf == current_suf).FirstOrDefault();
                                            For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop = For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop + (word_form_count_with_next / current_word_form_counts);
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Best_With_Relation_To_Lemma_Stem_Of_Word_Itself
                                else
                                {
                                    string word_lemma_stem = ""; double word_form_counts = 0; double word_form_count = 0;// Average = Average + 1;
                                    string word_form = For_Best_Solution_Selection_List[0].word + "_";
                                    var all_word_form_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(word_form) && !a.word_form_variants.Contains(" ")).ToList();
                                    all_word_form_count.ToList().ForEach(u =>
                                    {
                                        word_form_counts = word_form_counts + double.Parse(u.count);
                                    });
                                    for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                    {
                                        word_form_count = 0;
                                        word_lemma_stem = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem;
                                        try
                                        {
                                            var word_lemma_stem_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_lemma_stem)).ToList();
                                            word_form_count = double.Parse(word_lemma_stem_count[0].count);
                                        }
                                        catch { };
                                        if (word_form_counts == 0)
                                        {
                                            For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (0)); /// Average;
                                        }

                                        else
                                        {
                                            For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count / word_form_counts)); /// Average;
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region Where_Distinct_Tags_Equals_One
                    else
                    {
                        if (!For_Best_Solution_Selection_List[0].affectedBy.Equals("OOV"))
                        {
                            #region Best_With_Relation_To_Previous_Word_Form_Lemma_Stem
                            current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                            current_word_form_counts = 0; string previous_word_lemma_stem = "";
                            if (Text_Analysis[IDs - 2].stem.Equals("<s>"))
                            {
                                previous_word_lemma_stem = "<s>";
                            }
                            else
                            {
                                previous_word_lemma_stem = Text_Analysis[IDs - 2].word + "_" + Text_Analysis[IDs - 2].lemmaID + "_" + Text_Analysis[IDs - 2].stem;
                            }
                            try
                            {
                                var current_word_form_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(previous_word_lemma_stem) && a.word_form_variants.Contains(" ")).ToList();
                                current_word_form_count.ToList().ForEach(u =>
                                {
                                    current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                });
                            }
                            catch { };

                            if (!current_word_form_counts.Equals(0))
                            {
                                //Average = Average + 1; 
                                double word_form_count_with_previous = 0; string word_froms_previous_values = "";
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    word_froms_previous_values = previous_word_lemma_stem + " " + For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem;
                                    word_form_count_with_previous = 0;
                                    try
                                    {
                                        var count_with_previous_word_lemma_stem = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_froms_previous_values)).ToList();
                                        word_form_count_with_previous = double.Parse(count_with_previous_word_lemma_stem[0].count);
                                    }
                                    catch { };

                                    For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count_with_previous / current_word_form_counts)); /// Average;
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Next_Word_Form_Lemma_Stem_Or_Word_Form_Only
                            current_word_form_counts = 0; current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                            try
                            {
                                var current_word_vriants_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(current_word_form) && a.word_form_variants.Contains(" " + Text_Analysis[IDs].word + "_")).ToList();
                                current_word_vriants_count.ToList().ForEach(u =>
                                {
                                    current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                });
                            }
                            catch { };

                            if (!current_word_form_counts.Equals(0))
                            {
                                // Average = Average + 1;
                                string next_word = ""; double word_form_count_with_next = 0; string word_froms_next_values = "";
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    if (Text_Analysis[IDs].lemmaID.Equals("") && Text_Analysis[IDs].stem.Equals(""))
                                    {
                                        if (Text_Analysis[IDs].stem.Equals("</s>"))
                                        {
                                            next_word = "</s>";
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        }
                                        else
                                        {
                                            next_word = Text_Analysis[IDs].word + "_";
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        }
                                        word_form_count_with_next = 0;
                                        try
                                        {
                                            var count_with_next_word_lemma_stem = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Contains(word_froms_next_values)).ToList();
                                            count_with_next_word_lemma_stem.ToList().ForEach(u =>
                                            {
                                                word_form_count_with_next = word_form_count_with_next + double.Parse(u.count);
                                            });
                                        }
                                        catch { };
                                    }
                                    else if (!Text_Analysis[IDs].stem.Equals("") || (!Text_Analysis[IDs].pr.Equals("") && Text_Analysis[IDs].stem.Equals("")))
                                    {
                                        if (Text_Analysis[IDs].stem.Equals("</s>"))
                                        {
                                            next_word = "</s>";
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        }
                                        else
                                        {
                                            next_word = Text_Analysis[IDs].word + "_" + Text_Analysis[IDs].lemmaID + "_" + Text_Analysis[IDs].stem;
                                            word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word + "_";
                                        }
                                        word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                        word_form_count_with_next = 0;
                                        try
                                        {
                                            var count_with_next_word_lemma_stem = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_froms_next_values)).ToList();
                                            word_form_count_with_next = double.Parse(count_with_next_word_lemma_stem[0].count);
                                        }
                                        catch { };
                                    }
                                    For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count_with_next / current_word_form_counts)); /// Average;
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Lemma_Stem_Of_Word_Itself
                            var best_solution = For_Best_Solution_Selection_List.Select(c => new { c.lemmaID, c.word, c.pr, c.stem, c.suf, c.spattern, c.Prop, c.affectedBy }).OrderBy(a => a.Prop).ToList();
                            if (best_solution[best_solution.Count - 1].Prop == best_solution[best_solution.Count - 2].Prop)
                            {
                                string word_lemma_stem = ""; double word_form_counts = 0; double word_form_count = 0;// Average = Average + 1;
                                string word_form = For_Best_Solution_Selection_List[0].word + "_";
                                var all_word_form_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(word_form) && !a.word_form_variants.Contains(" ")).ToList();
                                all_word_form_count.ToList().ForEach(u =>
                                {
                                    word_form_counts = word_form_counts + double.Parse(u.count);
                                });
                                for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                {
                                    word_form_count = 0;
                                    word_lemma_stem = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem;
                                    try
                                    {
                                        var word_lemma_stem_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_lemma_stem)).ToList();
                                        word_form_count = double.Parse(word_lemma_stem_count[0].count);
                                    }
                                    catch { };
                                    if (word_form_counts == 0)
                                    {
                                        For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (0)); /// Average;
                                    }

                                    else
                                    {
                                        For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count / word_form_counts)); /// Average;
                                    }
                                }
                            }
                            #endregion

                            #region Best_With_Relation_To_Suf_Or_LemmaID
                            best_solution = For_Best_Solution_Selection_List.Select(c => new { c.lemmaID, c.word, c.pr, c.stem, c.suf, c.spattern, c.Prop, c.affectedBy }).OrderBy(a => a.Prop).ToList();
                            if (best_solution[best_solution.Count - 1].Prop == best_solution[best_solution.Count - 2].Prop)
                            {
                                #region Best_With_Relation_To_Stem_Or_Suf_Of_Word_Itself
                                if (best_solution[best_solution.Count - 1].lemmaID == best_solution[best_solution.Count - 2].lemmaID)
                                {
                                    double best_prop = best_solution[best_solution.Count - 1].Prop;
                                    var dis_sufixes = For_Best_Solution_Selection_List.Select(c => new { c.suf, c.stem, c.Prop }).Where(a => a.Prop.Equals(best_prop)).Distinct().ToList();

                                    #region Best_With_Relation_To_Its_Suf_Counts
                                    string current_word_suf = For_Best_Solution_Selection_List[0].word + "_";
                                    word_count = 0; double word_suf_count = 0;
                                    string tag_value = ""; string current_suf = "";
                                    if (best_solution[best_solution.Count - 1].stem.Contains("/"))
                                    {
                                        tag_value = "/" + best_solution[best_solution.Count - 1].stem.Split('/')[1].ToString();
                                    }
                                    else if (!best_solution[best_solution.Count - 1].stem.Contains("/"))
                                    {
                                        tag_value = "/" + best_solution[best_solution.Count - 1].stem.ToString();
                                    }
                                    try
                                    {
                                        var current_word_suf_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(current_word_suf) && a.word_stem_suf_vriants.EndsWith(tag_value) && !a.word_stem_suf_vriants.Contains(" ")).ToList();
                                        current_word_suf_count.ToList().ForEach(u =>
                                        {
                                            word_count = word_count + double.Parse(u.count);
                                        });
                                    }
                                    catch { };

                                    if (!word_count.Equals(0))
                                    {
                                        for (int best = 0; best < dis_sufixes.Count(); best++)
                                        {
                                            word_suf_count = 0;
                                            current_suf = dis_sufixes[best].suf;
                                            try
                                            {
                                                var word_form_suf_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(current_word_tag) && a.word_stem_suf_vriants.Contains(current_suf) && a.word_stem_suf_vriants.EndsWith(tag_value) && !a.word_stem_suf_vriants.Contains(" ")).ToList();
                                                word_form_suf_count.ToList().ForEach(u =>
                                                {
                                                    word_suf_count = word_suf_count + double.Parse(u.count);
                                                });
                                            }
                                            catch { };
                                            var update_suf = For_Best_Solution_Selection_List.Where(d => d.suf == current_suf).FirstOrDefault();
                                            For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop = For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop + (word_suf_count / word_count);
                                            //For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_tag_count / word_count)); /// Average;
                                        }
                                    }
                                    #endregion

                                    #region Best_With_Relation_To_Previous_Word_Form_Stem_Suf
                                    current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                                    current_word_form_counts = 0; string previous_word_stem_suf = "";
                                    if (Text_Analysis[IDs - 2].stem.Equals("<s>") || Text_Analysis[IDs - 2].stem.Equals("</s> <s>"))
                                    {
                                        previous_word_stem_suf = "<s>";
                                    }
                                    else
                                    {
                                        previous_word_stem_suf = Text_Analysis[IDs - 2].word + "_" + Text_Analysis[IDs - 2].suf + "_" + Text_Analysis[IDs - 2].stem;
                                    }
                                    try
                                    {
                                        var current_word_form_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(previous_word_stem_suf) && a.word_stem_suf_vriants.Contains(" " + current_word_form)).ToList();
                                        current_word_form_count.ToList().ForEach(u =>
                                        {
                                            current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                        });
                                    }
                                    catch { };

                                    if (!current_word_form_counts.Equals(0))
                                    {
                                        double word_form_count_with_previous = 0; string word_froms_previous_values = "";
                                        for (int best = 0; best < dis_sufixes.Count(); best++)
                                        {
                                            current_suf = dis_sufixes[best].suf;
                                            word_froms_previous_values = previous_word_stem_suf + " " + current_word_form + dis_sufixes[best].suf + "_" + dis_sufixes[best].stem;
                                            word_form_count_with_previous = 0;
                                            try
                                            {
                                                var count_with_previous_word_stem_suf = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.Equals(word_froms_previous_values)).ToList();
                                                word_form_count_with_previous = double.Parse(count_with_previous_word_stem_suf[0].count);
                                                var update_suf = For_Best_Solution_Selection_List.Where(d => d.suf == current_suf).FirstOrDefault();
                                                For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop = For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop + (word_form_count_with_previous / current_word_form_counts);
                                            }
                                            catch { };
                                        }
                                    }
                                    #endregion

                                    #region Best_With_Relation_To_Next_Word_Form_Suf_Stem_Or_Word_Form_Only
                                    current_word_form_counts = 0; current_word_form = For_Best_Solution_Selection_List[0].word + "_";
                                    try
                                    {
                                        var current_word_vriants_count = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.StartsWith(current_word_form) && a.word_stem_suf_vriants.Contains(tag_value + " " + Text_Analysis[IDs].word + "_")).ToList();
                                        current_word_vriants_count.ToList().ForEach(u =>
                                        {
                                            current_word_form_counts = current_word_form_counts + double.Parse(u.count);
                                        });
                                    }
                                    catch { };

                                    if (!current_word_form_counts.Equals(0))
                                    {
                                        string next_word = ""; double word_form_count_with_next = 0; string word_froms_next_values = "";
                                        for (int best = 0; best < dis_sufixes.Count(); best++)
                                        {
                                            current_suf = dis_sufixes[best].suf;
                                            if (Text_Analysis[IDs].lemmaID.Equals("") && Text_Analysis[IDs].stem.Equals(""))
                                            {
                                                next_word = Text_Analysis[IDs].word + "_";
                                                word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].suf + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                                word_form_count_with_next = 0;
                                                try
                                                {
                                                    var count_with_next_word_lemma_stem = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.Contains(word_froms_next_values)).ToList();
                                                    count_with_next_word_lemma_stem.ToList().ForEach(u =>
                                                    {
                                                        word_form_count_with_next = word_form_count_with_next + double.Parse(u.count);
                                                    });
                                                }
                                                catch { };
                                            }
                                            else if (!Text_Analysis[IDs].stem.Equals("") || (!Text_Analysis[IDs].pr.Equals("") && Text_Analysis[IDs].stem.Equals("")))
                                            {
                                                if (Text_Analysis[IDs].stem.Equals("</s>") || Text_Analysis[IDs].stem.Equals("</s> <s>"))
                                                {
                                                    next_word = "</s>";
                                                    word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                                }
                                                else
                                                {
                                                    next_word = Text_Analysis[IDs].word + "_" + Text_Analysis[IDs].suf + "_" + Text_Analysis[IDs].stem;
                                                    word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].suf + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word + "_";
                                                }
                                                word_froms_next_values = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].suf + "_" + For_Best_Solution_Selection_List[best].stem + " " + next_word;
                                                word_form_count_with_next = 0;
                                                try
                                                {
                                                    var count_with_next_word_lemma_stem = obj.Word_Stem_Suf.suf_forms.Select(s => new { s.word_stem_suf_vriants, s.count }).Where(a => a.word_stem_suf_vriants.Equals(word_froms_next_values)).ToList();
                                                    word_form_count_with_next = double.Parse(count_with_next_word_lemma_stem[0].count);
                                                }
                                                catch { };
                                            }
                                            var update_suf = For_Best_Solution_Selection_List.Where(d => d.suf == current_suf).FirstOrDefault();
                                            For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop = For_Best_Solution_Selection_List.First(d => d.suf == current_suf).Prop + (word_form_count_with_next / current_word_form_counts);
                                        }
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Best_With_Relation_To_Lemma_Stem_Of_Word_Itself
                                else
                                {
                                    string word_lemma_stem = ""; double word_form_counts = 0; double word_form_count = 0;// Average = Average + 1;
                                    string word_form = For_Best_Solution_Selection_List[0].word + "_";
                                    var all_word_form_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.StartsWith(word_form) && !a.word_form_variants.Contains(" ")).ToList();
                                    all_word_form_count.ToList().ForEach(u =>
                                    {
                                        word_form_counts = word_form_counts + double.Parse(u.count);
                                    });
                                    for (int best = 0; best < For_Best_Solution_Selection_List.Count(); best++)
                                    {
                                        word_form_count = 0;
                                        word_lemma_stem = For_Best_Solution_Selection_List[best].word + "_" + For_Best_Solution_Selection_List[best].lemmaID + "_" + For_Best_Solution_Selection_List[best].stem;
                                        try
                                        {
                                            var word_lemma_stem_count = obj.Word_Lemma_Stem.word_forms.Select(s => new { s.word_form_variants, s.count }).Where(a => a.word_form_variants.Equals(word_lemma_stem)).ToList();
                                            word_form_count = double.Parse(word_lemma_stem_count[0].count);
                                        }
                                        catch { };
                                        if (word_form_counts == 0)
                                        {
                                            For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (0)); /// Average;
                                        }

                                        else
                                        {
                                            For_Best_Solution_Selection_List[best].Prop = (For_Best_Solution_Selection_List[best].Prop + (word_form_count / word_form_counts)); /// Average;
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region Best_Of_All
                    var best_solutions = For_Best_Solution_Selection_List.Select(c => new { c.lemmaID, c.word, c.pr, c.stem, c.suf, c.spattern, c.Prop, c.affectedBy }).OrderBy(a => a.Prop).ToList();
                    Text_Analysis[IDs - 1].lemmaID = best_solutions[best_solutions.Count - 1].lemmaID;
                    Text_Analysis[IDs - 1].pr = best_solutions[best_solutions.Count - 1].pr;
                    Text_Analysis[IDs - 1].stem = best_solutions[best_solutions.Count - 1].stem;
                    Text_Analysis[IDs - 1].suf = best_solutions[best_solutions.Count - 1].suf;
                    Text_Analysis[IDs - 1].spattern = best_solutions[best_solutions.Count - 1].spattern;
                    Text_Analysis[IDs - 1].affectedBy = best_solutions[best_solutions.Count - 1].affectedBy + Rule_Number + "_Statistics";
                    best_solutions.Clear();
                    #endregion
                }
            }
            #endregion

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Threading;

namespace Arabic_Automatic_Diacritization
{
    class Running_BAMA_And_Get_Text_Solutions
    {
        #region variables and constructor
        public static string buckWalter = "Diac_Bucwalter\\";
        List<string> Text_Solutions_Values = new List<string>();
        public List<Text_Solutions> Text_Solutions_List = new List<Text_Solutions>();
        public List<Distinct_Word_For_BAMA> Distinct_Word_For_BAMA_List = new List<Distinct_Word_For_BAMA>();

        public AraMorph_output Diac_Output = new AraMorph_output();
        public XmlDocument Diac_Output_Table = new XmlDocument();
        string OOV_Word;
        #endregion

        public Running_BAMA_And_Get_Text_Solutions()
        {
        }
        public void BAMA_Solutions(string Edited_Text, out List<Text_Solutions> Text_Solutions, Form1 obj)
        {
            List<string> Dist_Words = new List<string>();
            string Text_Value = Edited_Text; string[] Words_List = Text_Value.Split(' '); Text_Value = "";
            List<string> beta_words = new List<string>();
            
            #region To_Get_Distinct_Arabic_Words_Only
            foreach (string Each_Word in Words_List)
            {
                string Word = Each_Word;
                if (Regex.IsMatch(Word, @"[ء-ي]"))
                {
                    var New_Words_List = obj.Diac_Lexicon.solution.Where(s => s.word == Word).Distinct().ToList();
                    if (New_Words_List.Count != 0)
                    {
                        foreach (var item in New_Words_List)
                        {
                            Text_Solutions Text_Solutions_List_Values = new Text_Solutions();
                            Text_Solutions_List_Values.word = item.word;
                            Text_Solutions_List_Values.lemmaID = item.lemmaID;
                            Text_Solutions_List_Values.pr = item.pr;
                            Text_Solutions_List_Values.stem = item.stem;
                            Text_Solutions_List_Values.suf = item.suf;
                            Text_Solutions_List_Values.spattern = item.spattern;
                            Text_Solutions_List_Values.affectedBy = "Diac_Lex";
                            Text_Solutions_List.Add(Text_Solutions_List_Values);
                        }
                    }
                    else
                    {
                        Distinct_Word_For_BAMA Distinct_Words = new Distinct_Word_For_BAMA();
                        var Distinct_Words_List = Distinct_Word_For_BAMA_List.Select(s => new {s.word }).Where(a => a.word == Word).ToList();
                        if (Distinct_Words_List.Count() == 0)
                        {
                            Distinct_Words.word = Word;
                            Distinct_Word_For_BAMA_List.Add(Distinct_Words);
                            Text_Value = Text_Value + Word + " ";
                        }
                    }
                    beta_words.Add(Word);
                }
            }
            #endregion

            #region Running_BAMA
            var dis_words = beta_words.Distinct();
            //Text_Value = Text_Value + Word + " ";
            bool suceseded = writeToFile(Text_Value);
            string BAMA_Diac_Output = @buckWalter + "Diac_Output.xml";
            if (!Text_Value.Equals(""))
            {
                runBuckwalter();

                while (CheckIfFileIsBeingUsed(BAMA_Diac_Output))
                {
                    //Loop till the file is available.
                }

                #region Loading Diac_Output Solutions
                StreamReader Diac_Output_Stream = new StreamReader("Diac_Bucwalter\\Diac_Output.xml");
                Diac_Output_Table.Load(Diac_Output_Stream);
                XmlSerializer Diac_Lexicon_Serializer = new XmlSerializer(typeof(AraMorph_output));
                using (StringReader Diac_Output_Reader = new StringReader(Diac_Output_Table.InnerXml))
                {
                    Diac_Output = (AraMorph_output)(Diac_Lexicon_Serializer.Deserialize(Diac_Output_Reader));
                }
                Diac_Output_Stream.Close();
                #endregion

                #region Add_New_Solution_To_Text_Splution_List
                string[] New_Words_List = Text_Value.Split(' '); Text_Value = "";
                foreach (string Each_Word in New_Words_List)
                {
                    string Word = Each_Word;
                    try
                    {
                        var New_Solutions_List = Diac_Output.solution.Where(a => a.word == Word).Distinct().ToList();
                        #region There are solutions for the word
                        if (!New_Solutions_List.Count.Equals(0))
                        {
                            foreach (var BAMA in New_Solutions_List)
                            {
                                Text_Solutions Text_Solutions_List_Values = new Text_Solutions();
                                Text_Solutions_List_Values.word = BAMA.word;
                                Text_Solutions_List_Values.lemmaID = BAMA.lemmaID;
                                Text_Solutions_List_Values.pr = BAMA.pr;
                                Text_Solutions_List_Values.stem = BAMA.stem;
                                if (BAMA.suf.Equals("AF/NOUN"))
                                {
                                    BAMA.suf = "";
                                }
                                if (Text_Solutions_List_Values.pr.Equals("") && Text_Solutions_List_Values.stem.Equals("Al/DET"))
                                {
                                    Text_Solutions_List_Values.pr = "Al/DET";
                                    Text_Solutions_List_Values.stem = "";
                                }
                                Text_Solutions_List_Values.suf = BAMA.suf;
                                Text_Solutions_List_Values.spattern = BAMA.spattern;
                                Text_Solutions_List_Values.affectedBy = "BAMA";
                                Text_Solutions_List.Add(Text_Solutions_List_Values);
                            }
                        }
                        else
                        {
                            #region OOV Words
                            var OOV_Prf_Suf = obj.Compatibility.combatibilities.Where(s => Word.StartsWith(s.pref) && Word.EndsWith(s.suf)).Distinct().ToList();
                            if (OOV_Prf_Suf.Count != 0)
                            {
                                foreach (var item in OOV_Prf_Suf)
                                {
                                    if (!item.pref.Equals("") && !item.suf.Equals(""))
                                    {
                                        int pref_indix = Word.IndexOf(item.pref) + item.pref.Length;
                                        int suf_indix = Word.LastIndexOf(item.suf);
                                        OOV_Word = Word.Substring(pref_indix, suf_indix - item.pref.Length);
                                    }
                                    else if (item.pref.Equals("") && !item.suf.Equals(""))
                                    {
                                        int suf_indix = Word.LastIndexOf(item.suf);
                                        OOV_Word = Word.Substring(0, suf_indix);
                                    }
                                    else if (!item.pref.Equals("") && item.suf.Equals(""))
                                    {
                                        int pref_indix = Word.IndexOf(item.pref) + item.pref.Length;
                                        OOV_Word = Word.Substring(pref_indix, Word.Length - pref_indix);
                                    }
                                    else
                                    {
                                        OOV_Word = Word;
                                    }

                                    #region Placeholder
                                    string Arabic_Word = OOV_Word;
                                    Arabic_Word = Arabic_Word.Replace("ا", "A");
                                    Arabic_Word = Arabic_Word.Replace("آ", "|");
                                    Arabic_Word = Arabic_Word.Replace("أ", ">");
                                    Arabic_Word = Arabic_Word.Replace("إ", "<");
                                    Arabic_Word = Arabic_Word.Replace("ؤ", "&");
                                    Arabic_Word = Arabic_Word.Replace("ئ", "}");
                                    Arabic_Word = Arabic_Word.Replace("ء", "'");
                                    Arabic_Word = Arabic_Word.Replace("ي", "y");
                                    Arabic_Word = Arabic_Word.Replace("ى", "Y");
                                    Arabic_Word = Arabic_Word.Replace("و", "w");
                                    Arabic_Word = Arabic_Word.Replace("ة", "p");
                                    Arabic_Word = Arabic_Word.Replace("ب", "b");
                                    Arabic_Word = Arabic_Word.Replace("ت", "t");
                                    Arabic_Word = Arabic_Word.Replace("ث", "v");
                                    Arabic_Word = Arabic_Word.Replace("ج", "j");
                                    Arabic_Word = Arabic_Word.Replace("ح", "H");
                                    Arabic_Word = Arabic_Word.Replace("خ", "x");
                                    Arabic_Word = Arabic_Word.Replace("د", "d");
                                    Arabic_Word = Arabic_Word.Replace("ذ", "*");
                                    Arabic_Word = Arabic_Word.Replace("ر", "r");
                                    Arabic_Word = Arabic_Word.Replace("ز", "z");
                                    Arabic_Word = Arabic_Word.Replace("س", "s");
                                    Arabic_Word = Arabic_Word.Replace("ش", "$");
                                    Arabic_Word = Arabic_Word.Replace("ص", "S");
                                    Arabic_Word = Arabic_Word.Replace("ض", "D");
                                    Arabic_Word = Arabic_Word.Replace("ط", "T");
                                    Arabic_Word = Arabic_Word.Replace("ظ", "Z");
                                    Arabic_Word = Arabic_Word.Replace("ع", "E");
                                    Arabic_Word = Arabic_Word.Replace("غ", "g");
                                    Arabic_Word = Arabic_Word.Replace("ف", "f");
                                    Arabic_Word = Arabic_Word.Replace("ق", "q");
                                    Arabic_Word = Arabic_Word.Replace("ك", "k");
                                    Arabic_Word = Arabic_Word.Replace("ل", "l");
                                    Arabic_Word = Arabic_Word.Replace("م", "m");
                                    Arabic_Word = Arabic_Word.Replace("ن", "n");
                                    Arabic_Word = Arabic_Word.Replace("ه", "h");

                                    OOV_Word = OOV_Word.Replace("ب", "-");
                                    OOV_Word = OOV_Word.Replace("ت", "-");
                                    OOV_Word = OOV_Word.Replace("ث", "-");
                                    OOV_Word = OOV_Word.Replace("ج", "-");
                                    OOV_Word = OOV_Word.Replace("ح", "-");
                                    OOV_Word = OOV_Word.Replace("خ", "-");
                                    OOV_Word = OOV_Word.Replace("د", "-");
                                    OOV_Word = OOV_Word.Replace("ذ", "-");
                                    OOV_Word = OOV_Word.Replace("ر", "-");
                                    OOV_Word = OOV_Word.Replace("ز", "-");
                                    OOV_Word = OOV_Word.Replace("س", "-");
                                    OOV_Word = OOV_Word.Replace("ش", "-");
                                    OOV_Word = OOV_Word.Replace("ص", "-");
                                    OOV_Word = OOV_Word.Replace("ض", "-");
                                    OOV_Word = OOV_Word.Replace("ط", "-");
                                    OOV_Word = OOV_Word.Replace("ظ", "-");
                                    OOV_Word = OOV_Word.Replace("ع", "-");
                                    OOV_Word = OOV_Word.Replace("غ", "-");
                                    OOV_Word = OOV_Word.Replace("ف", "-");
                                    OOV_Word = OOV_Word.Replace("ق", "-");
                                    OOV_Word = OOV_Word.Replace("ك", "-");
                                    OOV_Word = OOV_Word.Replace("ل", "-");
                                    OOV_Word = OOV_Word.Replace("م", "-");
                                    OOV_Word = OOV_Word.Replace("ن", "-");
                                    OOV_Word = OOV_Word.Replace("ه", "-");
                                    OOV_Word = OOV_Word.Replace("ا", "A");
                                    OOV_Word = OOV_Word.Replace("آ", "|");
                                    OOV_Word = OOV_Word.Replace("أ", ">");
                                    OOV_Word = OOV_Word.Replace("إ", "<");
                                    OOV_Word = OOV_Word.Replace("ؤ", "&");
                                    OOV_Word = OOV_Word.Replace("ئ", "}");
                                    OOV_Word = OOV_Word.Replace("ء", "'");
                                    OOV_Word = OOV_Word.Replace("ي", "y");
                                    OOV_Word = OOV_Word.Replace("ى", "Y");
                                    OOV_Word = OOV_Word.Replace("و", "w");
                                    OOV_Word = OOV_Word.Replace("ة", "p");
                                    #endregion

                                    try
                                    {
                                        var OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => s.OOV_Patterns.Equals(OOV_Word) && s.Tags.Equals(item.tags)).OrderByDescending(a => a.Count).ToList();
                                        if (!OOV_Pattern.Count.Equals(0))
                                        {
                                            if (item.tags.Contains("PV") && OOV_Word.EndsWith("A") && item.suffixes.Equals("a/V_SUF"))
                                            {
                                            }
                                            else if (item.tags.Contains("NOUN") && OOV_Word.EndsWith("A") && (item.suffixes.Equals("i/N_SUF") || item.suffixes.Equals("a/N_SUF") || item.suffixes.Equals("~a/POSS_SUF")))
                                            {
                                            }
                                            else
                                            {
                                                Text_Solutions Text_Solutions_List_Values = new Text_Solutions();
                                                Text_Solutions_List_Values.word = Word;
                                                Text_Solutions_List_Values.lemmaID = "";
                                                Text_Solutions_List_Values.pr = item.prefixes;
                                                Text_Solutions_List_Values.stem = OOV_Word + "/" + item.tags;
                                                Text_Solutions_List_Values.suf = item.suffixes;
                                                Text_Solutions_List_Values.spattern = Arabic_Word;
                                                Text_Solutions_List_Values.affectedBy = "OOV";
                                                Text_Solutions_List.Add(Text_Solutions_List_Values);
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => OOV_Word.Equals(s.OOV_Patterns)).OrderByDescending(a => a.Count).ToList();
                                                if (!OOV_Pattern.Count.Equals(0))
                                                {
                                                    if (item.tags.Contains("PV") && OOV_Word.EndsWith("A") && item.suffixes.Equals("a/V_SUF"))
                                                    {
                                                    }
                                                    else if (item.tags.Contains("NOUN") && OOV_Word.EndsWith("A") && (item.suffixes.Equals("i/N_SUF") || item.suffixes.Equals("a/N_SUF") || item.suffixes.Equals("~a/POSS_SUF")))
                                                    {
                                                    }
                                                    else
                                                    {
                                                        Text_Solutions Text_Solutions_List_Values = new Text_Solutions();
                                                        Text_Solutions_List_Values.word = Word;
                                                        Text_Solutions_List_Values.lemmaID = "";
                                                        Text_Solutions_List_Values.pr = item.prefixes;
                                                        Text_Solutions_List_Values.stem = OOV_Word + "/" + item.tags;
                                                        Text_Solutions_List_Values.suf = item.suffixes;
                                                        Text_Solutions_List_Values.spattern = Arabic_Word;
                                                        Text_Solutions_List_Values.affectedBy = "OOV";
                                                        Text_Solutions_List.Add(Text_Solutions_List_Values);
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    catch { }
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    catch {
                        #region OOV Words
                        var OOV_Prf_Suf = obj.Compatibility.combatibilities.Where(s => Word.StartsWith(s.pref) && Word.EndsWith(s.suf)).Distinct().ToList();
                        if (OOV_Prf_Suf.Count != 0)
                        {
                            foreach (var item in OOV_Prf_Suf)
                            {
                                if (!item.pref.Equals("") && !item.suf.Equals(""))
                                {
                                    int pref_indix = Word.IndexOf(item.pref) + item.pref.Length;
                                    int suf_indix = Word.LastIndexOf(item.suf);
                                    OOV_Word = Word.Substring(pref_indix, suf_indix - item.pref.Length);
                                }
                                else if (item.pref.Equals("") && !item.suf.Equals(""))
                                {
                                    int suf_indix = Word.LastIndexOf(item.suf);
                                    OOV_Word = Word.Substring(0, suf_indix);
                                }
                                else if (!item.pref.Equals("") && item.suf.Equals(""))
                                {
                                    int pref_indix = Word.IndexOf(item.pref) + item.pref.Length;
                                    OOV_Word = Word.Substring(pref_indix, Word.Length - pref_indix);
                                }
                                else
                                {
                                    OOV_Word = Word;
                                }

                                #region Placeholder
                                string Arabic_Word = OOV_Word;
                                Arabic_Word = Arabic_Word.Replace("ا", "A");
                                Arabic_Word = Arabic_Word.Replace("آ", "|");
                                Arabic_Word = Arabic_Word.Replace("أ", ">");
                                Arabic_Word = Arabic_Word.Replace("إ", "<");
                                Arabic_Word = Arabic_Word.Replace("ؤ", "&");
                                Arabic_Word = Arabic_Word.Replace("ئ", "}");
                                Arabic_Word = Arabic_Word.Replace("ء", "'");
                                Arabic_Word = Arabic_Word.Replace("ي", "y");
                                Arabic_Word = Arabic_Word.Replace("ى", "Y");
                                Arabic_Word = Arabic_Word.Replace("و", "w");
                                Arabic_Word = Arabic_Word.Replace("ة", "p");
                                Arabic_Word = Arabic_Word.Replace("ب", "b");
                                Arabic_Word = Arabic_Word.Replace("ت", "t");
                                Arabic_Word = Arabic_Word.Replace("ث", "v");
                                Arabic_Word = Arabic_Word.Replace("ج", "j");
                                Arabic_Word = Arabic_Word.Replace("ح", "H");
                                Arabic_Word = Arabic_Word.Replace("خ", "x");
                                Arabic_Word = Arabic_Word.Replace("د", "d");
                                Arabic_Word = Arabic_Word.Replace("ذ", "*");
                                Arabic_Word = Arabic_Word.Replace("ر", "r");
                                Arabic_Word = Arabic_Word.Replace("ز", "z");
                                Arabic_Word = Arabic_Word.Replace("س", "s");
                                Arabic_Word = Arabic_Word.Replace("ش", "$");
                                Arabic_Word = Arabic_Word.Replace("ص", "S");
                                Arabic_Word = Arabic_Word.Replace("ض", "D");
                                Arabic_Word = Arabic_Word.Replace("ط", "T");
                                Arabic_Word = Arabic_Word.Replace("ظ", "Z");
                                Arabic_Word = Arabic_Word.Replace("ع", "E");
                                Arabic_Word = Arabic_Word.Replace("غ", "g");
                                Arabic_Word = Arabic_Word.Replace("ف", "f");
                                Arabic_Word = Arabic_Word.Replace("ق", "q");
                                Arabic_Word = Arabic_Word.Replace("ك", "k");
                                Arabic_Word = Arabic_Word.Replace("ل", "l");
                                Arabic_Word = Arabic_Word.Replace("م", "m");
                                Arabic_Word = Arabic_Word.Replace("ن", "n");
                                Arabic_Word = Arabic_Word.Replace("ه", "h");

                                OOV_Word = OOV_Word.Replace("ب", "-");
                                OOV_Word = OOV_Word.Replace("ت", "-");
                                OOV_Word = OOV_Word.Replace("ث", "-");
                                OOV_Word = OOV_Word.Replace("ج", "-");
                                OOV_Word = OOV_Word.Replace("ح", "-");
                                OOV_Word = OOV_Word.Replace("خ", "-");
                                OOV_Word = OOV_Word.Replace("د", "-");
                                OOV_Word = OOV_Word.Replace("ذ", "-");
                                OOV_Word = OOV_Word.Replace("ر", "-");
                                OOV_Word = OOV_Word.Replace("ز", "-");
                                OOV_Word = OOV_Word.Replace("س", "-");
                                OOV_Word = OOV_Word.Replace("ش", "-");
                                OOV_Word = OOV_Word.Replace("ص", "-");
                                OOV_Word = OOV_Word.Replace("ض", "-");
                                OOV_Word = OOV_Word.Replace("ط", "-");
                                OOV_Word = OOV_Word.Replace("ظ", "-");
                                OOV_Word = OOV_Word.Replace("ع", "-");
                                OOV_Word = OOV_Word.Replace("غ", "-");
                                OOV_Word = OOV_Word.Replace("ف", "-");
                                OOV_Word = OOV_Word.Replace("ق", "-");
                                OOV_Word = OOV_Word.Replace("ك", "-");
                                OOV_Word = OOV_Word.Replace("ل", "-");
                                OOV_Word = OOV_Word.Replace("م", "-");
                                OOV_Word = OOV_Word.Replace("ن", "-");
                                OOV_Word = OOV_Word.Replace("ه", "-");
                                OOV_Word = OOV_Word.Replace("ا", "A");
                                OOV_Word = OOV_Word.Replace("آ", "|");
                                OOV_Word = OOV_Word.Replace("أ", ">");
                                OOV_Word = OOV_Word.Replace("إ", "<");
                                OOV_Word = OOV_Word.Replace("ؤ", "&");
                                OOV_Word = OOV_Word.Replace("ئ", "}");
                                OOV_Word = OOV_Word.Replace("ء", "'");
                                OOV_Word = OOV_Word.Replace("ي", "y");
                                OOV_Word = OOV_Word.Replace("ى", "Y");
                                OOV_Word = OOV_Word.Replace("و", "w");
                                OOV_Word = OOV_Word.Replace("ة", "p");
                                #endregion

                                try
                                {
                                    var OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => s.OOV_Patterns.Equals(OOV_Word) && s.Tags.Equals(item.tags)).OrderByDescending(a => a.Count).ToList();
                                    if (!OOV_Pattern.Count.Equals(0))
                                    {
                                        if (item.tags.Contains("PV") && OOV_Word.EndsWith("A") && item.suffixes.Equals("a/V_SUF"))
                                        {
                                        }
                                        else if (item.tags.Contains("NOUN") && OOV_Word.EndsWith("A") && (item.suffixes.Equals("i/N_SUF") || item.suffixes.Equals("a/N_SUF") || item.suffixes.Equals("~a/POSS_SUF")))
                                        {
                                        }
                                        else
                                        {
                                            Text_Solutions Text_Solutions_List_Values = new Text_Solutions();
                                            Text_Solutions_List_Values.word = Word;
                                            Text_Solutions_List_Values.lemmaID = "";
                                            Text_Solutions_List_Values.pr = item.prefixes;
                                            Text_Solutions_List_Values.stem = OOV_Word + "/" + item.tags;
                                            Text_Solutions_List_Values.suf = item.suffixes;
                                            Text_Solutions_List_Values.spattern = Arabic_Word;
                                            Text_Solutions_List_Values.affectedBy = "OOV";
                                            Text_Solutions_List.Add(Text_Solutions_List_Values);
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => OOV_Word.Equals(s.OOV_Patterns)).OrderByDescending(a => a.Count).ToList();
                                            if (!OOV_Pattern.Count.Equals(0))
                                            {
                                                if (item.tags.Contains("PV") && OOV_Word.EndsWith("A") && item.suffixes.Equals("a/V_SUF"))
                                                {
                                                }
                                                else if (item.tags.Contains("NOUN") && OOV_Word.EndsWith("A") && (item.suffixes.Equals("i/N_SUF") || item.suffixes.Equals("a/N_SUF") || item.suffixes.Equals("~a/POSS_SUF")))
                                                {
                                                }
                                                else
                                                {
                                                    Text_Solutions Text_Solutions_List_Values = new Text_Solutions();
                                                    Text_Solutions_List_Values.word = Word;
                                                    Text_Solutions_List_Values.lemmaID = "";
                                                    Text_Solutions_List_Values.pr = item.prefixes;
                                                    Text_Solutions_List_Values.stem = OOV_Word + "/" + item.tags;
                                                    Text_Solutions_List_Values.suf = item.suffixes;
                                                    Text_Solutions_List_Values.spattern = Arabic_Word;
                                                    Text_Solutions_List_Values.affectedBy = "OOV";
                                                    Text_Solutions_List.Add(Text_Solutions_List_Values);
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                                catch { }
                            }
                        }
                        #endregion
                        
                    }
                }
            }
            else
            {
                string path = "Diac_Bucwalter\\Diac_Output.xml";
                StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                string Diac_Output_Value = "<AraMorph_output>\n</AraMorph_output>";
                sw.WriteLine(Diac_Output_Value);
                sw.Flush();
                sw.Close();
            }
                    #endregion
                        #endregion

            Text_Solutions = Text_Solutions_List;
            Distinct_Word_For_BAMA_List.Clear();
            return;
        }

        private bool writeToFile(string Text_Value)
        {
            bool flag = true;
            string path = @buckWalter + "Diac_input.txt";
            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
            sw.WriteLine(Text_Value);
            sw.Flush();
            sw.Close();
            return flag;
        }
        private void runBuckwalter()
        {
            #region Running_Buckwlater
            ProcessStartInfo BAMA_PATH = new ProcessStartInfo(@buckWalter + "Diacritize.bat");
            BAMA_PATH.WindowStyle = ProcessWindowStyle.Hidden;
            Process Go = Process.Start(BAMA_PATH);
            Go.WaitForExit();
            Thread.Sleep(2000);
            close_all_cmd();
            #endregion
        }

        private static void close_all_cmd()
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains("perl"))
                {
                    try
                    {
                        clsProcess.Kill();
                    }
                    catch { }
                }
                //----------------------------------------------------------
                if (clsProcess.ProcessName == "cmd")
                {
                    try
                    {
                        clsProcess.Kill();
                    }
                    catch { }
                }
            }
        }
        public bool CheckIfFileIsBeingUsed(string fileName)
        {
            FileInfo f = new FileInfo(fileName);
            FileStream stream = null;
            try
            {
                stream = f.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace Arabic_Automatic_Diacritization
{
    class Getting_Arabic_Diacritics_Stage
    {
        public Getting_Arabic_Diacritics_Stage()
        {
        }
        #region Variabls and Constructors
        string voc; string pr; string stem; string suf; string word; string Diacritized_Text; string ecase;
        string Diacritized_Word; string Diacritized_Value;
        int Diac_Word_Nums; int Arabic_Words_Num; int Words_Need_Case_Ending; int Have_Case_Ending;
        #endregion

        public void Get_Internal_Diacritics(out string Internally_Diacritized_Text, out string Text_Arabic_Words, out string Diac_Word_Num, List<Analyzed_Text> Analyzed_Text, Form1 obj)
        {
            Diacritized_Value = ""; Diacritized_Text = ""; Internally_Diacritized_Text = ""; Diac_Word_Nums = 0; Arabic_Words_Num = 0;
            var Row_Solutions = Analyzed_Text.Select(c => new { c.ID, c.word, c.lemmaID, c.pr, c.stem, c.suf, c.spattern, c.def, c.ecase, c.affectedBy }).ToList();

            for (int r = 0; r < Row_Solutions.Count(); r++)
            {
                if (Regex.IsMatch(Row_Solutions[r].word, @"[ء-ي]"))
                {
                    if (Row_Solutions[r].word.Length > 1)
                    {
                        Arabic_Words_Num = Arabic_Words_Num + 1;
                    }
                    pr = Row_Solutions[r].pr; stem = Row_Solutions[r].stem; suf = Row_Solutions[r].suf; voc = ""; Diacritized_Word = ""; word = Row_Solutions[r].word;

                    #region OOV_Words
                    if (Row_Solutions[r].stem.Contains("-"))
                    {
                        int start = Row_Solutions[r].stem.IndexOf("/") + 1;
                        int end = Row_Solutions[r].stem.Length - start;
                        string OOV_Pat = Row_Solutions[r].stem.Substring(0, Row_Solutions[r].stem.IndexOf("/"));
                        string OOV_Tag = Row_Solutions[r].stem.Substring(start, end);
                        try
                        {
                            var OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => s.OOV_Patterns.Equals(OOV_Pat) && s.Tags.Equals(OOV_Tag)).OrderByDescending(a => a.Count).FirstOrDefault();
                            if (!OOV_Pattern.Count.Equals(0))
                            {
                                string charValue = "";
                                string Diac_Pattern = OOV_Pattern.Diac_Patterns;
                                for (int charIndex = 0; charIndex < Row_Solutions[r].spattern.Length; charIndex++)
                                {
                                    charValue = Row_Solutions[r].spattern.Substring(charIndex, 1);
                                    if (!charValue.Equals("A") && !charValue.Equals("|") && !charValue.Equals(">") && !charValue.Equals("<") && !charValue.Equals("&") && !charValue.Equals("}") &&
                                        !charValue.Equals("'") && !charValue.Equals("y") && !charValue.Equals("Y") && !charValue.Equals("w") && !charValue.Equals("p"))
                                    {
                                        if (OOV_Pattern.Diac_Patterns.Contains("-"))
                                        {
                                            int Diac_charIndex = OOV_Pattern.Diac_Patterns.IndexOf("-");
                                            OOV_Pattern.Diac_Patterns = OOV_Pattern.Diac_Patterns.Remove(Diac_charIndex, 1).Insert(Diac_charIndex, charValue);
                                        }
                                    }
                                }
                                stem = OOV_Pattern.Diac_Patterns + "/" + OOV_Tag;
                                OOV_Pattern.Diac_Patterns = Diac_Pattern;
                            }
                        }
                        catch
                        {
                            try
                            {
                                var OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => s.OOV_Patterns.Equals(OOV_Pat)).OrderByDescending(a => a.Count).FirstOrDefault();
                                if (!OOV_Pattern.Count.Equals(0))
                                {
                                    string charValue = "";
                                    string Diac_Pattern = OOV_Pattern.Diac_Patterns;
                                    for (int charIndex = 0; charIndex < Row_Solutions[r].spattern.Length; charIndex++)
                                    {
                                        charValue = Row_Solutions[r].spattern.Substring(charIndex, 1);
                                        if (!charValue.Equals("A") && !charValue.Equals("|") && !charValue.Equals(">") && !charValue.Equals("<") && !charValue.Equals("&") && !charValue.Equals("}") &&
                                            !charValue.Equals("'") && !charValue.Equals("y") && !charValue.Equals("Y") && !charValue.Equals("w") && !charValue.Equals("p"))
                                        {
                                            if (OOV_Pattern.Diac_Patterns.Contains("-"))
                                            {
                                                int Diac_charIndex = OOV_Pattern.Diac_Patterns.IndexOf("-");
                                                OOV_Pattern.Diac_Patterns = OOV_Pattern.Diac_Patterns.Remove(Diac_charIndex, 1).Insert(Diac_charIndex, charValue);
                                            }
                                        }
                                    }
                                    stem = OOV_Pattern.Diac_Patterns + "/" + OOV_Tag;
                                    OOV_Pattern.Diac_Patterns = Diac_Pattern;
                                }
                            }
                            catch
                            {
                                Diacritized_Word = word;
                            }
                        }
                    }
                    #endregion

                    #region Some_Voc_Needed_Rules
                    if (Row_Solutions[r].word.Length > 1 && !Row_Solutions[r].stem.Equals("e"))
                    {
                        Diac_Word_Nums = Diac_Word_Nums + 1;
                    }
                    if (pr.Contains("Al/DET"))
                    {
                        if (pr.Contains("li/PREP") && (stem.StartsWith("t") || stem.StartsWith("v") || stem.StartsWith("d") || stem.StartsWith("r") || stem.StartsWith("z") || stem.StartsWith("s") || stem.StartsWith("$") || stem.StartsWith("S") || stem.StartsWith("D") || stem.StartsWith("T") || stem.StartsWith("Z") || stem.StartsWith("n")))
                        {
                            pr = pr.Replace("Al", "l");
                            stem = stem.Substring(0, 1) + "~" + stem.Substring(1, stem.Length - 1);
                        }
                        if (pr.Contains("li/PREP") && stem.StartsWith("l"))
                        {
                            pr = pr.Replace("Al", "");
                            stem = stem.Replace("l", "l~");
                        }
                        else
                        {
                            if (pr.Contains("li/PREP"))
                            {
                                pr = pr.Replace("Al", "lo");
                            }
                            else
                            {
                                if (stem.StartsWith("t") || stem.StartsWith("v") || stem.StartsWith("d") || stem.StartsWith("r") || stem.StartsWith("z") || stem.StartsWith("s") || stem.StartsWith("$") || stem.StartsWith("S") || stem.StartsWith("D") || stem.StartsWith("T") || stem.StartsWith("Z") || stem.StartsWith("n") || stem.StartsWith("l"))
                                {
                                    stem = stem.Substring(0, 1) + "~" + stem.Substring(1, stem.Length - 1);
                                }
                                else
                                {
                                    pr = pr.Replace("Al", "Alo");
                                    if (pr.Contains("li/PREP"))
                                    {
                                        pr = pr.Replace("A", "");
                                    }
                                }
                            }
                        }
                    }

                    if (stem.StartsWith("Al") && stem.EndsWith("/NOUN_PROP"))
                    {
                        if (pr.Contains("li/PREP"))
                        {
                            if (stem.StartsWith("Alt") || stem.StartsWith("Alv") || stem.StartsWith("Ald") || stem.StartsWith("Alr") || stem.StartsWith("Alz") || stem.StartsWith("Als") || stem.StartsWith("Al$") || stem.StartsWith("AlS") || stem.StartsWith("AlD") || stem.StartsWith("AlT") || stem.StartsWith("AlZ") || stem.StartsWith("Aln"))
                            {
                                stem = stem.Substring(1, stem.Length - 1);
                            }
                            else if (pr.Contains("li/PREP") && stem.StartsWith("l"))
                            {
                                stem = stem.Substring(2, stem.Length - 1);
                            }
                            else if (stem.StartsWith("Alb") || stem.StartsWith("Alj") || stem.StartsWith("AlH") || stem.StartsWith("Alx") || stem.StartsWith("AlE") || stem.StartsWith("Alg") || stem.StartsWith("Alf") || stem.StartsWith("Alq") || stem.StartsWith("Alk") || stem.StartsWith("Alm") || stem.StartsWith("Alh") || stem.StartsWith("Alw") || stem.StartsWith("Aly"))
                            {
                                stem = stem.Substring(1, 1) + "o" + stem.Substring(2, stem.Length - 2);
                            }

                        }
                        else
                        {
                            if (stem.StartsWith("Alb") || stem.StartsWith("Alj") || stem.StartsWith("AlH") || stem.StartsWith("Alx") || stem.StartsWith("AlE") || stem.StartsWith("Alg") || stem.StartsWith("Alf") || stem.StartsWith("Alq") || stem.StartsWith("Alk") || stem.StartsWith("Alm") || stem.StartsWith("Alh") || stem.StartsWith("Alw") || stem.StartsWith("Aly") || stem.StartsWith("Al>") || stem.StartsWith("Al<") || stem.StartsWith("Al|"))
                            {

                                stem = "Alo" + stem.Substring(2, stem.Length - 2);
                            }
                        }
                    }
                    if ((stem.StartsWith("Alt") || stem.StartsWith("Alv") || stem.StartsWith("Ald") || stem.StartsWith("Alr") || stem.StartsWith("Alz") || stem.StartsWith("Als") || stem.StartsWith("Al$") || stem.StartsWith("AlS") || stem.StartsWith("AlD") || stem.StartsWith("AlT") || stem.StartsWith("AlZ") || stem.StartsWith("All") || stem.StartsWith("Aln")) && !stem.Substring(3, 1).Equals("~") && !pr.Contains("Al/DET"))
                    {
                        stem = stem.Substring(0, 3) + "~" + stem.Substring(3, stem.Length - 3);
                    }
                    if (pr.Contains("/CONJ+li/JUS"))
                    {
                        pr = pr.Replace("li/JUS", "lo/JUS");
                    }
                    if (pr.Contains("mA/PRON") || pr.Contains("mA/INTERJ") || pr.Contains("mA/PART"))
                    {
                        pr = pr.Replace("mA", "maA ");
                    }
                    if (suf.Contains("(null)/V_SUF"))
                    {
                        suf = suf.Replace("(null)/V_SUF", "");
                    }
                    if (suf.Contains("AF/NOUN"))
                    {
                        suf = suf.Replace("AF/NOUN", "");
                    }
                    if (word.EndsWith("ا") && (stem.Contains("NOUN") || stem.Contains("ADJ")) && !(stem.Contains("A/")) && suf == (""))
                    {
                        suf = suf + "A";
                    }
                    if (stem.Contains("PV") && suf.StartsWith("aw/"))
                    {
                        suf = suf.Replace("aw", "awo");
                    }
                    if (stem.Contains("PV") && !stem.Contains("A/") && !stem.Contains("y/") && !suf.StartsWith("a") && !suf.StartsWith("A") && !suf.StartsWith("at") && !suf.StartsWith("awoA") && !suf.StartsWith("uw") && !suf.Equals(""))
                    {
                        if (stem.Contains("n/") && (suf.StartsWith("na/") || suf.StartsWith("nA/")))
                        {
                            suf = suf.Replace("n", "~");
                        }
                        else
                        {
                            suf = "o" + suf;
                        }
                    }
                    if (stem.Contains("PV") && suf.StartsWith("at/"))
                    {
                        suf = suf.Replace("at", "ato");
                    }
                    if (stem.Contains("IV") && suf.StartsWith("na/"))
                    {
                        if (stem.Contains("n/"))
                        {
                            suf = suf.Replace("n", "~");
                        }
                        else
                        {
                            suf = "o" + suf;
                        }
                    }
                    if (stem.Contains(">an/CONJ") || stem.Contains("<in/CONJ") || stem.Contains("l`kin/CONJ") || stem.Contains("lan/PART") || stem.Contains("man/PRON") || (stem.Contains("Ean/PREP") && !suf.StartsWith("~")) || (stem.Contains("min/PREP") && !suf.StartsWith("~")))
                    {
                        stem = stem.Replace("n", "no");
                    }
                    if (stem.Contains("lam/PART") || stem.Contains(">am/CONJ") || stem.Contains("hum/PRON"))
                    {
                        stem = stem.Replace("m", "mo");
                    }
                    if (stem.Contains("kay/PART") || (stem.Contains("laday/NOUN") && !suf.Contains("~a/")))
                    {
                        stem = stem.Replace("y", "yo");
                    }
                    if (stem.Contains("<i*/NOUN"))
                    {
                        stem = stem.Replace("*", "*o");
                    }
                    if (stem.Contains("qad/PART"))
                    {
                        stem = stem.Replace("d", "do");
                    }
                    if (stem.Equals(">aw/CONJ") || stem.Equals("law/CONJ"))
                    {
                        stem = stem.Replace("w", "wo");
                    }
                    if (word.EndsWith("ا") && !stem.Contains("A/") && suf.Equals(""))
                    {
                        suf = "A";
                    }
                    if (stem.Contains("A/") && suf.StartsWith("At/"))
                    {
                        suf = suf.Replace("A", "");
                    }
                    if (stem.Contains(">/") && suf.StartsWith("At/"))
                    {
                        stem = stem.Replace(">/", "|/");
                        suf = suf.Replace("At/", "t/");
                    }
                    if (stem.EndsWith("kay/CONJ"))
                    {
                        stem = stem.Replace("kay/", "kayo/");
                    }
                    if ((stem.Equals("Ealay/PREP") || stem.Equals("<ilay/PREP")) && !suf.Equals("~a/POSS_SUF"))
                    {
                        stem = stem.Replace("y/", "yo/");
                    }
                    if (suf.Contains("m/"))
                    {
                        suf = suf.Replace("m/", "mo/");
                    }
                    if (stem.Equals("bal/CONJ") || stem.Equals("hal/PART") || stem.Equals("Al|n/NOUN"))
                    {
                        stem = stem.Replace("l", "lo");
                    }
                    if (stem.Equals("<i*an/CONJ"))
                    {
                        stem = stem.Replace("n/", "no/");
                    }
                    if (suf.Equals("ayo/N_SUF+ya/POSS_SUF"))
                    {
                        suf = "ay~a";
                    }
                    if (stem.Equals(">ay/PART"))
                    {
                        stem = stem.Replace(">ay/PART", ">ayo/PART");
                    }
                    #endregion

                    if (pr.Contains("/"))
                    {
                        string[] pr_tags = { "/CV_PRF", "/IV_PRF", "/CONJ", "/DET", "/FUT", "/JUS", "/PART", "/PREP", "/SUB", "/PRON", "/INTERJ" };
                        foreach (string Value in pr_tags)
                        {
                            if (pr.Contains(Value))
                            {
                                pr = Regex.Replace(pr, Value, string.Empty);
                            }
                        }

                    }


                    if (stem.Contains("/"))
                    {
                        string[] stem_tags = { "/ABBREV", "/ADJ", "/ADV", "/CONJ", "/CV", "/IV_PASS", "/IV", "/NOUN_PROP", "/NOUN", "/PART", "/PREP", "/PRON", "/PV_PASS", "/PV", "/FUNC_WORD" };
                        foreach (string Value in stem_tags)
                        {
                            if (stem.Contains(Value))
                            {
                                stem = Regex.Replace(stem, Value, string.Empty);
                            }
                        }
                    }

                    #region Some_Other_Voc_Rules_For_Avoiding_Replace_Of_stem_tag
                    if (stem.Contains("A") && !stem.StartsWith("Al"))
                    {
                        stem = stem.Replace("A", "aA");
                    }
                    if (stem.StartsWith("Al"))
                    {
                        if (stem.Substring(2, stem.Length - 2).Contains("A"))
                        {
                            stem = "Al" + stem.Substring(2, stem.Length - 2).Replace("A", "aA");
                        }
                    }
                    #endregion

                    if (suf.Contains("/"))
                    {
                        if (suf.StartsWith("At/") || suf.StartsWith("hAt/") || suf.StartsWith("A/") || suf.StartsWith("Ani/") || suf.StartsWith("atA/") || suf.StartsWith("atAni/") || suf.StartsWith("hA/") || suf.StartsWith("himA/") || suf.StartsWith("humA/") || suf.StartsWith("kumA/") || suf.StartsWith("onA/") || suf.StartsWith("~A/") || suf.StartsWith("tumA/") || suf.StartsWith("iyA/"))
                        {
                            suf = suf.Replace("A", "aA");
                        }
                        if (suf.Contains("+Ahu/") || suf.Contains("+hA/") || suf.Contains("+himA/") || suf.Contains("+humA/") || suf.Contains("+kumA/") || suf.Contains("+nA/"))
                        {
                            suf = suf.Replace("A", "aA");
                        }
                        string[] suf_tags = { "/POSS_SUF:2MS", "/POSS_SUF:2FS", "/POSS_SUF", "/N_SUF", "e/V_SUF", "/V_SUF:2FS", "/V_SUF:1S", "/V_SUF:3FS", "/V_SUF:2MS", "/V_SUF" };
                        foreach (string Value in suf_tags)
                        {
                            if (suf.Contains(Value))
                            {
                                suf = Regex.Replace(suf, Value, string.Empty);
                            }
                        }
                    }
                    pr = pr.Replace("+", ""); suf = suf.Replace("+", ""); stem = stem.Replace("+", "");
                    voc = pr + stem + suf;
                    if (voc != "")
                    {
                        Diacritized_Word = voc;
                        Get_Diacritics(voc, out Diacritized_Word);
                    }
                    else if (voc == "")
                    {
                        Diacritized_Word = Row_Solutions[r].word;
                    }
                }
                else
                {
                    Diacritized_Word = Row_Solutions[r].word;
                    Diacritized_Word = Diacritized_Word.Replace("\nS/\n/S\n", "\n");
                    Diacritized_Word = Diacritized_Word.Replace("\rS/\r/S\r", "\n\n");
                    Diacritized_Word = Diacritized_Word.Replace("\rS/\r\r/S\r", "\n\n");
                    Diacritized_Word = Diacritized_Word.Replace("\r\r", "\n\n");
                    Diacritized_Word = Diacritized_Word.Replace("/D\n/S\n", "");
                    Diacritized_Word = Diacritized_Word.Replace("\nS/\nD/", "");
                }
                Diacritized_Value = Diacritized_Value + " " + Diacritized_Word;
                Diacritized_Value = Diacritized_Value.Replace("\n ", "\n");
                Diacritized_Value = Diacritized_Value.Replace(" \n", "\n");
            }
            Diacritized_Text = Diacritized_Value;
            Internally_Diacritized_Text = Diacritized_Text.Substring(2, Diacritized_Text.Length - 2);
            Diac_Word_Num = Diac_Word_Nums.ToString();
            Text_Arabic_Words = Arabic_Words_Num.ToString();
            return;
        }

        public void Get_Case_Ending_Diacritics(out string Case_Ending_Diacritized_Text, out string Need_Case_Ending_Words, out string Case_Ending_Num, List<Analyzed_Text> Analyzed_Text, Form1 obj)
        {

            Diacritized_Value = ""; Diacritized_Text = ""; Words_Need_Case_Ending = 0; Have_Case_Ending = 0;
            var Row_Solutions = Analyzed_Text.Select(c => new { c.ID, c.word, c.lemmaID, c.pr, c.stem, c.suf, c.spattern, c.def, c.ecase, c.affectedBy }).ToList();
            for (int r = 0; r < Row_Solutions.Count(); r++)
            {
                if (Regex.IsMatch(Row_Solutions[r].word, @"[ء-ي]"))
                {
                    pr = Row_Solutions[r].pr; stem = Row_Solutions[r].stem; ; suf = Row_Solutions[r].suf; ecase = Row_Solutions[r].ecase; voc = ""; Diacritized_Word = ""; word = Row_Solutions[r].word;
                    #region OOV_Words
                    if (Row_Solutions[r].stem.Contains("-"))
                    {
                        int start = Row_Solutions[r].stem.IndexOf("/") + 1;
                        int end = Row_Solutions[r].stem.Length - start;
                        string OOV_Pat = Row_Solutions[r].stem.Substring(0, Row_Solutions[r].stem.IndexOf("/"));
                        string OOV_Tag = Row_Solutions[r].stem.Substring(start, end);
                        try
                        {
                            var OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => s.OOV_Patterns.Equals(OOV_Pat) && s.Tags.Equals(OOV_Tag)).OrderByDescending(a => a.Count).FirstOrDefault();
                            if (!OOV_Pattern.Count.Equals(0))
                            {
                                string charValue = "";
                                string Diac_Pattern = OOV_Pattern.Diac_Patterns;
                                for (int charIndex = 0; charIndex < Row_Solutions[r].spattern.Length; charIndex++)
                                {
                                    charValue = Row_Solutions[r].spattern.Substring(charIndex, 1);
                                    if (!charValue.Equals("A") && !charValue.Equals("|") && !charValue.Equals(">") && !charValue.Equals("<") && !charValue.Equals("&") && !charValue.Equals("}") &&
                                        !charValue.Equals("'") && !charValue.Equals("y") && !charValue.Equals("Y") && !charValue.Equals("w") && !charValue.Equals("p"))
                                    {
                                        if (OOV_Pattern.Diac_Patterns.Contains("-"))
                                        {
                                            int Diac_charIndex = OOV_Pattern.Diac_Patterns.IndexOf("-");
                                            OOV_Pattern.Diac_Patterns = OOV_Pattern.Diac_Patterns.Remove(Diac_charIndex, 1).Insert(Diac_charIndex, charValue);
                                        }
                                    }
                                }
                                stem = OOV_Pattern.Diac_Patterns + "/" + OOV_Tag;
                                OOV_Pattern.Diac_Patterns = Diac_Pattern;
                            }
                        }
                        catch
                        {
                            try
                            {
                                var OOV_Pattern = obj.OOV_Pattern.OOV_Pattern.Where(s => s.OOV_Patterns.Equals(OOV_Pat)).OrderByDescending(a => a.Count).FirstOrDefault();
                                if (!OOV_Pattern.Count.Equals(0))
                                {
                                    string charValue = "";
                                    string Diac_Pattern = OOV_Pattern.Diac_Patterns;
                                    for (int charIndex = 0; charIndex < Row_Solutions[r].spattern.Length; charIndex++)
                                    {
                                        charValue = Row_Solutions[r].spattern.Substring(charIndex, 1);
                                        if (!charValue.Equals("A") && !charValue.Equals("|") && !charValue.Equals(">") && !charValue.Equals("<") && !charValue.Equals("&") && !charValue.Equals("}") &&
                                            !charValue.Equals("'") && !charValue.Equals("y") && !charValue.Equals("Y") && !charValue.Equals("w") && !charValue.Equals("p"))
                                        {
                                            if (OOV_Pattern.Diac_Patterns.Contains("-"))
                                            {
                                                int Diac_charIndex = OOV_Pattern.Diac_Patterns.IndexOf("-");
                                                OOV_Pattern.Diac_Patterns = OOV_Pattern.Diac_Patterns.Remove(Diac_charIndex, 1).Insert(Diac_charIndex, charValue);
                                            }
                                        }
                                    }
                                    stem = OOV_Pattern.Diac_Patterns + "/" + OOV_Tag;
                                    OOV_Pattern.Diac_Patterns = Diac_Pattern;
                                }
                            }
                            catch
                            {
                                Diacritized_Word = word;
                            }
                        }
                    }
                    #endregion

                    if (Row_Solutions[r].stem.EndsWith("/NOUN") || Row_Solutions[r].stem.EndsWith("/ADJ") || Row_Solutions[r].stem.EndsWith("/NOUN_PROP") || Row_Solutions[r].stem.EndsWith("/ADV") || Row_Solutions[r].stem.Contains("/IV") || Row_Solutions[r].stem.Equals("gayor/PART"))
                    {
                        Words_Need_Case_Ending += 1;
                        if (!Row_Solutions[r].ecase.Equals("") || Row_Solutions[r].stem.EndsWith("/NOUN_PROP"))
                        {
                            Have_Case_Ending += 1;
                            if ((Row_Solutions[r].suf.Equals("At/N_SUF") || Row_Solutions[r].suf.Contains("at/N_SUF")) && Row_Solutions[r].suf.Contains("POSS"))
                            {
                                suf = Row_Solutions[r].suf.Replace("At", "At" + Row_Solutions[r].ecase);
                                suf = Row_Solutions[r].suf.Replace("at", "at" + Row_Solutions[r].ecase);
                            }
                            else
                            {
                                if (Row_Solutions[r].suf.Contains("POSS") && !Row_Solutions[r].suf.Contains("At/N_SUF") && !Row_Solutions[r].suf.Contains("at/NSUF"))
                                {
                                    suf = Row_Solutions[r].ecase + Row_Solutions[r].suf;
                                }
                                else if (Row_Solutions[r].suf.Contains("POSS") && Row_Solutions[r].suf.Contains("At/N_SUF"))
                                {
                                    suf = suf.Replace("At/N_SUF", "At/N_SUF" + Row_Solutions[r].ecase);
                                }
                                else
                                {
                                    if (Row_Solutions[r].suf.Contains("e/V_SUF"))
                                    {
                                        suf = Row_Solutions[r].suf.Replace("e/V_SUF", Row_Solutions[r].ecase);
                                    }
                                }
                            }
                        }
                    }

                    #region Some_Voc_Needed_Rules
                    Diac_Word_Nums = Diac_Word_Nums + 1;
                    if (word == "مازالت")
                    {

                    }
                    if (pr.Contains("Al/DET"))
                    {
                        if (pr.Contains("li/PREP") && (stem.StartsWith("t") || stem.StartsWith("v") || stem.StartsWith("d") || stem.StartsWith("r") || stem.StartsWith("z") || stem.StartsWith("s") || stem.StartsWith("$") || stem.StartsWith("S") || stem.StartsWith("D") || stem.StartsWith("T") || stem.StartsWith("Z") || stem.StartsWith("n")))
                        {
                            pr = pr.Replace("Al", "l");
                            stem = stem.Substring(0, 1) + "~" + stem.Substring(1, stem.Length - 1);
                        }
                        if (pr.Contains("li/PREP") && stem.StartsWith("l"))
                        {
                            pr = pr.Replace("Al", "");
                            stem = stem.Replace("l", "l~");
                        }
                        else
                        {
                            if (pr.Contains("li/PREP"))
                            {
                                pr = pr.Replace("Al", "lo");
                            }
                            else
                            {
                                if (stem.StartsWith("t") || stem.StartsWith("v") || stem.StartsWith("d") || stem.StartsWith("r") || stem.StartsWith("z") || stem.StartsWith("s") || stem.StartsWith("$") || stem.StartsWith("S") || stem.StartsWith("D") || stem.StartsWith("T") || stem.StartsWith("Z") || stem.StartsWith("n") || stem.StartsWith("l"))
                                {
                                    stem = stem.Substring(0, 1) + "~" + stem.Substring(1, stem.Length - 1);
                                }
                                else
                                {
                                    pr = pr.Replace("Al", "Alo");
                                    if (pr.Contains("li/PREP"))
                                    {
                                        pr = pr.Replace("A", "");
                                    }
                                }
                            }
                        }
                    }

                    if (stem.StartsWith("Al") && stem.EndsWith("/NOUN_PROP"))
                    {
                        if (pr.Contains("li/PREP"))
                        {
                            if (stem.StartsWith("Alt") || stem.StartsWith("Alv") || stem.StartsWith("Ald") || stem.StartsWith("Alr") || stem.StartsWith("Alz") || stem.StartsWith("Als") || stem.StartsWith("Al$") || stem.StartsWith("AlS") || stem.StartsWith("AlD") || stem.StartsWith("AlT") || stem.StartsWith("AlZ") || stem.StartsWith("Aln"))
                            {
                                stem = stem.Substring(1, stem.Length - 1);
                            }
                            else if (pr.Contains("li/PREP") && stem.StartsWith("l"))
                            {
                                stem = stem.Substring(2, stem.Length - 1);
                            }
                            else if (stem.StartsWith("Alb") || stem.StartsWith("Alj") || stem.StartsWith("AlH") || stem.StartsWith("Alx") || stem.StartsWith("AlE") || stem.StartsWith("Alg") || stem.StartsWith("Alf") || stem.StartsWith("Alq") || stem.StartsWith("Alk") || stem.StartsWith("Alm") || stem.StartsWith("Alh") || stem.StartsWith("Alw") || stem.StartsWith("Aly"))
                            {
                                stem = stem.Substring(1, 1) + "o" + stem.Substring(2, stem.Length - 2);
                            }

                        }
                        else
                        {
                            if (stem.StartsWith("Alb") || stem.StartsWith("Alj") || stem.StartsWith("AlH") || stem.StartsWith("Alx") || stem.StartsWith("AlE") || stem.StartsWith("Alg") || stem.StartsWith("Alf") || stem.StartsWith("Alq") || stem.StartsWith("Alk") || stem.StartsWith("Alm") || stem.StartsWith("Alh") || stem.StartsWith("Alw") || stem.StartsWith("Aly") || stem.StartsWith("Al>") || stem.StartsWith("Al<") || stem.StartsWith("Al|"))
                            {

                                stem = "Alo" + stem.Substring(2, stem.Length - 2);
                            }
                        }
                    }

                    if ((stem.StartsWith("Alt") || stem.StartsWith("Alv") || stem.StartsWith("Ald") || stem.StartsWith("Alr") || stem.StartsWith("Alz") || stem.StartsWith("Als") || stem.StartsWith("Al$") || stem.StartsWith("AlS") || stem.StartsWith("AlD") || stem.StartsWith("AlT") || stem.StartsWith("AlZ") || stem.StartsWith("All") || stem.StartsWith("Aln")) && !stem.Substring(3, 1).Equals("~") && !pr.Contains("Al/DET"))
                    {
                        stem = stem.Substring(0, 3) + "~" + stem.Substring(3, stem.Length - 3);
                    }
                    if (pr.Contains("/CONJ+li/JUS"))
                    {
                        pr = pr.Replace("li/JUS", "lo/JUS");
                    }
                    if (pr.Contains("mA/PRON") || pr.Contains("mA/INTERJ") || pr.Contains("mA/PART"))
                    {
                        pr = pr.Replace("mA", "maA ");
                    }
                    if (suf.Contains("(null)/V_SUF"))
                    {
                        suf = suf.Replace("(null)/V_SUF", "");
                    }
                    if (suf.Contains("AF/NOUN"))
                    {
                        suf = suf.Replace("AF/NOUN", "");
                    }
                    if (word.EndsWith("ا") && (stem.Contains("NOUN") || stem.Contains("ADJ")) && !(stem.Contains("A/")) && suf == (""))
                    {
                        suf = suf + "A";
                    }
                    if (stem.Contains("PV") && suf.StartsWith("aw/"))
                    {
                        suf = suf.Replace("aw", "awo");
                    }
                    if (stem.Contains("PV") && !stem.Contains("A/") && !stem.Contains("y/") && !suf.StartsWith("a") && !suf.StartsWith("A") && !suf.StartsWith("at") && !suf.StartsWith("awoA") && !suf.StartsWith("uw") && !suf.Equals(""))
                    {
                        if (stem.Contains("n/") && (suf.StartsWith("na/") || suf.StartsWith("nA/")))
                        {
                            suf = suf.Replace("n", "~");
                        }
                        else
                        {
                            suf = "o" + suf;
                        }
                    }
                    if (stem.Contains("PV") && suf.StartsWith("at/"))
                    {
                        suf = suf.Replace("at", "ato");
                    }
                    if (stem.Contains("IV") && suf.StartsWith("na/"))
                    {
                        if (stem.Contains("n/"))
                        {
                            suf = suf.Replace("n", "~");
                        }
                        else
                        {
                            suf = "o" + suf;
                        }
                    }
                    if (stem.Contains(">an/CONJ") || stem.Contains("<in/CONJ") || stem.Contains("l`kin/CONJ") || stem.Contains("lan/PART") || stem.Contains("man/PRON") || (stem.Contains("Ean/PREP") && !suf.StartsWith("~")) || (stem.Contains("min/PREP") && !suf.StartsWith("~")))
                    {
                        stem = stem.Replace("n", "no");
                    }
                    if (stem.Contains("lam/PART") || stem.Contains(">am/CONJ") || stem.Contains("hum/PRON"))
                    {
                        stem = stem.Replace("m", "mo");
                    }
                    if (stem.Contains("kay/PART") || (stem.Contains("laday/NOUN") && !suf.Contains("~a/")))
                    {
                        stem = stem.Replace("y", "yo");
                    }
                    if (stem.Contains("<i*/NOUN"))
                    {
                        stem = stem.Replace("*", "*o");
                    }
                    if (stem.Contains("qad/PART"))
                    {
                        stem = stem.Replace("d", "do");
                    }
                    if (stem.Equals(">aw/CONJ") || stem.Equals("law/CONJ"))
                    {
                        stem = stem.Replace("w", "wo");
                    }
                    if (word.EndsWith("ا") && !stem.Contains("A/") && suf.Equals(""))
                    {
                        suf = "A";
                    }
                    if (stem.Contains("A/") && suf.StartsWith("At/"))
                    {
                        suf = suf.Replace("A", "");
                    }
                    if (stem.Contains("aY/") && ecase.Equals("F"))
                    {
                        stem = stem.Replace("aY/", "Y/");
                    }
                    if (stem.Contains(">/") && suf.StartsWith("At/"))
                    {
                        stem = stem.Replace(">/", "|/");
                        suf = suf.Replace("At/", "t/");
                    }
                    if (stem.EndsWith("kay/CONJ"))
                    {
                        stem = stem.Replace("kay/", "kayo/");
                    }
                    if ((stem.Equals("Ealay/PREP") || stem.Equals("<ilay/PREP")) && !suf.Equals("~a/POSS_SUF"))
                    {
                        stem = stem.Replace("y/", "yo/");
                    }
                    if (suf.Contains("m/"))
                    {
                        suf = suf.Replace("m/", "mo/");
                    }
                    if (stem.Equals("bal/CONJ") || stem.Equals("hal/PART") || stem.Equals("Al|n/NOUN"))
                    {
                        stem = stem.Replace("l", "lo");
                    }
                    if (stem.Equals("<i*an/CONJ"))
                    {
                        stem = stem.Replace("n/", "no/");
                    }
                    if (suf.Equals("ayo/N_SUF+ya/POSS_SUF"))
                    {
                        suf = "ay~a";
                    }
                    if (stem.Equals(">ay/PART"))
                    {
                        stem = stem.Replace(">ay/PART", ">ayo/PART");
                    }
                    #region in case ending only
                    if ((suf.Contains("hi/") || suf.Contains("him/")) && (ecase.Equals("a") || ecase.Equals("u")))
                    {
                        suf = suf.Replace("hi", "hu");
                    }
                    if ((suf.Contains("hu/") || suf.Contains("hum/")) && (ecase.Equals("i")))
                    {
                        suf = suf.Replace("hu", "hi");
                    }
                    if (stem.EndsWith("/NOUN_PROP"))
                    {
                        if ((stem.Contains("y/NOUN_PROP") && !stem.Contains("oy/NOUN_PROP")) || (stem.Contains("w/NOUN_PROP") && !stem.Contains("ow/NOUN_PROP")))
                        {
                            ecase = "e";
                        }
                        else if (stem.Contains("Y/NOUN_PROP") || stem.Contains("A/NOUN_PROP"))
                        {
                            ecase = "e";
                        }
                        else if (word.Substring(word.Length - 1, 1) + ecase != "اF")
                        {
                            ecase = "o";
                        }
                    }
                    #endregion
                    #endregion

                    if (pr.Contains("/"))
                    {
                        string[] pr_tags = { "/CV_PRF", "/IV_PRF", "/CONJ", "/DET", "/FUT", "/JUS", "/PART", "/PREP", "/SUB", "/PRON" };
                        foreach (string Value in pr_tags)
                        {
                            if (pr.Contains(Value))
                            {
                                pr = Regex.Replace(pr, Value, string.Empty);
                            }
                        }
                    }
                    if (stem.Contains("/"))
                    {
                        string[] stem_tags = { "/ABBREV", "/ADJ", "/ADV", "/CONJ", "/CV", "/IV_PASS", "/IV", "/NOUN_PROP", "/NOUN", "/PART", "/PREP", "/PRON", "/PV_PASS", "/PV", "/FUNC_WORD" };
                        foreach (string Value in stem_tags)
                        {
                            if (stem.Contains(Value))
                            {
                                stem = Regex.Replace(stem, Value, string.Empty);
                            }
                        }
                    }

                    #region Some_Other_Voc_Rules_For_Avoiding_Replace_Of_stem_tag
                    if (stem.Contains("A") && !stem.StartsWith("Al"))
                    {
                        stem = stem.Replace("A", "aA");
                    }
                    if (stem.StartsWith("Al"))
                    {
                        if (stem.Substring(2, stem.Length - 2).Contains("A"))
                        {
                            stem = "Al" + stem.Substring(2, stem.Length - 2).Replace("A", "aA");
                        }
                    }
                    #endregion

                    if (suf.Contains("/"))
                    {
                        if (suf.StartsWith("At/") || suf.StartsWith("hAt/") || suf.StartsWith("A/") || suf.StartsWith("Ani/") || suf.StartsWith("atA/") || suf.StartsWith("atAni/") || suf.StartsWith("hA/") || suf.StartsWith("ihA/") || suf.StartsWith("uhA/") || suf.StartsWith("ahA/") || suf.StartsWith("himA/") || suf.StartsWith("ihimA/") || suf.StartsWith("humA/") || suf.StartsWith("ahumA/") || suf.StartsWith("uhumA/") || suf.StartsWith("kumA/") || suf.StartsWith("ikumA/") || suf.StartsWith("ukumA/") || suf.StartsWith("akumA/") || suf.StartsWith("onA/") || suf.StartsWith("inA/") || suf.StartsWith("unA/") || suf.StartsWith("anA/") || suf.StartsWith("~A/") || suf.StartsWith("tumA/") || suf.StartsWith("iyA/"))
                        {
                            suf = suf.Replace("A", "aA");
                        }
                        if (suf.Contains("+Ahu/") || suf.Contains("+hA/") || suf.Contains("+himA/") || suf.Contains("+humA/") || suf.Contains("+kumA/") || suf.Contains("+nA/"))
                        {
                            suf = suf.Replace("A", "aA");
                        }
                        string[] suf_tags = { "/POSS_SUF:2MS", "/POSS_SUF:2FS", "/POSS_SUF", "/N_SUF", "e/V_SUF", "/V_SUF:2FS", "/V_SUF:1S", "/V_SUF:3FS", "/V_SUF:2MS", "/V_SUF" };
                        foreach (string Value in suf_tags)
                        {
                            if (suf.Contains(Value))
                            {
                                suf = Regex.Replace(suf, Value, string.Empty);
                            }
                        }
                    }
                    pr = pr.Replace("+", ""); suf = suf.Replace("+", ""); stem = stem.Replace("+", "");
                    if (Row_Solutions[r].suf.Contains("POSS") || Row_Solutions[r].suf.StartsWith("e/V_SUF"))
                    {
                        voc = pr + stem + suf;
                    }
                    else
                    {
                        voc = pr + stem + suf + ecase;
                    }
                    if (voc != "")
                    {
                        Diacritized_Word = voc;
                        Get_Diacritics(voc, out Diacritized_Word);
                    }
                    else if (voc == "")
                    {
                        Diacritized_Word = Row_Solutions[r].word;
                    }

                }
                else
                {
                    Diacritized_Word = Row_Solutions[r].word;
                    Diacritized_Word = Diacritized_Word.Replace("\nS/\n/S\n", "\n");
                    Diacritized_Word = Diacritized_Word.Replace("\rS/\r/S\r", "\n\n");
                    Diacritized_Word = Diacritized_Word.Replace("\rS/\r\r/S\r", "\n\n");
                    Diacritized_Word = Diacritized_Word.Replace("\r\r", "\n\n");
                    Diacritized_Word = Diacritized_Word.Replace("/D\n/S\n", "");
                    Diacritized_Word = Diacritized_Word.Replace("\nS/\nD/", "");
                }
                Diacritized_Value = Diacritized_Value + " " + Diacritized_Word;
                Diacritized_Value = Diacritized_Value.Replace("\n ", "\n");
                Diacritized_Value = Diacritized_Value.Replace(" \n", "\n");
            }
            Diacritized_Text = Diacritized_Value;
            Case_Ending_Diacritized_Text = Diacritized_Text.Substring(2, Diacritized_Text.Length - 2);
            Case_Ending_Num = Have_Case_Ending.ToString();
            Need_Case_Ending_Words = Words_Need_Case_Ending.ToString();
            return;
        }

        private void Get_Diacritics(string Diacritized_Value, out string Diacritized_Text)
        {
            if (Diacritized_Word.Contains("maA zaAl"))
            {

            }
            Diacritized_Text = Diacritized_Value;
            if (!Diacritized_Text.Contains("maA zaAl"))
            {
            Diacritized_Text = Diacritized_Text.Replace(" ", "");
            }
            Diacritized_Text = Diacritized_Text.Replace("<", "إ");
            Diacritized_Text = Diacritized_Text.Replace(">", "أ");
            Diacritized_Text = Diacritized_Text.Replace("&", "ؤ");
            Diacritized_Text = Diacritized_Text.Replace("AF", "ًا");
            Diacritized_Text = Diacritized_Text.Replace("A", "ا");
            Diacritized_Text = Diacritized_Text.Replace("|", "آ");
            Diacritized_Text = Diacritized_Text.Replace("{", "ا");
            Diacritized_Text = Diacritized_Text.Replace("}", "ئ");
            Diacritized_Text = Diacritized_Text.Replace("'", "ء");
            Diacritized_Text = Diacritized_Text.Replace("b", "ب");
            Diacritized_Text = Diacritized_Text.Replace("t", "ت");
            Diacritized_Text = Diacritized_Text.Replace("v", "ث");
            Diacritized_Text = Diacritized_Text.Replace("j", "ج");
            Diacritized_Text = Diacritized_Text.Replace("H", "ح");
            Diacritized_Text = Diacritized_Text.Replace("x", "خ");
            Diacritized_Text = Diacritized_Text.Replace("d", "د");
            Diacritized_Text = Diacritized_Text.Replace("*", "ذ");
            Diacritized_Text = Diacritized_Text.Replace("r", "ر");
            Diacritized_Text = Diacritized_Text.Replace("z", "ز");
            Diacritized_Text = Diacritized_Text.Replace("s", "س");
            Diacritized_Text = Diacritized_Text.Replace("$", "ش");
            Diacritized_Text = Diacritized_Text.Replace("S", "ص");
            Diacritized_Text = Diacritized_Text.Replace("D", "ض");
            Diacritized_Text = Diacritized_Text.Replace("T", "ط");
            Diacritized_Text = Diacritized_Text.Replace("Z", "ظ");
            Diacritized_Text = Diacritized_Text.Replace("E", "ع");
            Diacritized_Text = Diacritized_Text.Replace("g", "غ");
            Diacritized_Text = Diacritized_Text.Replace("f", "ف");
            Diacritized_Text = Diacritized_Text.Replace("q", "ق");
            Diacritized_Text = Diacritized_Text.Replace("k", "ك");
            Diacritized_Text = Diacritized_Text.Replace("l", "ل");
            Diacritized_Text = Diacritized_Text.Replace("m", "م");
            Diacritized_Text = Diacritized_Text.Replace("n", "ن");
            Diacritized_Text = Diacritized_Text.Replace("h", "ه");
            Diacritized_Text = Diacritized_Text.Replace("w", "و");
            Diacritized_Text = Diacritized_Text.Replace("y", "ي");
            Diacritized_Text = Diacritized_Text.Replace("YF", "ًى");
            Diacritized_Text = Diacritized_Text.Replace("Y", "ى");
            Diacritized_Text = Diacritized_Text.Replace("p", "ة");
            Diacritized_Text = Diacritized_Text.Replace("e", "");
            Diacritized_Text = Diacritized_Text.Replace("a", "َ");
            Diacritized_Text = Diacritized_Text.Replace("u", "ُ");
            Diacritized_Text = Diacritized_Text.Replace("i", "ِ");
            Diacritized_Text = Diacritized_Text.Replace("K", "ٍ");
            Diacritized_Text = Diacritized_Text.Replace("F", "ً");
            Diacritized_Text = Diacritized_Text.Replace("N", "ٌ");
            Diacritized_Text = Diacritized_Text.Replace("o", "ْ");
            Diacritized_Text = Diacritized_Text.Replace("~", "ّ");
            Diacritized_Text = Diacritized_Text.Replace("`", "َٰ");
            Diacritized_Text = Diacritized_Text.Replace("َِ", "ِ");
            return;
        }
    }
}

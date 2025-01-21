using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;

namespace Arabic_Automatic_Diacritization
{
    class Set_Case_Ending_Stage
    {
        public Set_Case_Ending_Stage()
        {
        }
        #region Variabls and Constructors
        public int Case_ID;
        string Trans_Out;

        #endregion
        public void Get_Case_Ending_Diacritics(List<Analyzed_Text> Analyzed_Texts, out List<Analyzed_Text> Analyzed_Text, Form1 obj)
        {
            var Analayzed_List = Analyzed_Texts.Select(c => new { c.ID, c.word, c.lemmaID, c.pr, c.stem, c.suf, c.spattern, c.def, c.ecase, c.affectedBy }).ToList();

            #region From_Morphologically_Analyzed_Texts_To_Set_Case_Ending
            Analyzed_Texts.Clear();
            for (int d = 0; d < Analayzed_List.Count(); d++)
            {
                Analyzed_Text Solution_List = new Analyzed_Text();
                Solution_List.ID = Analayzed_List[d].ID;
                Solution_List.word = Analayzed_List[d].word;
                Solution_List.lemmaID = Analayzed_List[d].lemmaID;
                Solution_List.pr = Analayzed_List[d].pr;
                Solution_List.stem = Analayzed_List[d].stem;
                Solution_List.suf = Analayzed_List[d].suf;
                Solution_List.spattern = Analayzed_List[d].spattern;
                Solution_List.def = Analayzed_List[d].def;
                Solution_List.ecase = Analayzed_List[d].ecase;
                Solution_List.affectedBy = Analayzed_List[d].affectedBy;
                Analyzed_Texts.Add(Solution_List);
            }
            #endregion

            #region 1. Set_Case_For_Present_Verbs
            var IV_List = Analyzed_Texts.Where(a => ((a.stem.Contains("IV") && a.suf.StartsWith("e/V_SUF")) && a.ecase.Equals(""))).ToList();
            IV_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                #region Gemminated Verbs
                if (u.stem.Contains("~/"))
                {
                    if (u.pr.Contains("li/JUS") || (Case_ID > 2 && (Analyzed_Texts[Case_ID - 2].stem.Equals("hay~A/NOUN") || Analyzed_Texts[Case_ID - 2].stem.Equals("lam/PART") || Analyzed_Texts[Case_ID - 2].stem.Equals("lam~A/NOUN") || Analyzed_Texts[Case_ID - 2].stem.Equals("<in/CONJ") || (Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && Analyzed_Texts[Case_ID - 2].stem.Contains("~/IV")) || (Analyzed_Texts[Case_ID - 2].ecase.Equals("o") && Analyzed_Texts[Case_ID - 2].stem.Contains("IV")))))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.1";
                    }
                    else if (u.pr.Contains("li/SUB") || (Case_ID > 2 && (Analyzed_Texts[Case_ID - 2].stem.Equals("lan/PART") || Analyzed_Texts[Case_ID - 2].stem.Equals(">an/CONJ") || Analyzed_Texts[Case_ID - 2].stem.Equals("kay/CONJ") || Analyzed_Texts[Case_ID - 2].stem.Equals("likay/CONJ") || Analyzed_Texts[Case_ID - 2].stem.Equals("Hat~aY/PART") || Analyzed_Texts[Case_ID - 2].stem.Equals(">al~A/PART") || (Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && Analyzed_Texts[Case_ID - 2].stem.Contains("IV")))) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.2";
                    }
                    else if (Case_ID > 3 && (Analyzed_Texts[Case_ID - 3].stem.Equals("Hat~aY/PART") || Analyzed_Texts[Case_ID - 3].stem.Equals(">an/CONJ")) && Analyzed_Texts[Case_ID - 2].stem.Equals("lA/PART") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.3";
                    }
                    else if (Case_ID > 4 && (Analyzed_Texts[Case_ID - 4].stem.Equals("lan/PART") || Analyzed_Texts[Case_ID - 4].stem.Equals(">an/CONJ") || Analyzed_Texts[Case_ID - 4].stem.Equals("kay/CONJ") || Analyzed_Texts[Case_ID - 4].stem.Equals("likay/CONJ") || Analyzed_Texts[Case_ID - 4].stem.Equals("Hat~aY/PART") || Analyzed_Texts[Case_ID - 4].stem.Equals(">al~A/PART") || (Analyzed_Texts[Case_ID - 3].ecase.Equals("a") && Analyzed_Texts[Case_ID - 3].stem.Contains("IV"))) && Analyzed_Texts[Case_ID - 2].stem.Equals(">aw/CONJ") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.4";
                    }
                    else if (Case_ID > 2 && Analyzed_Texts[Case_ID - 2].stem.Equals("CV") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.5";
                    }
                    else if (((Case_ID > 2 || Case_ID == 3) && (Analyzed_Texts[Case_ID - 2].stem.Equals("man/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("<in/CONJ"))) && Analyzed_Texts[Case_ID].stem.Contains("IV") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.6";
                    }
                    else if ((Case_ID > 3 && (Analyzed_Texts[Case_ID - 3].stem.Equals("man/PRON") || Analyzed_Texts[Case_ID - 3].stem.Equals("<in/CONJ"))) && Analyzed_Texts[Case_ID - 2].stem.Contains("IV") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.7";
                    }
                    else
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.1.8";
                    }
                }
                #endregion

                #region Other Imperfect Verb Cases
                else
                {
                    if ((u.stem.Contains("Y/") || u.stem.Contains("A/")) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.1";
                    }
                    else if ((u.spattern.Equals("ful") || u.spattern.Equals("fil") || u.spattern.Equals("fal")) && !u.suf.Contains("na/V_SUF") && Analyzed_Texts[Case_ID - 1].ecase.Equals("") && !Analyzed_Texts[Case_ID - 1].lemmaID.StartsWith("w"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.2";
                    }
                    else if ((u.stem.Contains("a/") || u.stem.Contains("u/") || u.stem.Contains("i/")) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.3";
                    }
                    else if (Case_ID > 2 && Analyzed_Texts[Case_ID - 2].stem.Equals("CV") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.4";
                    }
                    else if ((u.pr.Contains("li/JUS") || (Case_ID > 2 && (Analyzed_Texts[Case_ID - 2].stem.Equals("hay~A/NOUN") || Analyzed_Texts[Case_ID - 2].stem.Equals("lam/PART") || Analyzed_Texts[Case_ID - 2].stem.Equals("lam~A/NOUN") || Analyzed_Texts[Case_ID - 2].stem.Equals("<in/CONJ") || (Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && Analyzed_Texts[Case_ID - 2].stem.Contains("~/IV")) || (Analyzed_Texts[Case_ID - 2].ecase.Equals("o") && Analyzed_Texts[Case_ID - 2].stem.Contains("IV"))))) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.5";
                    }
                    else if ((u.pr.Contains("li/SUB") || (Case_ID > 2 && (Analyzed_Texts[Case_ID - 2].stem.Equals("lan/PART") || Analyzed_Texts[Case_ID - 2].stem.Equals(">an/CONJ") || Analyzed_Texts[Case_ID - 2].stem.Equals("kay/CONJ") || Analyzed_Texts[Case_ID - 2].stem.Equals("likay/CONJ") || Analyzed_Texts[Case_ID - 2].stem.Equals("Hat~aY/PART") || Analyzed_Texts[Case_ID - 2].stem.Equals(">al~A/PART") || (Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && Analyzed_Texts[Case_ID - 2].stem.Contains("IV"))))) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.6";
                    }
                    else if ((Case_ID > 3 && (Analyzed_Texts[Case_ID - 3].stem.Equals("Hat~aY/PART") || Analyzed_Texts[Case_ID - 3].stem.Equals(">an/CONJ"))) && Analyzed_Texts[Case_ID - 2].stem.Equals("lA/PART") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.7";
                    }
                    else if (Case_ID > 4 && (Analyzed_Texts[Case_ID - 4].stem.Equals("lan/PART") || Analyzed_Texts[Case_ID - 4].stem.Equals(">an/CONJ") || Analyzed_Texts[Case_ID - 4].stem.Equals("kay/CONJ") || Analyzed_Texts[Case_ID - 4].stem.Equals("likay/CONJ") || Analyzed_Texts[Case_ID - 4].stem.Equals("Hat~aY/PART") || Analyzed_Texts[Case_ID - 4].stem.Equals(">al~A/PART") || (Analyzed_Texts[Case_ID - 3].ecase.Equals("a") && Analyzed_Texts[Case_ID - 3].stem.Contains("IV"))) && Analyzed_Texts[Case_ID - 2].stem.Equals(">aw/CONJ") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.8";
                    }
                    else if (((Case_ID > 2 || Case_ID == 3) && (Analyzed_Texts[Case_ID - 2].stem.Equals("man/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("<in/CONJ"))) && Analyzed_Texts[Case_ID].stem.Contains("IV") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.9";
                    }
                    else if ((Case_ID > 3 && (Analyzed_Texts[Case_ID - 3].stem.Equals("man/PRON") || Analyzed_Texts[Case_ID - 3].stem.Equals("<in/CONJ"))) && Analyzed_Texts[Case_ID - 2].stem.Contains("IV") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "o";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.10";
                    }
                    else if ((u.stem.Contains("w/") || u.stem.Contains("y/")) && u.ecase.Contains(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.11";
                    }
                    else if (u.ecase.Contains(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.12";
                    }
                }

                #endregion
            });

            #region IV_Default
            IV_List = Analyzed_Texts.Where(a => (a.stem.Contains("IV") && a.ecase.Equals("") && a.ecase.Equals(""))).ToList();
            IV_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "e";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:1.2.13";
            });
            #endregion
            #endregion

            #region 2. Set_Case_For_NOUNs_ADJs_Exception_or_Default_Case
            var Exp_List = Analyzed_Texts.Where(a => (a.stem.StartsWith("Hayov/") || a.stem.StartsWith("muno*/") || a.stem.Equals("qaT~/NOUN")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "u";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.1";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.StartsWith("Al|n/") || a.stem.StartsWith("hunAk/") || a.stem.StartsWith("maE/") || a.stem.StartsWith("TawAl/") || a.stem.StartsWith("buEayod/") || a.stem.StartsWith(">ay~An/") || a.stem.StartsWith("qubayol/") || a.stem.StartsWith("vam~/") || a.stem.StartsWith(">ayon/") || (a.stem.Equals("ragom/NOUN") && !a.pr.Contains("Al/DET"))) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "a";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.2";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.StartsWith("<i*A/") || a.stem.StartsWith("<i*/") || a.stem.StartsWith("hunA/") || a.stem.StartsWith("mivolamA/") || a.stem.StartsWith("ladaY/") || a.stem.StartsWith("|na*Ak/") || a.stem.StartsWith(">ayonamA/") ||
            a.stem.StartsWith("baEodamA/") || a.stem.StartsWith("bayonamA/") || a.stem.StartsWith("EinodamA/") || a.stem.StartsWith("HAlamA/") || a.stem.StartsWith("HawAlay/") || a.stem.StartsWith("HayovumA/") ||
            a.stem.StartsWith("HiynamA/") || a.stem.StartsWith("Hiyna*Ak/") || a.stem.StartsWith("kayofamA") || a.stem.StartsWith("kul~amA/") || a.stem.StartsWith("lam~A/") || a.stem.StartsWith("mataY/") || a.stem.StartsWith("qal~amA/") ||
            a.stem.StartsWith("rayovamA/") || a.stem.StartsWith("TAlamA/") || a.stem.StartsWith("ladaY/") || a.stem.StartsWith("laday/") || a.stem.StartsWith("waqota*Ak/") || a.stem.StartsWith("waqotamA/") || a.stem.StartsWith("faqaT/") || a.stem.StartsWith("h`ka*A/") || a.stem.StartsWith("kilA/")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "e";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.3";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.StartsWith(">aHad") || a.stem.StartsWith("HAdiy") || a.stem.StartsWith("vAniy") || a.stem.StartsWith("valAv") || a.stem.StartsWith(">arobaE") ||
            a.stem.StartsWith("xamos") || a.stem.StartsWith("sit~") || a.stem.StartsWith("saboE") || a.stem.StartsWith("vamAniy") || a.stem.StartsWith("tisoE") || a.stem.StartsWith("vAliv") ||
            a.stem.StartsWith("rAbiE") || a.stem.StartsWith("xAmis") || a.stem.StartsWith("sAdis") || a.stem.StartsWith("sAbiE") || a.stem.StartsWith("vAmin") || a.stem.StartsWith("tAsiE")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.StartsWith("Ea$ar") || Analyzed_Texts[Case_ID].stem.StartsWith("Ea$or"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.4";
                }
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.StartsWith("Ea$ar") || a.stem.StartsWith("Ea$or")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Case_ID > 2 && (Analyzed_Texts[Case_ID - 2].stem.StartsWith(">aHad") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("HAdiy") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("{ivonA") ||
                Analyzed_Texts[Case_ID - 2].stem.StartsWith("vAniy") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("valAv") || Analyzed_Texts[Case_ID - 2].stem.StartsWith(">arobaE") ||
                Analyzed_Texts[Case_ID - 2].stem.StartsWith("xamos") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("sit~") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("saboE") ||
                Analyzed_Texts[Case_ID - 2].stem.StartsWith("vamAniy") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("tisoE") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("vAliv") ||
                Analyzed_Texts[Case_ID - 2].stem.StartsWith("rAbiE") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("xAmis") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("sAdis") ||
                Analyzed_Texts[Case_ID - 2].stem.StartsWith("sAbiE") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("vAmin") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("tAsiE"))))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.5";
                }
            });

            Exp_List = Analyzed_Texts.Where(a => a.stem.StartsWith("lab~/") && a.suf.StartsWith("ayo/N_SUF") && a.suf.Contains("POSS") && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID].ecase = "e";
                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.6";
            });

            Exp_List = Analyzed_Texts.Where(a => a.ecase.StartsWith("vam~/")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && !Analyzed_Texts[Case_ID].pr.Contains("PREP") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID].ecase = "u";
                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.7.1";
                }
                else if ((Analyzed_Texts[Case_ID].def.Equals("INDEF")) && !Analyzed_Texts[Case_ID].pr.Contains("PREP") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID].ecase = "N";
                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.7.2";
                }
            });

            Exp_List = Analyzed_Texts.Where(a => (a.suf.StartsWith("uwna/N_SUF") || a.suf.StartsWith("uw/N_SUF") || a.suf.StartsWith("atAni/N_SUF") || a.suf.StartsWith("atA/N_SUF") || a.suf.StartsWith("Ani/N_SUF") || a.suf.StartsWith("A/N_SUF") || a.suf.Contains("iy/POSS") || a.suf.StartsWith("iyna/N_SUF") || a.suf.StartsWith("iy/N_SUF") || a.suf.StartsWith("atayoni/N_SUF") || a.suf.StartsWith("atayo/N_SUF") || a.suf.StartsWith("ayoni/N_SUF") || a.suf.StartsWith("ayo/N_SUF") || a.suf.StartsWith("awoA/N_SUF")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "e";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.8";
            });

            Exp_List = Analyzed_Texts.Where(a => a.word.EndsWith("ا") && !a.stem.Contains("A/") && a.suf.Equals("") && (a.stem.Contains("/NOUN") || a.stem.Contains("/ADJ") || a.stem.Contains("/ADV")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "F";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.9";
            });

            Exp_List = Analyzed_Texts.Where(a => a.stem.StartsWith("Hayov/") || a.stem.StartsWith("siy~amA/") || a.stem.StartsWith("lam~A/")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Contains("NOUN") && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID].ecase = "N";
                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.10.1";
                }
                else
                {
                    if ((Analyzed_Texts[Case_ID].suf.Contains("iy/POSS") || Analyzed_Texts[Case_ID].stem.Contains("Y") || Analyzed_Texts[Case_ID].stem.Contains("w") || Analyzed_Texts[Case_ID].stem.Contains("A")) && Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "e";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.10.2";
                    }
                    else
                    {
                        if (Analyzed_Texts[Case_ID].stem.Contains("NOUN") && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.10.3";
                        }
                    }
                }
            });

            Exp_List = Analyzed_Texts.Where(a => a.stem.StartsWith(">abuw/") && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "e";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.11";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.Contains("Y/") || (a.stem.Contains("A/") && a.suf.Equals(""))) && (a.stem.EndsWith("/NOUN") || a.stem.EndsWith("/ADJ")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (u.def.Equals("DEF") || u.def.Equals("EDAFAH") && u.ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.12.1";
                }
                else
                {
                    if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                    {
                        if (u.stem.Contains("Y/") && u.lemmaID.EndsWith("Y") && !u.lemmaID.Equals("mataY") && (!u.spattern.EndsWith("Y") ) && !u.spattern.Equals(">afoEal"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.12.2.1";
                        }
                        else if ((u.spattern.EndsWith("Y") || u.spattern.Equals("")) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "e";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.12.2.2";
                        }
                        else if (u.spattern.Equals("faEol") && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.12.2.3";
                        }
                        else if (u.stem.Equals("A/") && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "e";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.12.2.4";
                        }
                    }
                }
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.Contains("Hiyna}i*/") || a.stem.Contains("waqota}i*/")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "K";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.13";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.Equals("baEod/NOUN") || a.stem.Equals("qabol/NOUN")) && (a.suf.Equals("")) && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Case_ID > 2 && Analyzed_Texts[Case_ID - 2].stem.Equals("min/PREP") && Analyzed_Texts[Case_ID - 2].suf.Equals(""))
                {
                    if (!Analyzed_Texts[Case_ID].stem.Contains("/NOUN") && !Analyzed_Texts[Case_ID].stem.Contains("/CONJ") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "u";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.14.1";
                    }
                    else
                    {
                        if ((Analyzed_Texts[Case_ID].stem.Contains("/PV") || Analyzed_Texts[Case_ID].stem.Contains("/IV")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.14.2";
                        }
                    }
                }
            });

            Exp_List = Analyzed_Texts.Where(a => (a.lemmaID.Equals("EAdap")) && a.def.Equals("INDEF") && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "F";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.15";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN") && (a.suf.Equals("awoA/N_SUF") || a.suf.Equals("uwA/N_SUF") || a.suf.Equals("a/N_SUF") || a.suf.Equals("i/N_SUF")) && a.ecase.Equals(""))).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "e";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16";
            });

            Exp_List = Analyzed_Texts.Where(a => (a.lemmaID.Equals("Hasob") && a.ecase.Equals(""))).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID - 2].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 1].pr.Contains("/PREP"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.17.1";
                }
                else if ((Analyzed_Texts[Case_ID - 1].pr.Equals("fa/CONJ") || Analyzed_Texts[Case_ID].def.Equals("INDEF")) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.17.1";
                }
                else if (u.ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.17.2";
                }
            });

            #region 2.16 Set_Case_For_ADV(T&P&M)
            #region 2.16.1 ADV_M
            Exp_List = Analyzed_Texts.Where(a => a.stem.Contains("ADV") && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                if (u.ecase.Equals(""))
                {
                    Case_ID = int.Parse(u.ID);
                    if (u.def.Equals("EDAFAH") || u.stem.Contains("kayof/") || u.spattern.Equals("fuEalaA'"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.1.1";
                    }
                    else
                    {
                        if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                        {
                            if (u.suf.Equals("At/N_SUF") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "K";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.16.1.2";
                            }
                            else
                            {
                                if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID].ecase = "F";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:2.16.1.3";
                                }
                            }
                        }
                    }
                }
            });

            #endregion

            #region 2.16.2 ADV_T&P
            Exp_List = Analyzed_Texts.Where(a => (a.stem.Equals("qabol/NOUN") || a.stem.Equals("baEod/NOUN") || a.stem.Equals("bayon/NOUN") || a.stem.Equals("xilAl/NOUN") || a.stem.Equals("naHow/NOUN") ||
             a.stem.Equals("<ivor/NOUN") || a.stem.Equals("dAxil/NOUN") || a.stem.Equals("taHot/NOUN") || a.stem.Equals("duwn/NOUN") || a.stem.Equals("muqAbil/NOUN") ||
             a.stem.Equals("xalof/NOUN") || a.stem.Equals("Hawol/NOUN") || a.stem.Equals("Did~/NOUN") || a.stem.Equals("xArij/NOUN") || a.stem.Equals(">amAm/NOUN") ||
             a.stem.Equals("Eabor/NOUN") || a.stem.Equals("tijAh/NOUN") || a.stem.Equals("fawoq/NOUN") || a.stem.Equals("wasaT/NOUN") || a.stem.Equals("Sawob/NOUN") ||
             a.stem.Equals(">asofal/NOUN") || a.stem.Equals("HiyAl/NOUN") || a.stem.Equals("$amAl/NOUN") || a.stem.Equals("garob/NOUN") || a.stem.Equals("$aroq/NOUN") ||
             a.stem.Equals("$aroqiy~/NOUN") || a.stem.Equals("garobiy~/NOUN") || a.stem.Equals("$amAliy~/NOUN") || a.stem.Equals("Dimon/NOUN") || a.stem.Equals("warA'/NOUN") ||
             a.stem.Equals("<izA'/NOUN") || a.stem.Equals("maTolaE/NOUN") || a.stem.Equals("|nA'/NOUN") || a.stem.Equals(">avonA'/NOUN") || a.stem.Equals("Hiyn/NOUN") || a.stem.Equals("Eaqib/NOUN")) && !a.pr.Contains("Al/DET") && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (u.ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID - 2].stem.Contains("/PREP") || u.pr.Contains("/PREP"))
                    {
                        Case_ID = int.Parse(u.ID);
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.2.1";
                    }
                    else
                    {
                        Case_ID = int.Parse(u.ID);
                        if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "u";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.2.2";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.2.3";
                        }
                    }
                }
            });

            Exp_List = Analyzed_Texts.Where(a => a.stem.Equals("masA'/NOUN") && a.ecase.Equals("")).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID - 1].def.Equals("EDAFAH"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.2.3";
                }
                else
                {
                    if (Analyzed_Texts[Case_ID - 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID - 2].stem.Equals("*At/NOUN"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.2.4.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.2.4.2";
                        }
                    }
                }
            });
            #endregion

            #region 2.16.3 ADV_M
            Exp_List = Analyzed_Texts.Where(a => (a.stem.Contains("/ADV") && a.ecase.Equals(""))).ToList();
            Exp_List.ToList().ForEach(u =>
            {
                if (u.ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID - 1].def.Equals("EDAFAH"))
                    {
                        Case_ID = int.Parse(u.ID);
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.3.1";
                    }
                    else
                    {
                        Case_ID = int.Parse(u.ID);
                        Analyzed_Texts[Case_ID - 1].ecase = "F";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:2.16.3.2";
                    }
                }
            });

            #endregion
            #endregion
            #endregion

            #region 3.Set_Case_For_NOUNs
            #region 3.1 if the prefix of the word contains PREP
            var N_List = Analyzed_Texts.Where(a => a.pr.Contains("PREP") && (a.stem.EndsWith("/NOUN") || a.stem.Equals("gayor/PART") || a.stem.EndsWith("/PRON")) && a.ecase.Equals("")).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((u.stem.Equals("h`*A/PRON") || u.stem.Equals("h`*ihi/PRON") || u.stem.Equals("h`&ulA'i/PRON") || u.stem.Equals("h`*Ani/PRON") ||
                     u.stem.Equals("h`*ayoni/PRON") || u.stem.Equals("hAtayoni/PRON") || u.stem.Equals("*`lika/PRON") || u.stem.Equals("*Aka/PRON") ||
                     u.stem.Equals("h`*iy/PRON") || u.stem.Equals("tiloka/PRON") || u.stem.Equals(">uwla}ika/PRON")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID].def.Equals("DEF"))
                    {
                        Analyzed_Texts[Case_ID].ecase = "i";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.1.10";
                    }
                }
                else if (Analyzed_Texts[Case_ID - 1].ecase.Equals("") && !u.stem.Contains("/PRON"))
                {
                    Case_ID = int.Parse(u.ID);
                    if ((u.suf.Equals("iyna/N_SUF") || u.suf.Equals("iy/N_SUF") || u.suf.Equals("atayoni/N_SUF") || u.suf.Equals("atayo/N_SUF") || u.suf.Equals("ayoni/N_SUF") || u.suf.Equals("ayo/N_SUF") || u.suf.Contains("iy/POSS")) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.1";
                    }
                    else if ((u.stem.Contains("oy/") || u.stem.Contains("ow/")) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("DEF") || u.def.Equals("EDAFAH"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.2.1";
                        }
                        else
                        {
                            if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "K";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.2.2";
                            }
                        }
                    }
                    else if (u.stem.Contains("w/") && u.ecase.Equals(""))
                    {
                        if (u.suf.Equals("") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.3.1";
                        }

                        else if ((u.suf.Contains("POSS") || (u.suf.Equals("") && (u.def.Equals("EDAFAH"))) || (u.def.Equals("DEF"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "e";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.3.2";
                        }
                        else if ((u.suf.StartsWith("at/N_SUF") || u.suf.Equals("ap/N_SUF")) && u.def.Equals("INDEF") && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.3.3";
                        }
                        else if ((u.suf.StartsWith("at/N_SUF") || u.suf.Equals("ap/N_SUF")) && (u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.3.4";
                        }
                    }
                    else if ((u.def.Equals("INDEF")) && (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") ||
                            u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                            u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") ||
                            u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") ||
                            u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                            u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.4";
                    }
                    else if (u.stem.Equals(">amos") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.5";
                    }
                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "K";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.6";
                    }
                    else if ((u.stem.Contains("|/") || u.stem.Contains("y/")) && (u.suf.Contains("POSS") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.7";
                    }
                    else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        if ((u.stem.Contains("y/") && !u.stem.Contains("oy/")) || (u.stem.Contains("w/") && !u.stem.Contains("ow/")))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "e";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.8.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.8.2";
                        }
                    }
                    else if ((u.stem.Contains("gayor/")) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.9";
                    }
                }

            });
            #endregion

            #region 3.2 if the previous Word of the Nominal word is EDAFAH
            N_List = Analyzed_Texts.Where(a => (a.def.Equals("EDAFAH") || a.stem.Equals("gayor/PART")) && !a.suf.Contains("POSS") && !a.stem.StartsWith("vam~/") && !a.stem.StartsWith("Hayov/")).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID].stem.Equals("h`*A/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*ihi/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`&ulA'i/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*Ani/PRON") ||
                     Analyzed_Texts[Case_ID].stem.Equals("h`*ayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("hAtayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*`lika/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*Aka/PRON") ||
                     Analyzed_Texts[Case_ID].stem.Equals("h`*iy/PRON") || Analyzed_Texts[Case_ID].stem.Equals("tiloka/PRON") || Analyzed_Texts[Case_ID].stem.Equals(">uwla}ika/PRON")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("DEF"))
                    {
                        Analyzed_Texts[Case_ID + 1].ecase = "i";
                        Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.2.15.1";
                    }
                    else if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH"))
                    {
                        Analyzed_Texts[Case_ID + 1].ecase = "i";
                        Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.2.15.2";
                    }
                }
                else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].suf.Equals("iyna/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("iy/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("atayoni/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("atayo/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("ayoni/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("ayo/N_SUF") || Analyzed_Texts[Case_ID].suf.Contains("iy/POSS")) && !Analyzed_Texts[Case_ID].pr.StartsWith("wa/CONJ") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.1.1";
                        }
                        else if (u.stem.Equals("<i*A/NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                if ((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/")))
                                {
                                    Analyzed_Texts[Case_ID].ecase = "e";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.1.2.1.1";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID].ecase = "u";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.1.2.1.2";
                                }
                            }
                            else if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.1.2.2";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID - 1].stem.Contains("oy/") || Analyzed_Texts[Case_ID - 1].stem.Contains("ow")) && u.ecase.Equals(""))
                        {
                            if (!Analyzed_Texts[Case_ID].pr.StartsWith("wa/CONJ") && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")))
                            {
                                Analyzed_Texts[Case_ID].ecase = "i";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.2.1";
                            }
                            else if (Analyzed_Texts[Case_ID].def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID].ecase = "K";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.2.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID].stem.Contains("oy/") || Analyzed_Texts[Case_ID].stem.Contains("ow/"))
                        {
                            if ((Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("")) && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "K";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.3.1";
                            }
                            else if ((Analyzed_Texts[Case_ID].suf.Equals("") || Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("at/N_SUF")) && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && !Analyzed_Texts[Case_ID].pr.StartsWith("wa/CONJ") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "i";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.3.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID].stem.Contains("y/") || Analyzed_Texts[Case_ID].stem.Contains("w/"))
                        {
                            if ((Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF")) && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "K";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.4.1";
                            }
                            else if ((Analyzed_Texts[Case_ID].suf.Equals("") || Analyzed_Texts[Case_ID].suf.Contains("POSS")) && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.4.2";
                            }
                            else if ((Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("at/N_SUF")) && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "i";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.4.3";
                            }
                            else if ((Analyzed_Texts[Case_ID].suf.Equals("") || Analyzed_Texts[Case_ID].suf.Contains("POSS")) && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.4.4";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID].stem.Equals("gad/NOUN"))
                            {
                                Analyzed_Texts[Case_ID].ecase = "K";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.5.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "i";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.5.2";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID].def.Equals("INDEF")) && (Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") ||
                              Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") ||
                              Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") ||
                              Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") ||
                              Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") ||
                              Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID].stem.Contains("~/"))) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.6";
                        }

                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "K";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.7";
                        }
                    }

                    else if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID + 1].suf.Equals("iyna/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("iy/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("atayoni/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("atayo/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("ayoni/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("ayo/N_SUF") || Analyzed_Texts[Case_ID].suf.Contains("iy/POSS")) && !Analyzed_Texts[Case_ID].pr.StartsWith("wa/CONJ") && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "e";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.8";
                        }
                        else if ((u.stem.Contains("oy/") || u.stem.Contains("ow")) && u.ecase.Equals(""))
                        {
                            if (!Analyzed_Texts[Case_ID + 1].pr.StartsWith("wa/CONJ") && (Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "i";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.9.1";
                            }
                            else if (Analyzed_Texts[Case_ID + 1].def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "K";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.9.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID + 1].stem.Contains("oy/") || Analyzed_Texts[Case_ID + 1].stem.Contains("ow/"))
                        {
                            if ((Analyzed_Texts[Case_ID + 1].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("At/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("")) && Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "K";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.10.1";
                            }
                            else if ((Analyzed_Texts[Case_ID + 1].suf.Equals("") || Analyzed_Texts[Case_ID + 1].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("At/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("at/N_SUF")) && (Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && !Analyzed_Texts[Case_ID + 1].pr.StartsWith("wa/CONJ") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "i";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.2.10.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID + 1].stem.Contains("y/") || Analyzed_Texts[Case_ID + 1].stem.Contains("w/"))
                        {
                            if ((Analyzed_Texts[Case_ID + 1].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("At/N_SUF")) && Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "K";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.11.1";
                            }
                            else if ((Analyzed_Texts[Case_ID + 1].suf.Equals("") || Analyzed_Texts[Case_ID + 1].suf.Contains("POSS")) && Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "e";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.11.2";
                            }
                            else if ((Analyzed_Texts[Case_ID + 1].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("At/N_SUF") || Analyzed_Texts[Case_ID + 1].suf.Equals("at/N_SUF")) && (Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "i";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.11.3";
                            }
                            else if ((Analyzed_Texts[Case_ID + 1].suf.Equals("") || Analyzed_Texts[Case_ID + 1].suf.Contains("POSS")) && (Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "e";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.11.4";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID + 1].stem.Equals("gad/NOUN"))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "K";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.12.1";
                            }
                            else Analyzed_Texts[Case_ID + 1].ecase = "i";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.12.2";
                        }
                        else if ((Analyzed_Texts[Case_ID + 1].def.Equals("INDEF")) && (Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEiyl") ||
                              Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAlil") ||
                              Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEil") ||
                              Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fayaAEil") ||
                              Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fuEalaA'") ||
                              Analyzed_Texts[Case_ID + 1].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID + 1].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID + 1].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID + 1].stem.Contains("~/"))) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "a";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.13";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "K";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.2.14";
                        }
                    }
                }
            });
            #endregion

            #region 3.3 if the previous stem of the Nominal word contains PREP
            N_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/PREP")) && a.suf.Equals("")).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID].stem.Equals("h`*A/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*ihi/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`&ulA'i/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*Ani/PRON") ||
                     Analyzed_Texts[Case_ID].stem.Equals("h`*ayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("hAtayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*`lika/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*Aka/PRON") ||
                     Analyzed_Texts[Case_ID].stem.Equals("h`*iy/PRON") || Analyzed_Texts[Case_ID].stem.Equals("tiloka/PRON") || Analyzed_Texts[Case_ID].stem.Equals(">uwla}ika/PRON")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("DEF"))
                    {
                        Analyzed_Texts[Case_ID + 1].ecase = "i";
                        Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.3.9";
                    }
                }

                else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Analyzed_Texts[Case_ID].suf.Equals("iyna/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("iy/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("atayoni/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("atayo/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("ayoni/N_SUF") || Analyzed_Texts[Case_ID].suf.Equals("ayo/N_SUF") || Analyzed_Texts[Case_ID].suf.Contains("iy/POSS")) && !Analyzed_Texts[Case_ID].pr.StartsWith("wa/CONJ") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "e";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.1";
                    }
                    else if ((u.stem.Contains("oy/") || u.stem.Contains("ow")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "i";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.2.1";
                        }
                        else if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "K";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.2.2";
                        }
                    }
                    else if ((Analyzed_Texts[Case_ID].stem.Contains("y/") || Analyzed_Texts[Case_ID].stem.Contains("w/")) && !Analyzed_Texts[Case_ID].suf.StartsWith("ap/") && !Analyzed_Texts[Case_ID].suf.StartsWith("at/") && !Analyzed_Texts[Case_ID].suf.StartsWith("At/") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].suf.Equals("") && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.3.1";
                        }
                        else if (Analyzed_Texts[Case_ID].suf.Contains("POSS") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.3.2";
                        }
                        else if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                             Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                             Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                             Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fuEolaY") ||
                             Analyzed_Texts[Case_ID].spattern.Equals("fiEolaY") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaY") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlaY") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                             Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                             (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl'") && Analyzed_Texts[Case_ID].stem.Contains("'/"))) && (Analyzed_Texts[Case_ID].stem.Contains("A/")) && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.3.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].stem.Contains("A/") && Analyzed_Texts[Case_ID].suf.Equals("") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "e";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.4";
                    }
                    else if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "i";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.5";
                    }
                    else if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                        Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                        Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                        Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                        Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                        (Analyzed_Texts[Case_ID].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(awA).(i)."))) || Analyzed_Texts[Case_ID].stem.StartsWith("jahan~am") ||
                        (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) && !Analyzed_Texts[Case_ID].stem.StartsWith(">arobaE/") && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals("")))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.6";
                    }
                    else if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].stem.StartsWith("gad/") && Analyzed_Texts[Case_ID].pr.StartsWith("Al/DET") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "i";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.7";
                    }
                    else if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "K";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.8";
                    }
                    else if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "K";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.9";
                    }
                }
                else if (Analyzed_Texts[Case_ID].stem.Contains("gayor/") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID].ecase = "i";
                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.3.9";
                }
            });
            #endregion

            #region rules transfered due to priority restrictions
            #region EilAwapF EalaY -- <iDAfapF li or <ilaY 
            N_List = Analyzed_Texts.Where(a => (a.lemmaID.Equals("EilAwap") || a.lemmaID.Equals("<i$Arap") || a.lemmaID.Equals("<iDAfap") || a.lemmaID.Equals("muqAranap") || a.lemmaID.Equals("niyAbap") || a.lemmaID.Equals("{isotijAbap")) && a.suf.Equals("ap/N_SUF") && a.def.Equals("INDEF") && a.ecase.Equals("")).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Contains("/PREP") || Analyzed_Texts[Case_ID].pr.Contains("/PREP"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.5.5";
                }
            });
            #endregion

            #region xAS~ap >an~a
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals("xAS~/NOUN") && a.suf.Equals("ap/N_SUF") && a.def.Equals("INDEF") && a.ecase.Equals(""))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Equals(">an~a/CONJ") || Analyzed_Texts[Case_ID].pr.Contains("/PREP"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.5.8";
                }
            });
            #endregion

            #region 3.5.5 After xuSuwSAF
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals("xuSuwS/NOUN") && a.word.EndsWith("ا") && a.suf.Equals(""))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.Contains("iy/") && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "e";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1";
                    }
                    else if ((Analyzed_Texts[Case_ID].def.Contains("DEF") || Analyzed_Texts[Case_ID].def.Contains("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.5.1";
                    }
                }
            });
            #endregion            

            #region 3.12 AlEAma, All~ayolapa, Al$~ahora
            var Days_Years_ADV = Analyzed_Texts.Where(a => ((((a.stem.Equals("EAm/NOUN") || a.stem.Equals("$ahor/NOUN")) && a.suf.Equals("")) || ((a.stem.Equals("san/NOUN") || a.stem.Equals("layol/NOUN") || a.stem.Equals("bAriH/NOUN")) && a.suf.Equals("ap/N_SUF")) && a.pr.StartsWith("Al/DET")) && a.ecase.Equals(""))).ToList();
            Days_Years_ADV.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID - 2].stem.Equals("h`*A/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("h`*ihi/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("h`&ulA'i/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("h`*Ani/PRON") ||
                     Analyzed_Texts[Case_ID - 2].stem.Equals("h`*ayoni/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("hAtayoni/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("*`lika/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("*Aka/PRON") ||
                     Analyzed_Texts[Case_ID - 2].stem.Equals("h`*iy/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("tiloka/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals(">uwla}ika/PRON")))
                {
                    if (Analyzed_Texts[Case_ID - 3].stem.StartsWith("$itA'/") || Analyzed_Texts[Case_ID - 3].stem.StartsWith("Sayof/") || Analyzed_Texts[Case_ID - 3].stem.StartsWith("rabiyE/") || Analyzed_Texts[Case_ID - 3].stem.StartsWith("xariyf/"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.12.1";
                    }
                    else if (Analyzed_Texts[Case_ID - 3].stem.Contains("<s>"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.12.2";
                    }
                    else if (u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.12.3";
                    }
                }
                else
                {
                    if (Analyzed_Texts[Case_ID - 2].stem.StartsWith("$itA'/") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("Sayof/") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("rabiyE/") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("xariyf/"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.12.4";
                    }
                    else if (u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.12.5";
                    }
                }
            });
            #endregion

            #region 3.5.7 natiyjapa & natiyjapF li
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals("natiyj/NOUN") && a.suf.Equals("ap/N_SUF") && a.ecase.Equals(""))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (u.def.Equals("EDAFAH") && u.ecase.Equals(""))
                {

                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.5.7.1";
                }
                else if (u.def.Equals("INDEF") && Analyzed_Texts[Case_ID].pr.StartsWith("li/PREP") && u.ecase.Equals(""))
                {

                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.5.7.2";
                }
            });
            #endregion
            #endregion

            #region 3.4 set NOUNs as Nominative
            #region 3.4.1 if the previous stem equal ">am~A/PART" or the previous stem equal "lawolA/CONJ"
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals(">am~A/PART") || a.stem.Equals("lawolA/CONJ") || a.stem.Equals("<in~amA/CONJ"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID].stem.Equals("huwa/PRON") || Analyzed_Texts[Case_ID].stem.Equals("hiya/PRON")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && (Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")))
                    {
                        Analyzed_Texts[Case_ID + 1].ecase = "u";
                        Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.1.6";
                    }
                }
                else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))) && (Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].suf.Equals("") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.1.1";
                        }
                        else
                        {
                            if (Analyzed_Texts[Case_ID].pr.Contains("Al/DET") && Analyzed_Texts[Case_ID].stem.Equals("yawom/NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "a";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.1.2";
                            }
                            else
                            {
                                if (!Analyzed_Texts[Case_ID].pr.Contains("Al/DET") && (Analyzed_Texts[Case_ID].stem.Equals("dAxil/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("Einod/NOUN")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID].ecase = "a";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.1.3";
                                }
                                else
                                {
                                    if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID].ecase = "u";
                                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.1.4";
                                    }
                                    else
                                    {
                                        if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                        {
                                            Analyzed_Texts[Case_ID].ecase = "N";
                                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.1.5";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.4.2 if the noun is found at the begining of the sentence
            N_List = Analyzed_Texts.Where(a => (a.stem.Contains("<s>") || a.word.Equals(":") || a.stem.Contains("huwa/PRON") || a.stem.Contains("hiya/PRON") || a.stem.Contains(">anota/PRON") || a.stem.Contains(">anoti/PRON") )).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID].stem.Equals("h`*A/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*ihi/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`&ulA'i/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*Ani/PRON") ||
                   Analyzed_Texts[Case_ID].stem.Equals("h`*ayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("hAtayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*`lika/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*Aka/PRON") ||
                   Analyzed_Texts[Case_ID].stem.Equals("h`*iy/PRON") || Analyzed_Texts[Case_ID].stem.Equals("tiloka/PRON") || Analyzed_Texts[Case_ID].stem.Equals(">uwla}ika/PRON")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("DEF"))
                    {
                        Analyzed_Texts[Case_ID + 1].ecase = "u";
                        Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.8";
                    }
                }
                else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))) && Analyzed_Texts[Case_ID].suf.Equals("") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.1";
                        }
                        else if (Analyzed_Texts[Case_ID].pr.Contains("Al/DET") && Analyzed_Texts[Case_ID].stem.Equals("yawom/NOUN") && !Analyzed_Texts[Case_ID + 1].suf.Contains("ADJ") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.2";
                        }
                        else if ((Analyzed_Texts[Case_ID].stem.Equals("*At/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("<ivor/NOUN") || Analyzed_Texts[Case_ID].stem.Equals(">amAm/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals(">avonA'/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("baEod/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("bayon/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("Dimon/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("duwn/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("EAm/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("Eaqib/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("Einod/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("fajor/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("fawoq/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("fawor/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("Hasob/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("Hawol/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("laHoZ/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("maE/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("naHow/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("qabol/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("qubayol/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("taHot/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("warA'/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("wasaT/NOUN") ||
                                Analyzed_Texts[Case_ID].stem.Equals("xArij/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("Hiyn/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("xilAl/NOUN") || 
                                Analyzed_Texts[Case_ID].stem.Equals("yawom/NOUN")) && Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.3";
                        }
                        else if ((Analyzed_Texts[Case_ID].spattern.Equals("faAE") || Analyzed_Texts[Case_ID].spattern.Equals("mufaAE")) && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.4";
                        }
                        else if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                                (Analyzed_Texts[Case_ID].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(awA).(i)."))) || Analyzed_Texts[Case_ID].stem.StartsWith("jahan~am") ||
                                (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) && !Analyzed_Texts[Case_ID].stem.StartsWith(">arobaE/") && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals("")))
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.5";
                        }
                        else if ((Analyzed_Texts[Case_ID].def.Equals("INDEF") && !Analyzed_Texts[Case_ID].spattern.StartsWith("kam/") && !Analyzed_Texts[Case_ID].stem.StartsWith("Tay~ib/")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            if ((((Analyzed_Texts[Case_ID].stem.Equals("fajo>") || Analyzed_Texts[Case_ID].stem.StartsWith("mar~/") || Analyzed_Texts[Case_ID].suf.StartsWith("natiyj/")) && Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF")) || Analyzed_Texts[Case_ID].stem.StartsWith("<iymA'/")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "F";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.6.1";
                            }
                            else if (Analyzed_Texts[Case_ID].spattern.Equals("mafaAE") && Analyzed_Texts[Case_ID].def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID].ecase = "K";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.6.2";
                            }
                            else if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.6.3";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            if ((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/")))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.7.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.2.7.2";
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.4.3 if the noun is found after ADJ + 'uwna' or the ADJ after NOUN + 'uwna'
            N_List = Analyzed_Texts.Where(a => (a.suf.Equals("uwna/N_SUF"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (u.stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID].stem.EndsWith("/ADJ") && u.def.Equals(Analyzed_Texts[Case_ID].def) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")))
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.3.1.1";
                        }
                        else
                        {
                            if (u.def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.3.1.2";

                            }
                        }
                    }
                    else if (u.stem.EndsWith("/ADJ") && Analyzed_Texts[Case_ID - 2].stem.EndsWith("/NOUN") && u.def.Equals(Analyzed_Texts[Case_ID - 2].def) && Analyzed_Texts[Case_ID - 2].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID - 2].def.Equals("DEF") || Analyzed_Texts[Case_ID - 2].def.Equals("EDAFAH")))
                        {
                            Analyzed_Texts[Case_ID - 2].ecase = "u";
                            Analyzed_Texts[Case_ID - 2].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.4.3.2.1";
                        }
                        else
                        {
                            if (Analyzed_Texts[Case_ID - 2].def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID - 2].ecase = "N";
                                Analyzed_Texts[Case_ID - 2].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.4.3.2.2";
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.4.4 if the noun is found after the kAn/PV and its sisters
            N_List = Analyzed_Texts.Where(a => (a.lemmaID.Equals("kAn-u")) || a.lemmaID.Equals(">aSobaH") || a.lemmaID.Equals("layosa")).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && (Analyzed_Texts[Case_ID + 1].stem.Contains("/PV") || Analyzed_Texts[Case_ID + 1].stem.Contains("/IV")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")))
                        {
                            if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (!Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.1.1.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.1.1.2";
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID + 1].stem.Contains("/ADJ") && !Analyzed_Texts[Case_ID + 2].stem.StartsWith("bayon/") && !Analyzed_Texts[Case_ID + 2].stem.Contains("/PREP") && !Analyzed_Texts[Case_ID].pr.Contains("PREP") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")))
                        {
                            if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (!Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.1.2.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.1.2.2";
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID + 2].stem.EndsWith("/NOUN") && !Analyzed_Texts[Case_ID + 2].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 2].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID + 2].def.Equals("INDEF")) && (Analyzed_Texts[Case_ID + 2].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID + 2].spattern.Equals(">afaAEiyl") ||
                            Analyzed_Texts[Case_ID + 2].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID + 2].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID + 2].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID + 2].spattern.Equals("faEaAlil") ||
                            Analyzed_Texts[Case_ID + 2].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID + 2].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID + 2].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID + 2].spattern.Equals("tafaAEil") ||
                            Analyzed_Texts[Case_ID + 2].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID + 2].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID + 2].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID + 2].spattern.Equals("fayaAEil") ||
                            Analyzed_Texts[Case_ID + 2].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID + 2].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID + 2].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID + 2].spattern.Equals("fuEalaA'") ||
                            Analyzed_Texts[Case_ID + 2].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID + 2].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID + 2].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID + 2].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID + 2].stem.Contains("~/"))) && Analyzed_Texts[Case_ID + 2].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID + 2].ecase = "a";
                            Analyzed_Texts[Case_ID + 2].affectedBy = Analyzed_Texts[Case_ID + 2].affectedBy + "_C:3.4.4.2.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "F";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.2.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].def.Equals("DEF") && Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.5";
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("/PREP") && Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID + 1].def.Equals("INDEF"))
                        {
                            if ((Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEiyl") ||
                                 Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAlil") ||
                                 Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEil") ||
                                 Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fayaAEil") ||
                                 Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fuEalaA'") ||
                                 Analyzed_Texts[Case_ID + 1].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID + 1].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID + 1].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID + 1].stem.Contains("~/"))) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "u";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.4.3.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.2";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            if (((Analyzed_Texts[Case_ID + 1].stem.Contains("y/") && !Analyzed_Texts[Case_ID + 1].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID + 1].stem.Contains("w/") && !Analyzed_Texts[Case_ID + 1].stem.Contains("ow/"))))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "e";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.4.3.3";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "u";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.4.3.4";
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].stem.EndsWith("/PREP") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].def.Equals("INDEF"))
                        {
                            if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") ||
                                 Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") ||
                                 Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") ||
                                 Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") ||
                                 Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") ||
                                 Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID].stem.Contains("~/"))) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.2";
                            }
                        }
                        else if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.3";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.4";
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 1].pr.Contains("PREP"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "F";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.3";
                        }
                        else if (Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH") || Analyzed_Texts[Case_ID + 1].def.Equals("INDEF"))
                        {
                            if (Analyzed_Texts[Case_ID].def.Equals("DEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))))
                                {
                                    Analyzed_Texts[Case_ID].ecase = "e";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.4";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID].ecase = "u";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.3.5";
                                }
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.4.5 if the prefix begin with fa/CONJ or la/PART
            N_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN") && (a.pr.Contains("fa/CONJ") || a.pr.Contains("la/PART")))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")))
                    {
                        if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && !Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.5.1.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.5.1.2";
                        }
                    }
                    else
                    {
                        if (Analyzed_Texts[Case_ID].def.Equals("INDEF"))
                        {
                            if (((Analyzed_Texts[Case_ID].stem.Contains("y/") && Analyzed_Texts[Case_ID].stem.Contains("oy/")) || (!Analyzed_Texts[Case_ID].stem.Contains("w/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))))
                            {
                                Analyzed_Texts[Case_ID].ecase = "e";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.1.2.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.4.1.2.2";
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.4.6 Alkhabar
            N_List = Analyzed_Texts.Where(a => (a.stem.Contains("<s>"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Case_ID < Analyzed_Texts.Count() - 1 && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                {
                    if (Case_ID < Analyzed_Texts.Count() && Analyzed_Texts[Case_ID + 1].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                    {
                        if (Case_ID < Analyzed_Texts.Count() && ((Analyzed_Texts[Case_ID + 1].stem.Contains("y/") && !Analyzed_Texts[Case_ID + 1].stem.Contains("oy/")) || (Analyzed_Texts[Case_ID + 1].stem.Contains("w/") && !Analyzed_Texts[Case_ID + 1].stem.Contains("ow/"))) && Analyzed_Texts[Case_ID + 1].suf.Equals("") && (Analyzed_Texts[Case_ID].def.Equals("DEF")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "e";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.4.6.1";
                        }
                        else if (Case_ID < Analyzed_Texts.Count() && (Analyzed_Texts[Case_ID + 1].stem.Equals("*At/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("<ivor/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals(">amAm/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals(">avonA'/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("baEod/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("bayon/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("Dimon/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("duwn/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("EAm/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("Eaqib/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("Einod/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("fajor/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("fawoq/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("fawor/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("Hasob/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("Hawol/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("laHoZ/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("maE/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("naHow/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("qabol/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("qubayol/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("taHot/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("warA'/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("wasaT/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("xArij/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("Hiyn/NOUN") || Analyzed_Texts[Case_ID + 1].stem.Equals("xilAl/NOUN") ||
                                Analyzed_Texts[Case_ID + 1].stem.Equals("yawom/NOUN")) && Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "a";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.6.2";
                        }
                        else if (Case_ID < Analyzed_Texts.Count() && (Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEal") ||
                 Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAliyl") ||
                 Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEiyl") ||
                 Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEilaA'") ||
                 Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("lafoEaA'") ||
                 (Analyzed_Texts[Case_ID + 1].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID + 1].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID + 1].spattern, ".(awA).(i)."))) || Analyzed_Texts[Case_ID + 1].stem.StartsWith("jahan~am") ||
                 (Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID + 1].stem.Contains("'/")) && !Analyzed_Texts[Case_ID + 1].stem.StartsWith(">arobaE/") && Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 1].ecase.Equals("")))
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "u";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.6.3";
                        }
                        else if (Case_ID < Analyzed_Texts.Count() && (Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && !Analyzed_Texts[Case_ID + 1].spattern.StartsWith("kam/") && !Analyzed_Texts[Case_ID + 1].stem.StartsWith("Tay~ib/")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            if (Case_ID < Analyzed_Texts.Count() && (((Analyzed_Texts[Case_ID + 1].stem.Equals("fajo>") || Analyzed_Texts[Case_ID + 1].stem.StartsWith("mar~/") || Analyzed_Texts[Case_ID + 1].suf.StartsWith("natiyj/")) && Analyzed_Texts[Case_ID + 1].suf.Equals("ap/N_SUF")) || Analyzed_Texts[Case_ID + 1].stem.StartsWith("<iymA'/")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "F";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.7.1";
                            }
                            else if (Case_ID < Analyzed_Texts.Count() && Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID].stem.Contains("NOUN") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "N";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.7.2";
                            }
                        }
                        else if (Case_ID < Analyzed_Texts.Count() && (Analyzed_Texts[Case_ID + 1].def.Equals("DEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].stem.Contains("NOUN") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID + 1].ecase = "u";
                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.4.8";

                        }
                    }
                }
            });
            #endregion
            #endregion

            #region 3.5 set NOUNs as Accusitive
            #region 3.5.1 if the noun is found after <in~a/CONJ and its sisters
            #region Suffix Equal Blank
            N_List = Analyzed_Texts.Where(a => ((a.stem.Equals("<in~a/CONJ") || a.stem.Equals(">an~a/CONJ") || a.stem.Equals("li>an~a/CONJ") || a.stem.Equals("ka>an~a/CONJ") || a.stem.Equals("layota/CONJ") || a.stem.Equals("laEal~a/CONJ") || a.stem.Equals("l`kin~a/CONJ")) && a.suf.Equals(""))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    #region In case of Albadal
                    if ((Analyzed_Texts[Case_ID].stem.Equals("h`*A/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*ihi/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`&ulA'i/PRON") || Analyzed_Texts[Case_ID].stem.Equals("h`*Ani/PRON") ||
                        Analyzed_Texts[Case_ID].stem.Equals("h`*ayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("hAtayoni/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*`lika/PRON") || Analyzed_Texts[Case_ID].stem.Equals("*Aka/PRON") ||
                        Analyzed_Texts[Case_ID].stem.Equals("h`*iy/PRON") || Analyzed_Texts[Case_ID].stem.Equals("tiloka/PRON") || Analyzed_Texts[Case_ID].stem.Equals(">uwla}ika/PRON")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("NOUN") && (Analyzed_Texts[Case_ID + 2].stem.Contains("/IV") || Analyzed_Texts[Case_ID + 2].stem.Contains("/PV")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID + 1].suf.Equals("At/N_SUF"))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "i";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.1.1.1";
                            }
                            else
                            {
                                if (Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID + 1].ecase = "a";
                                    Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.1.1.2";
                                }
                            }
                        }
                        else
                        {
                            if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("DEF") && (Analyzed_Texts[Case_ID + 2].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 2].def.Equals("INDEF")) && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                            {
                                if (Analyzed_Texts[Case_ID + 1].suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID + 1].ecase = "i";
                                    Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.1.2.1";
                                }
                                else
                                {
                                    if (Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID + 1].ecase = "a";
                                        Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.1.2.2";
                                    }
                                }
                            }
                            else
                            {
                                if (Analyzed_Texts[Case_ID + 2].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("DEF"))
                                {
                                    if (Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                                    {

                                        if ((Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEal") ||
                                               Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAliyl") ||
                                               Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEiyl") ||
                                               Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEilaA'") ||
                                               Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("lafoEaA'") ||
                                               (Analyzed_Texts[Case_ID + 1].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID + 1].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID + 1].spattern, ".(awA).(i)."))) ||
                                               (Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID + 1].stem.Contains("'/")) && !Analyzed_Texts[Case_ID + 1].stem.StartsWith(">arobaE/")))
                                        {
                                            Analyzed_Texts[Case_ID + 1].ecase = "u";
                                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.2.3.1.1";
                                        }
                                        else
                                        {
                                            Analyzed_Texts[Case_ID + 1].ecase = "N";
                                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.2.3.1.2";

                                        }
                                    }
                                    else
                                    {
                                        if (Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH"))
                                        {
                                            Analyzed_Texts[Case_ID + 1].ecase = "u";
                                            Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.2.3.2";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Other cases
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && (Analyzed_Texts[Case_ID + 1].stem.Contains("/IV") || Analyzed_Texts[Case_ID + 1].stem.Contains("/PV")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "i";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.1";
                        }
                        else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].def.Equals("DEF") && (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID + 1].def.Equals("INDEF")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "i";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.2.1";
                        }
                        else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.2.2";
                        }
                        if (Analyzed_Texts[Case_ID + 1].ecase.Equals(""))
                        {
                            if ((Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEal") ||
                                Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("faEaAliyl") ||
                                Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("tafaAEiyl") ||
                                Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID + 1].spattern.Equals(">afoEilaA'") ||
                                Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID + 1].spattern.Equals("lafoEaA'") ||
                                (Analyzed_Texts[Case_ID + 1].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID + 1].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID + 1].spattern, ".(awA).(i)."))) ||
                                (Analyzed_Texts[Case_ID + 1].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID + 1].stem.Contains("'/")) && !Analyzed_Texts[Case_ID + 1].stem.StartsWith(">arobaE/")))
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "u";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.2.2.3.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID + 1].ecase = "N";
                                Analyzed_Texts[Case_ID + 1].affectedBy = Analyzed_Texts[Case_ID + 1].affectedBy + "_C:3.5.2.2.3.2";

                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID].def.Equals("DEF"))
                    {
                        if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {

                            if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                                   Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                                   Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                                   Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                                   Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                                   (Analyzed_Texts[Case_ID].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(awA).(i)."))) ||
                                   (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) && !Analyzed_Texts[Case_ID].stem.StartsWith(">arobaE/")))
                            {
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.3.1.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID].ecase = "N";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.3.1.2";

                            }
                        }
                        else if (Analyzed_Texts[Case_ID].def.Equals("EDAFAH"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.3.2";
                        }
                        else if (Analyzed_Texts[Case_ID].def.Equals("DEF") && (Analyzed_Texts[Case_ID + 1].def.Equals("INDEF") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.3.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/ADJ") || Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN_PROP")) && Analyzed_Texts[Case_ID].def.Equals("DEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].stem.Contains("y/") || Analyzed_Texts[Case_ID].stem.Contains("w/")) && !Analyzed_Texts[Case_ID].stem.Contains("oy/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.5.1";
                        }
                        else
                        {
                            if (Analyzed_Texts[Case_ID].suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "i";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.5.2.1";
                            }
                            else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID].ecase = "a";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.5.2.2";
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].def.Equals("DEF") && (Analyzed_Texts[Case_ID + 1].stem.Equals("Al~a*iy/PRON") || Analyzed_Texts[Case_ID + 1].stem.Equals("Al~atiy/PRON") || Analyzed_Texts[Case_ID + 1].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.4";
                    }
                }
                #endregion
            });
            #endregion

            #region Suffix Does Not Equal Blank
            N_List = Analyzed_Texts.Where(a => ((a.stem.Equals("<in~a/CONJ") || a.stem.Equals(">an~a/CONJ") || a.stem.Equals("ka>an~a/CONJ") || a.stem.Equals("layota/CONJ") || a.stem.Equals("laEal~a/CONJ")))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                         Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                         Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                         Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                         Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                         (Analyzed_Texts[Case_ID].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(awA).(i)."))) ||
                         (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) && !Analyzed_Texts[Case_ID].stem.StartsWith(">arobaE/")) && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "N";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1.2.1";
                    }

                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "N";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1.2.2";
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/ADJ") || Analyzed_Texts[Case_ID + 1].stem.EndsWith("/NOUN_PROP")) && Analyzed_Texts[Case_ID].def.Equals("DEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].stem.Contains("y/") || Analyzed_Texts[Case_ID].stem.Contains("w/")) && !Analyzed_Texts[Case_ID].stem.Contains("oy/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1.2.4.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "u";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1.2.4.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].suf.Contains("At/N_SUF"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "i";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1.2.3.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.1.2.3.2";
                        }
                    }
                }
            });
            #endregion
            #endregion

            #region 3.5.2 if the noun is found afetr numbers
            Regex regex = new Regex(@"^\d+$");
            N_List = Analyzed_Texts.Where(a => (regex.IsMatch(a.word))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.EndsWith("NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID - 1].word.Length.Equals(1) || Analyzed_Texts[Case_ID - 1].word.Equals(10))
                        {
                            if (Regex.IsMatch(Analyzed_Texts[Case_ID - 1].word, "^[0-9]$") || Analyzed_Texts[Case_ID - 1].word.Equals(10))
                            {
                                if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {

                                    if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                                           (Analyzed_Texts[Case_ID].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(awA).(i)."))) || Analayzed_List[Case_ID].lemmaID.Equals("miloyuwn") ||
                                           (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) && !Analyzed_Texts[Case_ID].stem.StartsWith(">arobaE/")))
                                    {
                                        Analyzed_Texts[Case_ID].ecase = "a";
                                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.1.1";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID].ecase = "K";
                                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.1.2";
                                    }
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID].ecase = "i";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.2";
                                }
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 1].word.Length > 1 && !Analyzed_Texts[Case_ID - 1].word.Equals("10"))
                        {
                            string last_number_value = Analyzed_Texts[Case_ID - 1].word.Substring(Analyzed_Texts[Case_ID - 1].word.Length - 2);
                            if ((Regex.IsMatch(last_number_value, @"^\d[11-99]$")))
                            {
                                if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {

                                    Analyzed_Texts[Case_ID].ecase = "F";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.2.2";
                                }
                                else if (Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID].ecase = "a";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.2.3";
                                }
                            }
                            else if (last_number_value.Equals("10") || last_number_value.Equals("00"))
                            {
                                if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {

                                    if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") ||
                                           Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") ||
                                           (Analyzed_Texts[Case_ID].spattern.Equals("") && (Regex.IsMatch(Analyzed_Texts[Case_ID].stem, ".(awA).(iy).") || Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(awA).(i)."))) || Analayzed_List[Case_ID].lemmaID.Equals("miloyuwn") ||
                                           (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) && !Analyzed_Texts[Case_ID].stem.StartsWith(">arobaE/")))
                                    {
                                        Analyzed_Texts[Case_ID].ecase = "a";
                                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.1.1";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID].ecase = "K";
                                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.1.2";
                                    }
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID].ecase = "i";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.5.2.1.2";
                                }
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.5.3 if the noun is found after 'lA' Aln~Afiyap liljinos
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals("lA/PART"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && !Analyzed_Texts[Case_ID].pr.Contains("Al/DET") && !Analyzed_Texts[Case_ID].suf.Contains("POSS") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID + 1].stem.EndsWith("/PREP") || Analyzed_Texts[Case_ID + 1].stem.Contains("</s>") || Analyzed_Texts[Case_ID + 1].stem.Equals(">an/CONJ") || Analyzed_Texts[Case_ID + 1].stem.Equals(">an~a/CONJ") || Analyzed_Texts[Case_ID + 1].stem.Contains("e"))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.5.3";
                    }
                }
            });
            #endregion

            #region 3.5.4 if the noun is found before numbers EAm, yawom, sanap, sAEap
            N_List = Analyzed_Texts.Where(a => (Regex.IsMatch(a.word, @"^\d+$"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Case_ID > 2 && (Analyzed_Texts[Case_ID - 2].stem.StartsWith("EAm/") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("yawom/") || ((Analyzed_Texts[Case_ID - 2].stem.StartsWith("sAE/NOUN") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("san/NOUN") || Analyzed_Texts[Case_ID - 2].stem.StartsWith("gadA/NOUN")) && Analyzed_Texts[Case_ID - 2].suf.Equals("ap/N_SUF")))) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 2].ecase = "a";
                        Analyzed_Texts[Case_ID - 2].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.5.4.1";
                    }
                }
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Case_ID > 3 && (Analyzed_Texts[Case_ID - 3].stem.StartsWith("EAm/") || Analyzed_Texts[Case_ID - 3].stem.StartsWith("yawom/") || ((Analyzed_Texts[Case_ID - 3].stem.StartsWith("sAE/NOUN") || Analyzed_Texts[Case_ID - 3].stem.StartsWith("san/NOUN") || Analyzed_Texts[Case_ID - 3].stem.StartsWith("gadA/NOUN")) && Analyzed_Texts[Case_ID - 3].suf.Equals("ap/N_SUF")))) && Analyzed_Texts[Case_ID].ecase.Equals("") && Analyzed_Texts[Case_ID - 2].stem.Equals("e"))
                    {
                        Analyzed_Texts[Case_ID - 3].ecase = "a";
                        Analyzed_Texts[Case_ID - 3].affectedBy = Analyzed_Texts[Case_ID - 3].affectedBy + "_C:3.5.4.2";
                    }
                }
            });
            #endregion

            #region 3.5.5 qayora >an~a, bayoda >an~a
            N_List = Analyzed_Texts.Where(a => ((a.stem.Contains("gayor/") || a.stem.Contains("bayod/") || (a.stem.Equals("bugoy/NOUN") && a.suf.Equals("ap/N_SUF"))) && a.ecase.Equals(""))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {

                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.5.5";
                }
            });
            #endregion

            #region 3.5.6 bugoyapa
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals("bugoy/NOUN") && a.suf.Equals("ap/N_SUF") && a.ecase.Equals(""))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (u.ecase.Equals(""))
                {

                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 2].affectedBy + "_C:3.5.6";
                }
            });
            #endregion            
            #endregion

            #region 3.6 yA  && >ay~uhA munAdaY
            N_List = Analyzed_Texts.Where(a => (a.stem.Equals("yA/PART"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                if (u.ecase.Equals(""))
                {
                    Case_ID = int.Parse(u.ID);
                    #region 6.1 if yA is occured before NOUN_PROP
                    if (Analyzed_Texts[Case_ID].stem.Contains("/NOUN_PROP"))
                    {
                        if ((Analyzed_Texts[Case_ID].stem.Contains("Y/") || Analyzed_Texts[Case_ID].stem.Contains("A/") || Analyzed_Texts[Case_ID].stem.Contains("w/") || Analyzed_Texts[Case_ID].stem.Contains("y/")) && Analyzed_Texts[Case_ID].ecase.Equals("") && !Analyzed_Texts[Case_ID].stem.Contains("Eamorw/"))
                        {
                            Case_ID = int.Parse(u.ID);
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.6.1.1";
                        }
                        else
                        {
                            if (Analyzed_Texts[Case_ID].stem.Contains("Eamorw/") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Case_ID = int.Parse(u.ID);
                                Analyzed_Texts[Case_ID].ecase = "u";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.6.1.2";
                            }
                            else
                            {
                                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {
                                    Case_ID = int.Parse(u.ID);
                                    Analyzed_Texts[Case_ID].ecase = "u";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.6.1.3";
                                }
                            }
                        }
                    }
                    #endregion

                    #region 6.2 if yA is occured before NOUN
                    else
                    {
                        if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN"))
                        {
                            if (Analyzed_Texts[Case_ID].def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Case_ID = int.Parse(u.ID);
                                Analyzed_Texts[Case_ID].ecase = "a";
                                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.6.2.1";
                            }
                            else
                            {
                                if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                                {
                                    Case_ID = int.Parse(u.ID);
                                    Analyzed_Texts[Case_ID].ecase = "u";
                                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.6.2.2";
                                }
                            }
                        }
                    }
                    #endregion
                }
            });

            N_List = Analyzed_Texts.Where(a => (a.stem.Equals(">ay~uhA/PART"))).ToList();
            N_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                #region 6.1 if yA is occured before NOUN_PROP
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    Case_ID = int.Parse(u.ID);
                    Analyzed_Texts[Case_ID].ecase = "u";
                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.6.2";
                }

                #endregion
            });
            #endregion

            #region 3.7 set Case for NOUNs After Verbs
            var Afetr_Verbs_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN") && a.ecase.Equals(""))).ToList();
            Afetr_Verbs_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (u.ecase.Equals(""))
                {
                    Case_ID = int.Parse(u.ID);
                    #region Command Verbs
                    if (Analyzed_Texts[Case_ID - 2].stem.Contains("/CV"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.1";
                    }
                    #endregion

                    #region Imperfect & Past Verbs
                    #region Active
                    #region PV or IV + wa/CONJ+Al/DET
                    else if (Analyzed_Texts[Case_ID - 2].stem.EndsWith("/IV") || Analyzed_Texts[Case_ID - 2].stem.EndsWith("/PV"))
                    {
                        if (u.pr.Equals("wa/CONJ+Al/DET") && u.ecase.Equals(""))
                        {
                            if (u.stem.Contains("iy/"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "e";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.1.1";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.1.2";
                            }
                        }
                        #endregion

                        if ((Analyzed_Texts[Case_ID - 2].stem.EndsWith("/IV") || Analyzed_Texts[Case_ID - 2].stem.EndsWith("/PV")) && u.ecase.Equals(""))
                        {
                            var Trans_List = obj.Lemma_Trans.Trans.Select(a => new { a.LemmaID, a.Tra }).Where(a => (a.LemmaID.Equals(Analyzed_Texts[Case_ID - 2].lemmaID))).Distinct().ToList();

                            #region To Select The Most Suitable Transitivity
                            if (Trans_List.Count > 1)
                            {
                                Trans_Out = Trans_List[0].Tra;
                            }
                            if (Trans_List.Count.Equals(1))
                            {
                                Trans_Out = Trans_List[0].Tra;
                            }
                            #endregion

                            #region in case of the subject is attached with the verb
                            #region Imperfect Verbs
                            if (Analyzed_Texts[Case_ID - 2].stem.EndsWith("/IV") && Analyzed_Texts[Case_ID - 2].suf.Contains("+") && u.ecase.Equals(""))
                            {
                                if (Trans_List.Count() > 0 && Trans_Out.Equals("TST2"))
                                {
                                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                    u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                    u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                    u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                    u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                    (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                    (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.1";
                                    }
                                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "F";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.2";
                                    }
                                    else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.3";
                                    }
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                    u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                    u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                    u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                    u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                    (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                    (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.4";
                                    }
                                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        if (u.stem.Equals("mar~/NOUN") && u.suf.Equals("ap/N_SUF"))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.5.1";
                                        }
                                        else
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "N";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.5.2";
                                        }
                                    }
                                    else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.2.6";
                                    }
                                }
                            }

                            #endregion

                            #region Past Verbs
                            else if (Analyzed_Texts[Case_ID - 2].stem.EndsWith("/PV") && Analyzed_Texts[Case_ID - 2].suf.Contains("+") && u.ecase.Equals(""))
                            {
                                if (u.stem.Contains("iy/") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.1";
                                }
                                else if (Trans_List.Count() > 0 && (Trans_Out.Equals("TST2") || Trans_Out.Equals("TSTD")))
                                {
                                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                    u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                    u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                    u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                    u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                    (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                    (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.2";
                                    }
                                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "N";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.3";
                                    }
                                    else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.4";
                                    }
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                    u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                    u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                    u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                    u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                    (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                    (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.5";
                                    }
                                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "N";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.6";
                                    }
                                    else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.3.7";
                                    }
                                }
                            }
                            #endregion
                            #endregion

                            #region nafoEal
                            if (Analyzed_Texts[Case_ID - 2].pr.Contains("na/") && u.ecase.Equals(""))
                            {
                                if (u.def.Equals("DEF") || u.def.Equals("EDAFAH"))
                                {
                                    if (u.suf.Contains("At/"))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.30";
                                    }
                                    else if (u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.2";
                                    }
                                }
                                else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                {
                                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/"))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.3";
                                    }
                                    else if (u.suf.Contains("At/") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "K";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.4";
                                    }
                                    else if (u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "F";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.5";
                                    }
                                }
                            }
                            #endregion

                            #region NERG
                            if (Trans_List.Count() > 0 && Trans_Out.Equals("NERG") && Analyzed_Texts[Case_ID - 2].suf.Contains("tu/") && u.ecase.Equals(""))
                            {
                                if (u.def.Equals("DEF") || u.def.Equals("EDAFAH") && u.ecase.Equals(""))
                                {
                                    if (u.suf.Contains("At/N_SUF"))
                                    {
                                        Case_ID = int.Parse(u.ID);
                                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.4.1";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.4.2";
                                    }
                                }
                                else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                {
                                    if (u.suf.Contains("At/N_SUF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "K";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.2.4.3";

                                    }
                                    else if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") ||
                                       u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") ||
                                       u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                       u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals("fuEolaY") || u.spattern.Equals("fiEolaY") ||
                                       u.spattern.Equals("faEolaY") || u.spattern.Equals("faEaAlaY") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") ||
                                       u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl'") && u.stem.Contains("'/"))) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.4.4";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "F";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.4.5";
                                    }
                                }
                            }
                            #endregion

                            #region TSTI, NACC, NERG
                            else if (Trans_List.Count() > 0 && (Trans_Out.Equals("TSTI") || Trans_Out.Equals("NACC") || Trans_Out.Equals("NERG")) && !u.lemmaID.Equals("masA'"))
                            {
                                if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.5.1";
                                }
                                else if ((u.def.Equals("INDEF")) && u.ecase.Equals(""))
                                {
                                    if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") ||
                                        u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") ||
                                        u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                        u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals("fuEolaY") || u.spattern.Equals("fiEolaY") ||
                                        u.spattern.Equals("faEolaY") || u.spattern.Equals("faEaAlaY") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") ||
                                        u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl'") && u.stem.Contains("'/"))) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.5.2";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "N";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.2.5.3";
                                    }
                                }
                            }
                            #endregion

                            #region TSTD & TST2 with no attached subject
                            else if (Trans_List.Count() > 0 && (Trans_Out.Equals("TST2") || Trans_Out.Equals("AUX") || Trans_Out.Equals("TSTD")) && u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID].suf.Contains("/POSS_SUF") && u.ecase.Equals(""))
                            {
                                if (u.def.Equals("DEF") || u.def.Equals("EDAFAH"))
                                {
                                    if ((u.suf.Contains("y/") && !u.suf.Contains("oy/")) || (u.suf.Contains("w/") && !u.suf.Contains("ow/")))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.31";
                                    }
                                    else if (u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.2";
                                    }
                                }
                                else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                {
                                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/"))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.3";
                                    }
                                    else if (u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "N";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.9.5";
                                    }
                                }
                            }
                            #endregion
                        }
                    }

                    #region in case of apposition
                    else if (Analyzed_Texts[Case_ID - 3].stem.EndsWith("/IV") || Analyzed_Texts[Case_ID - 3].stem.EndsWith("/PV") && u.ecase.Equals(""))
                    {
                        try
                        {
                            var Trans_List = obj.Lemma_Trans.Trans.Select(a => new { a.LemmaID, a.Tra }).Where(a => (a.LemmaID.Equals(Analyzed_Texts[Case_ID - 3].lemmaID))).Distinct().ToList();
                            Trans_Out = Trans_List[0].Tra;
                            if (Trans_List.Count() > 0 && (Trans_Out.Equals("TSTI") || Trans_Out.Equals("NACC") || Trans_Out.Equals("NERG")) && !u.lemmaID.Equals("masA'"))
                            {
                                if ((Analyzed_Texts[Case_ID - 2].stem.Equals("h`*A/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("h`*ihi/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("h`&ulA'i/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("h`*Ani/PRON") ||
                                Analyzed_Texts[Case_ID - 2].stem.Equals("h`*ayoni/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("hAtayoni/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("*`lika/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("*Aka/PRON") ||
                                Analyzed_Texts[Case_ID - 2].stem.Equals("h`*iy/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals("tiloka/PRON") || Analyzed_Texts[Case_ID - 2].stem.Equals(">uwla}ika/PRON")) && u.pr.Equals("Al/DET") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3";
                                }
                                else if (((Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") && Analyzed_Texts[Case_ID - 2].suf.Contains("/POSS") && Analyzed_Texts[Case_ID - 2].stem.Contains("")) || (Analyzed_Texts[Case_ID - 2].stem.Contains("/PREP") && Analyzed_Texts[Case_ID - 2].suf.Contains("/POSS"))) && u.stem.EndsWith("/NOUN") && u.ecase.Contains(""))
                                {
                                    if (u.def.Equals("DEF") || u.def.Equals("EDAFAH"))
                                    {
                                        if ((u.stem.Contains("y/") && !u.stem.Contains("oy/")) || (u.stem.Contains("w/") && !u.stem.Contains("ow/")))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "e";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.1";
                                        }
                                        else if (u.ecase.Equals(""))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "u";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.2";
                                        }
                                    }
                                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                    u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                    u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                    u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                    u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                    (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                    (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/"))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "u";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.3";
                                        }
                                        else if (u.ecase.Equals(""))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "N";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.4";
                                        }
                                    }
                                }
                            }
                            else if (Trans_List.Count() > 0 && (Trans_Out.Equals("TST2") || Trans_Out.Equals("TSTD")))
                            {
                                if (((Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") && Analyzed_Texts[Case_ID - 2].suf.Contains("/POSS") && Analyzed_Texts[Case_ID - 2].stem.Contains("")) || (Analyzed_Texts[Case_ID - 2].stem.Contains("/PREP") && Analyzed_Texts[Case_ID - 2].suf.Contains("/POSS"))) && u.stem.EndsWith("/NOUN") && u.ecase.Contains(""))
                                {
                                    if (u.def.Equals("DEF") || u.def.Equals("EDAFAH"))
                                    {
                                        if (u.suf.Contains("At/"))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.6.1";
                                        }
                                        else if (u.ecase.Equals(""))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.6.2";
                                        }
                                    }
                                    else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                                    {
                                        if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                    u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                    u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                    u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                    u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                    (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                    (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/"))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.7.1";
                                        }
                                        else if (u.suf.Contains("At/") && u.ecase.Equals(""))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.7.2";
                                        }
                                        else if (u.ecase.Equals(""))
                                        {
                                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.7.3";
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    #endregion

                    else if ( Case_ID > 4 && ((Analyzed_Texts[Case_ID - 4].stem.EndsWith("/IV") || Analyzed_Texts[Case_ID - 4].stem.EndsWith("/PV")) && Analyzed_Texts[Case_ID - 3].stem.EndsWith("/PREP") && Analyzed_Texts[Case_ID - 2].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID - 2].def.Equals("DEF") && u.ecase.Equals("")))
                    {
                        var Trans_List = obj.Lemma_Trans.Trans.Select(a => new { a.LemmaID, a.Tra }).Where(a => (a.LemmaID.Equals(Analyzed_Texts[Case_ID - 4].lemmaID))).Distinct().ToList();
                        if (Trans_List[0].Tra.Equals("TSTI") || Trans_List[0].Tra.Equals("NACC"))
                        {
                            if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                                        u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                                        u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                                        u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                                        u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                                        (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                                        (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.5.1";
                            }
                            else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                if (u.stem.Equals("mar~/NOUN") && u.suf.Equals("ap/N_SUF"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.5.2.1";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.5.2.2";
                                }
                            }
                            else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.5.3";
                            }
                        }
                    }
                    #endregion

                    #region Passive
                    else if ((Analyzed_Texts[Case_ID - 2].stem.EndsWith("/IV_PASS") || Analyzed_Texts[Case_ID - 2].stem.EndsWith("/PV_PASS")) && u.ecase.Equals(""))
                    {
                        var Trans_List = obj.Lemma_Trans.Trans.Select(a => new { a.LemmaID, a.Tra }).Where(a => (a.LemmaID.Equals(Analyzed_Texts[Case_ID - 2].lemmaID))).Distinct().ToList();

                        #region To Select The Most Suitable Transitivity
                        if (Trans_List.Count > 1)
                        {
                            Trans_Out = Trans_List[0].Tra;
                        }
                        if (Trans_List.Count.Equals(1))
                        {
                            Trans_Out = Trans_List[0].Tra;
                        }
                        #endregion

                        #region TST2
                        if (Trans_List.Count() > 0 && Trans_Out.Equals("TST2") && u.ecase.Equals(""))
                        {
                            if (u.def.Equals("DEF") && Analyzed_Texts[Case_ID].def.Equals("INDEF"))
                            {
                                if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.1";
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.2";
                                }
                            }
                            else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") ||
                                    u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") ||
                                    u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                    u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals("fuEolaY") || u.spattern.Equals("fiEolaY") ||
                                    u.spattern.Equals("faEolaY") || u.spattern.Equals("faEaAlaY") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") ||
                                    u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl'") && u.stem.Contains("'/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.3";
                                }
                                else if (u.suf.Contains("At/N_SUF") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "K";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.4";
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.5";
                                }
                            }
                            else if (u.def.Equals("EDAFAH") && u.ecase.Equals(""))
                            {
                                if (u.suf.Contains("At/N_SUF") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.6";

                                }
                                else if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.7";
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.8";
                                }
                            }
                        }
                        #endregion

                        #region Other Cases
                        else
                        {
                            if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") ||
                                    u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") ||
                                    u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                    u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals("fuEolaY") || u.spattern.Equals("fiEolaY") ||
                                    u.spattern.Equals("faEolaY") || u.spattern.Equals("faEaAlaY") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") ||
                                    u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl'") && u.stem.Contains("'/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.9";
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.3.10";
                                }
                            }
                            else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                            {
                                if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.11";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.12";
                                }
                            }
                        }
                        #endregion
                    }

                    #region In case of apposition
                    else if (Case_ID > 4 && ((Analyzed_Texts[Case_ID - 4].stem.EndsWith("/IV_PASS") || Analyzed_Texts[Case_ID - 4].stem.EndsWith("/PV_PASS")) && (Analyzed_Texts[Case_ID - 3].stem.EndsWith("/NOUN_PROP") && Analyzed_Texts[Case_ID - 2].stem.EndsWith("/NOUN_PROP")) && u.ecase.Equals("")))
                    {
                        var Trans_List = obj.Lemma_Trans.Trans.Select(a => new { a.LemmaID, a.Tra }).Where(a => (a.LemmaID.Equals(Analyzed_Texts[Case_ID - 4].lemmaID))).Distinct().ToList();

                        #region To Select The Most Suitable Transitivity
                        if (Trans_List.Count > 1)
                        {
                            Trans_Out = Trans_List[0].Tra;
                        }
                        if (Trans_List.Count.Equals(1))
                        {
                            Trans_Out = Trans_List[0].Tra;
                        }
                        #endregion

                        #region TST2
                        if (Trans_List.Count() > 0 && Trans_Out.Equals("TST2") && u.ecase.Equals(""))
                        {
                            if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.1";
                            }
                            else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") ||
                                    u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") ||
                                    u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                    u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals("fuEolaY") || u.spattern.Equals("fiEolaY") ||
                                    u.spattern.Equals("faEolaY") || u.spattern.Equals("faEaAlaY") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") ||
                                    u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl'") && u.stem.Contains("'/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.2";
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.3";
                                }
                            }
                        }
                        #endregion

                        #region Other Cases
                        else
                        {
                            if (Analyzed_Texts[Case_ID - 4].stem.Equals("Ead~/IV_PASS"))
                            {
                                if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.11";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.12";
                                    }
                                }
                            }
                            else if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                            {
                                if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.13";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.14";
                                }
                            }
                        }
                        #endregion
                    }

                    else if ((Analyzed_Texts[Case_ID - 3].stem.EndsWith("/IV_PASS") || Analyzed_Texts[Case_ID - 3].stem.EndsWith("/PV_PASS")) && Analyzed_Texts[Case_ID - 2].stem.EndsWith("/NOUN_PROP") && u.ecase.Equals(""))
                    {
                        var Trans_List = obj.Lemma_Trans.Trans.Select(a => new { a.LemmaID, a.Tra }).Where(a => (a.LemmaID.Equals(Analyzed_Texts[Case_ID - 3].lemmaID))).Distinct().ToList();

                        #region To Select The Most Suitable Transitivity
                        if (Trans_List.Count > 1)
                        {
                            Trans_Out = Trans_List[0].Tra;
                        }
                        if (Trans_List.Count.Equals(1))
                        {
                            Trans_Out = Trans_List[0].Tra;
                        }
                        #endregion

                        #region TST2
                        if (Trans_List.Count() > 0 && Trans_Out.Equals("TST2") && u.ecase.Equals(""))
                        {
                            if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.1";
                            }
                            else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") ||
                                    u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") ||
                                    u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                    u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals("fuEolaY") || u.spattern.Equals("fiEolaY") ||
                                    u.spattern.Equals("faEolaY") || u.spattern.Equals("faEaAlaY") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") ||
                                    u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl'") && u.stem.Contains("'/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.4";
                                }
                                else if (u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "F";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.4.5";
                                }
                            }
                        }
                        #endregion

                        #region Other Cases
                        else
                        {
                            if (Analyzed_Texts[Case_ID - 4].stem.Equals("Ead~/IV_PASS"))
                            {
                                if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.15";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.16";
                                    }
                                }
                            }
                            else if (u.ecase.Equals(""))
                            {
                                if ((u.def.Equals("DEF") || u.def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    if ((u.stem.Contains("y/") || u.stem.Contains("w/")) && !u.stem.Contains("oy/") && !u.stem.Contains("ow/") && u.suf.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.17";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.7.7.3.18";
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                    #endregion
                    #endregion
                }
            });
            #endregion

            #region 3.8 Set Case for Nouns After '*uw'
            var NOUNs_Afetr_thuw = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN") && a.ecase.Equals(""))).ToList();
            NOUNs_Afetr_thuw.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID - 2].lemmaID.Equals("*uw") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID - 1].def.Equals("EDAFAH") || Analyzed_Texts[Case_ID - 1].def.Equals("DEF")) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Case_ID = int.Parse(u.ID);
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.8.1";
                        }
                        else if (Analyzed_Texts[Case_ID - 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            if ((Analyzed_Texts[Case_ID].def.Equals("INDEF")) && (Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID].stem.Contains("~/"))) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                            {
                                Case_ID = int.Parse(u.ID);
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.8.2.1";
                            }
                            else
                            {
                                Case_ID = int.Parse(u.ID);
                                Analyzed_Texts[Case_ID - 1].ecase = "K";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.8.2.2";
                            }
                        }
                    }
                }
            });
            #endregion

            #region 3.9 set case for Nouns After 'hal'
            var NOUNs_Afetr_hal = Analyzed_Texts.Where(a => (a.stem.Equals("hal/PART"))).ToList();
            NOUNs_Afetr_hal.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Analyzed_Texts[Case_ID].def.Equals("EDAFAH") || Analyzed_Texts[Case_ID].def.Equals("DEF")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Case_ID = int.Parse(u.ID);
                        Analyzed_Texts[Case_ID].ecase = "u";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.9.1";
                    }
                    else
                    {
                        if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Case_ID = int.Parse(u.ID);
                            Analyzed_Texts[Case_ID].ecase = "N";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.9.2";
                        }
                    }
                }
            });
            #endregion

            #region 3.10 set case for *At/NOUN_ADV(T/P)
            var NOUNs_Afetr_thAt = Analyzed_Texts.Where(a => (a.stem.Equals("*At/NOUN") && a.ecase.Equals(""))).ToList();
            NOUNs_Afetr_thAt.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Case_ID].stem.Equals("SabAH/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("masA'/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("yawom/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("EAm/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("layol/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("mar~/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("san/NOUN")) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.10.1";
                }
                else if ((Analyzed_Texts[Case_ID].stem.Equals("yamiyn/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("$imAl/NOUN")) && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.10.2";
                }
            });
            #endregion

            #region 3.11 set case for yawom/NOUN_ADV(T)
            Days_Years_ADV = Analyzed_Texts.Where(a => (a.stem.Equals("yawom/NOUN") && a.ecase.Equals(""))).ToList();
            Days_Years_ADV.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].pr.Equals("Al/DET"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.11";
                }
            });
            #endregion

            #region 3.13 set case for siwaY
            var siwaY = Analyzed_Texts.Where(a => (a.lemmaID.Equals("siwaY") && a.ecase.Equals(""))).ToList();
            siwaY.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if ((Analyzed_Texts[Case_ID].def.Equals("DEF") || Analyzed_Texts[Case_ID].def.Equals("EDAFAH")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].stem.Contains("y/") || Analyzed_Texts[Case_ID].stem.Contains("w/")) && !Analyzed_Texts[Case_ID].stem.Contains("oy/") && !Analyzed_Texts[Case_ID].stem.Contains("ow/"))
                        {
                            Analyzed_Texts[Case_ID].ecase = "e";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.13.1";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "i";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.13.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID].def.Equals("INDEF") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        if ((Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") ||
                            Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") ||
                            Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") ||
                            Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fuEolaY") || Analyzed_Texts[Case_ID].spattern.Equals("fiEolaY") ||
                            Analyzed_Texts[Case_ID].spattern.Equals("faEolaY") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlaY") || Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") ||
                            Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") || Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl'") && Analyzed_Texts[Case_ID].stem.Contains("'/"))) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.13.3";
                        }
                        else if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "K";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:3.13.4";
                        }
                    }
                }
            });
            #endregion

            #region 3.14 set case for mivola
            var mivola = Analyzed_Texts.Where(a => (a.stem.Equals("mivol/NOUN") && a.pr.Equals("") && a.suf.Equals("") && a.ecase.Equals(""))).ToList();
            mivola.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID - 1].ecase = "a";
                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.14";
            });
            #endregion

            #region 3.15 set case for SabAh - kul~
            Days_Years_ADV = Analyzed_Texts.Where(a => (a.stem.Equals("SabAH/NOUN") && a.suf.Equals("") && a.ecase.Equals(""))).ToList();
            Days_Years_ADV.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Equals("masA'/NOUN") || Analyzed_Texts[Case_ID].pr.Equals("Al/DET"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.15.1";
                }
            });

            Days_Years_ADV = Analyzed_Texts.Where(a => (a.stem.Equals("SabAH/NOUN") && !a.pr.Contains("/PREP") && a.suf.Equals("") && a.ecase.Equals(""))).ToList();
            Days_Years_ADV = Analyzed_Texts.Where(a => (a.stem.Equals("SabAH/NOUN") && a.suf.Equals("") && a.ecase.Equals(""))).ToList();
            Days_Years_ADV.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Equals("masA'/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("SabAH/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("waqot/NOUN") ||
                Analyzed_Texts[Case_ID].stem.Equals("EAm/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("yawom/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("daqiyq/NOUN") ||
                Analyzed_Texts[Case_ID].stem.Equals("sAE/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("vAniy/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("san/NOUN") || Analyzed_Texts[Case_ID].stem.Equals("mar~/NOUN"))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.15.2";
                }
            });
            #endregion

            #region 5. Coordination
            var Coordination = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN")  && a.ecase.Equals(""))).ToList();
            Coordination.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                #region wa/CONJ
                if (u.pr.Contains("wa/CONJ"))
                {
                    if (u.def.Equals("INDEF") && u.spattern.Equals(">afoEal"))
                    {
                        if (Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 2].ecase.Equals("N") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("Ani/N_SUF")) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "u";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.1";
                        }
                        else if (Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") || Analyzed_Texts[Case_ID - 2].ecase.Equals("a") || (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("ayoni/N_SUF") && (Analyzed_Texts[Case_ID - 2].pr.Contains("PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP")))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.2";
                        }
                        else if (Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") || Analyzed_Texts[Case_ID - 2].ecase.Equals("F") || (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("ayoni/N_SUF"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].def.Equals("DEF") && Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && u.ecase.Equals(""))
                    {
                        if (!Analyzed_Texts[Case_ID - 2].ecase.Equals("e") && u.ecase.Equals(""))
                        {
                            if (u.suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].suf.Equals("At/NSUF") && Analyzed_Texts[Case_ID - 2].ecase.Equals("i") && !Analyzed_Texts[Case_ID - 2].pr.Contains("PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("PREP") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.2";
                            }
                            else if ((Analyzed_Texts[Case_ID - 1].stem.Contains("y/") || Analyzed_Texts[Case_ID - 1].stem.Contains("w/")) && !Analyzed_Texts[Case_ID - 1].stem.Contains("oy/") && !Analyzed_Texts[Case_ID - 1].stem.Contains("ow/") && Analyzed_Texts[Case_ID - 1].ecase.Contains(""))
                            {
                                if (Analyzed_Texts[Case_ID - 2].ecase.Equals("a"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.1";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.2";
                                }
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.4.3";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("e") && u.ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 2].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("Ani/N_SUF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("ayoni/N_SUF"))
                            {
                                if ((Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.2";
                                }
                                else if (!(Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.suf.Equals("At/N_SUF") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.3";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.4";
                                }
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("F") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                    {
                        if (u.suf.Equals("At/N_SUF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.1";
                        }
                        else if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                            u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                            u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                            u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.2";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && (Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 2].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("EDAFAH"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.4.1";
                        }
                        else if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.4.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("i") && (Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 2].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("EDAFAH") || u.def.Equals("DEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.5.1";
                        }
                        else if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.5.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") && (Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 2].def.Equals("INDEF")) && u.ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID - 3].stem.Contains("<s>") && Analyzed_Texts[Case_ID - 2].spattern.Equals("mafaAE") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 1].def.Equals("DEF") || Analyzed_Texts[Case_ID - 1].def.Equals("EDAFAH"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.1";
                            }
                            else if (u.def.Equals("INDEF"))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                                u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                                u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.3";
                                }
                            }
                        }
                        else if (u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.6.1";
                        }
                        else if (u.def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.6.2";
                        }
                        // وزيرة الدفاع الأمريكية
                    }
                    else if (u.ecase.Equals("") && !Analyzed_Texts[Case_ID - 2].stem.Contains("/ADJ"))
                    {
                        if (u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID - 2].def.Equals("INDEF"))
                        {
                            if ((u.stem.Contains("A/") || (u.stem.Contains("y/") && !u.stem.Contains("oy/")) || (u.stem.Contains("w/") && !u.stem.Contains("ow/")) || u.stem.Contains("Y/")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "e";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].ecase.EndsWith("F") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.2";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("N") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.3";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.4";
                            }
                        }
                        else if (u.ecase.Equals(""))
                        {
                            if ((Analyzed_Texts[Case_ID - 2].ecase.Equals("u") || Analyzed_Texts[Case_ID - 2].ecase.Equals("a")) && Analyzed_Texts[Case_ID - 2].def.Equals("INDEF"))
                            {
                                if (Analyzed_Texts[Case_ID - 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals("") && Analyzed_Texts[Case_ID - 2].ecase.Equals("u"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.1";
                                }
                                else if (Analyzed_Texts[Case_ID - 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals("") && Analyzed_Texts[Case_ID - 2].ecase.Equals("a"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "K";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.3";
                                }
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5";
                            }
                        }
                    }
                }
                #endregion

                #region >aw/CONJ
                else if (Analyzed_Texts[Case_ID - 2].stem.Equals(">aw/CONJ") && u.ecase.Equals(""))
                {
                    if (u.def.Equals("INDEF") && u.spattern.Equals(">afoEal"))
                    {
                        if (Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 3].ecase.Equals("N") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("Ani/N_SUF")) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "u";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.1";
                        }
                        else if (Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") || Analyzed_Texts[Case_ID - 3].ecase.Equals("a") || (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("ayoni/N_SUF") && (Analyzed_Texts[Case_ID - 3].pr.Contains("PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP")))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.2";
                        }
                        else if (Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") || Analyzed_Texts[Case_ID - 3].ecase.Equals("F") || (Analyzed_Texts[Case_ID - 3].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("ayoni/N_SUF"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 3].def.Equals("DEF") && Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && u.ecase.Equals(""))
                    {
                        if (!Analyzed_Texts[Case_ID - 3].ecase.Equals("e") && u.ecase.Equals(""))
                        {
                            if (u.suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID - 3].ecase.Equals("a") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].suf.Equals("At/NSUF") && Analyzed_Texts[Case_ID - 3].ecase.Equals("i") && !Analyzed_Texts[Case_ID - 3].pr.Contains("PREP") && !Analyzed_Texts[Case_ID - 4].stem.Contains("PREP") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.2";
                            }
                            else if ((Analyzed_Texts[Case_ID - 1].stem.Contains("y/") || Analyzed_Texts[Case_ID - 1].stem.Contains("w/")) && !Analyzed_Texts[Case_ID - 1].stem.Contains("oy/") && !Analyzed_Texts[Case_ID - 1].stem.Contains("ow/") && Analyzed_Texts[Case_ID - 1].ecase.Contains(""))
                            {
                                if (Analyzed_Texts[Case_ID - 3].ecase.Equals("a"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.1";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.2";
                                }
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 3].ecase;
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.4.1";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("e") && u.ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 3].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("Ani/N_SUF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("ayoni/N_SUF"))
                            {
                                if ((Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.2";
                                }
                                else if (!(Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.suf.Equals("At/N_SUF") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.3";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.4";
                                }
                            }
                            else if (Analyzed_Texts[Case_ID - 3].stem.Contains("Y/") || Analyzed_Texts[Case_ID - 3].stem.Contains("y/") || Analyzed_Texts[Case_ID - 3].stem.Contains("A/") || Analyzed_Texts[Case_ID - 3].stem.Contains("w/"))
                            {
                                if ((Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.22.1";
                                }
                                else if (!(Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.suf.Equals("At/N_SUF") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.22.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.22.3";
                                }
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("F") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                    {
                        if (u.suf.Equals("At/N_SUF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.1";
                        }
                        else if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                            u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                            u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                            u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.2";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("a") && (Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("EDAFAH"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.4.1";
                        }
                        else if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.4.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("i") && (Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("EDAFAH") || u.def.Equals("DEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.5.1";
                        }
                        else if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.5.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") && (Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("INDEF")) && u.ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID - 4].stem.Contains("<s>") && Analyzed_Texts[Case_ID - 3].spattern.Equals("mafaAE") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 1].def.Equals("DEF") || Analyzed_Texts[Case_ID - 1].def.Equals("EDAFAH"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.1";
                            }
                            else if (u.def.Equals("INDEF"))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                                u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                                u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.3";
                                }
                            }
                        }
                        else if (u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.6.1";
                        }
                        else if (u.def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.6.2";
                        }
                        // وزيرة الدفاع الأمريكية
                    }
                    else if (u.ecase.Equals("") && !Analyzed_Texts[Case_ID - 3].stem.Contains("/ADJ"))
                    {
                        if (u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID - 3].def.Equals("INDEF"))
                        {
                            if ((u.stem.Contains("A/") || (u.stem.Contains("y/") && !u.stem.Contains("oy/")) || (u.stem.Contains("w/") && !u.stem.Contains("ow/")) || u.stem.Contains("Y/")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "e";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].ecase.EndsWith("F") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.2";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("N") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.3";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.4";
                            }
                        }
                        else if (u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 3].ecase;
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5";
                        }
                    }
                }
                #endregion
            });
            #endregion

            #region 3.16 set case for waHod, nafos, baEoD
            var badal_baEoD = Analyzed_Texts.Where(a => ((a.stem.Equals("waHod/NOUN") || a.stem.Equals("nafos/NOUN") || a.stem.Equals("baEoD/NOUN") || a.stem.Equals("kul~/NOUN")) && a.suf.Contains("/POSS") && a.ecase.Equals(""))).ToList();
            badal_baEoD.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Equals("baEoD/NOUN"))
                {
                    if (Analyzed_Texts[Case_ID].def.Equals("DEF") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.16.1.1";
                    }
                    else if (u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "u";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.16.1.2";
                    }
                }
                else if (u.ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.16.2";
                }
            });
            #endregion

            #region 3.Defaults
            var Defaults = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN") && a.ecase.Equals(""))).ToList();
            Defaults.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (u.def.Equals("DEF") || u.def.Equals("EDAFAH"))
                {
                    if (u.suf.Contains("At/"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.1.Default";
                    }
                    else if ( ((u.stem.Contains("y/") && !u.stem.Contains("oy/")) || (u.stem.Contains("w/") && !u.stem.Contains("ow/"))) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.2.Default";
                    }
                    else if (u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.3.Default";
                    }
                }
                else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                {
                    if (u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") ||
                u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") || u.spattern.Equals("faEaAliyl") ||
                u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") ||
                u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") ||
                u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") ||
                (u.spattern.Equals("") && (Regex.IsMatch(u.stem, ".(awA).(iy).") || Regex.IsMatch(u.spattern, ".(awA).(i)."))) || u.stem.StartsWith("jahan~am") ||
                (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) && !u.stem.StartsWith(">arobaE/"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.4.Default";
                    }
                    else if (u.suf.Contains("At/") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "K";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.5.Default";
                    }
                    else if (u.ecase.Equals(""))
                    {
                        if (!Analyzed_Texts[Case_ID - 1].word.EndsWith("ا"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "N";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.6.1.Default";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:3.6.2.Default";
                        }
                    }
                }
            });
            #endregion
            #endregion

            #region 4. Set_Case_For_NOUN_PROP
            #region 4.1 if the prefix of the word contains PREP
            var NP_List = Analyzed_Texts.Where(a => a.pr.Contains("PREP") && a.stem.EndsWith("/NOUN_PROP") && a.ecase.Equals("")).ToList();
            NP_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Case_ID - 1].spattern.Equals(""))
                {
                    if (u.stem.StartsWith("Al") || u.pr.Contains("Al/DET") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.1";
                    }
                    else if (u.stem.Contains("Y/") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "F";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.2";
                    }
                    else if ((u.spattern.Equals("fiEol") || u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                         u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") || u.spattern.Equals("fawaAEiyl") ||
                         u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") || u.spattern.Equals("lafoEaA'") || (u.Equals("faEolaAl") && u.stem.Contains("'/")) ||
                        ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.3";
                    }
                    else if ((u.suf.Equals("ap/N_SUF") || u.stem.Contains("ap/")) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.4";
                    }
                    else if (u.stem.Equals("<isorA}iyl/NOUN_PROP") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.5";
                    }
                    else
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "K";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.6";
                    }
                }

                else if (Analyzed_Texts[Case_ID - 1].spattern.Equals("") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID - 1].stem.Equals("<isolA}iyl/NOUN_PROP"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.2.1";
                    }
                    else
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "e";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.2.2";
                    }
                }
            });
            #endregion

            #region 4.2 if the tag of the previous word contains PREP
            NP_List = Analyzed_Texts.Where(a => a.stem.Contains("PREP")).ToList();
            NP_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.Contains("/NOUN_PROP") && !Analyzed_Texts[Case_ID].spattern.Equals("") && !Analyzed_Texts[Case_ID].spattern.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID].stem.StartsWith("Al") || Analyzed_Texts[Case_ID].pr.Contains("Al/DET") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "i";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.3.1";
                        }
                        else if (Analyzed_Texts[Case_ID].stem.Contains("Y/") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "F";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.3.2";
                        }
                        else if ((Analyzed_Texts[Case_ID].spattern.Equals("fiEol") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") ||
                                    Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID].stem.Contains("~/"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.3.3";
                        }
                        else if ((Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID].stem.Contains("ap/")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.3.4";
                        }
                        else if (Analyzed_Texts[Case_ID].stem.Equals("<isorA}iyl/NOUN_PROP") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID].ecase = "a";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.3.5";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID].ecase = "K";
                            Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.3.6";
                        }
                    }

                    else if (Analyzed_Texts[Case_ID].stem.EndsWith("/NOUN_PROP") && Analyzed_Texts[Case_ID].spattern.Equals("") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "e";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.1.4.2";
                    }
                }
            });
            #endregion

            #region 4.3 if the def of the parevious word is EDAFAH
            NP_List = Analyzed_Texts.Where(a => a.def.Equals("EDAFAH") && !a.suf.Contains("POSS")).ToList();
            NP_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Case_ID].stem.Contains("/NOUN_PROP") && !Analyzed_Texts[Case_ID].spattern.Equals("") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    if (Analyzed_Texts[Case_ID].stem.StartsWith("Al") || Analyzed_Texts[Case_ID].pr.Contains("Al/DET") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "i";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.3.1";
                    }
                    else if (Analyzed_Texts[Case_ID].stem.Contains("Y/") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "F";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.3.2";
                    }
                    else if ((Analyzed_Texts[Case_ID].spattern.Equals("fiEol") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEil") || Analyzed_Texts[Case_ID].spattern.Equals(">afaAEiyl") ||
                                Analyzed_Texts[Case_ID].spattern.Equals(">afoEal") || Analyzed_Texts[Case_ID].spattern.Equals("faEaA}il") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("faEaAlil") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("faEaAliyl") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("mafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("tafaAEil") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("tafaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEil") || Analyzed_Texts[Case_ID].spattern.Equals("fawaAEiyl") || Analyzed_Texts[Case_ID].spattern.Equals("fayaAEil") ||
                                Analyzed_Texts[Case_ID].spattern.Equals(">afoEilaA'") || Analyzed_Texts[Case_ID].spattern.Equals("faEolaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fiEoliyaA'") || Analyzed_Texts[Case_ID].spattern.Equals("fuEalaA'") ||
                                Analyzed_Texts[Case_ID].spattern.Equals("lafoEaA'") || (Analyzed_Texts[Case_ID].spattern.Equals("faEolaAl") && Analyzed_Texts[Case_ID].stem.Contains("'/")) || ((Regex.IsMatch(Analyzed_Texts[Case_ID].spattern, ".(a).(A).(i).")) && Analyzed_Texts[Case_ID].stem.Contains("~/"))) && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.3.3";
                    }
                    else if ((Analyzed_Texts[Case_ID].suf.Equals("ap/N_SUF") || Analyzed_Texts[Case_ID].stem.Contains("ap/") || Analyzed_Texts[Case_ID].stem.Contains("A'/")) && Analyzed_Texts[Case_ID].ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.3.4";
                    }
                    else if (Analyzed_Texts[Case_ID].stem.Equals("<isorA}iyl/NOUN_PROP"))
                    {
                        Analyzed_Texts[Case_ID].ecase = "a";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.3.5";
                    }
                    else
                    {
                        Analyzed_Texts[Case_ID].ecase = "K";
                        Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.3.6";
                    }
                }

                else if (Analyzed_Texts[Case_ID].stem.Contains("/NOUN_PROP") && Analyzed_Texts[Case_ID].spattern.Equals("") && Analyzed_Texts[Case_ID].ecase.Equals(""))
                {
                    Analyzed_Texts[Case_ID].ecase = "e";
                    Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.3.4.2";
                }
            });
            #endregion

            #region 4.4 for AF case
            NP_List = Analyzed_Texts.Where(a => a.word.EndsWith("ا") && !a.stem.Contains("A/") && a.suf.Equals("") && a.ecase.Equals("")).ToList();
            NP_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                Analyzed_Texts[Case_ID].ecase = "F";
                Analyzed_Texts[Case_ID].affectedBy = Analyzed_Texts[Case_ID].affectedBy + "_C:4.4";
            });
            #endregion

            #region 4.5 >ay~Am Alo>usobuwE
            NP_List = Analyzed_Texts.Where(a => (a.stem.Equals("sabot/NOUN_PROP") || a.stem.Equals(">aHad/NOUN_PROP") || a.stem.Equals("{ivonayoni/NOUN_PROP") || a.stem.Equals("vulavA''/NOUN_PROP") || a.stem.Equals(">arobiEA''/NOUN_PROP") || a.stem.Equals("xamiys/NOUN_PROP") || a.stem.Equals("jumoE/NOUN_PROP")) && (a.pr.Equals("Al/DET") && a.ecase.Equals(""))).ToList();
            NP_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Case_ID - 1].spattern.Equals(""))
                {
                    if (u.stem.StartsWith("Al") || u.pr.Contains("Al/DET") && u.ecase.Equals(""))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:4.1.1.1";
                    }
                }
            });
            #endregion
            #endregion

            #region 5. Set_Case_For_ADJ
            var A_List = Analyzed_Texts.Where(a => a.stem.Contains("/ADJ") && a.ecase.Equals("")).ToList();
            A_List.ToList().ForEach(u =>
            {
                Case_ID = int.Parse(u.ID);
                #region ADJ After NOUN
                if (u.ecase.Equals(""))
                {
                    if (u.def.Equals("INDEF") && u.spattern.Equals(">afoEal"))
                    {
                        if (Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 2].ecase.Equals("N") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("Ani/N_SUF")) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "u";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.1";
                        }
                        else if (Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") || Analyzed_Texts[Case_ID - 2].ecase.Equals("a") || (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("ayoni/N_SUF") && (Analyzed_Texts[Case_ID - 2].pr.Contains("PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP")))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.2";
                        }
                        else if (Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") || Analyzed_Texts[Case_ID - 2].ecase.Equals("F") || (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("ayoni/N_SUF"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.1.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].def.Equals("DEF") && Analyzed_Texts[Case_ID - 2].stem.Contains("/NOUN") && u.ecase.Equals(""))
                    {
                        if (!Analyzed_Texts[Case_ID - 2].ecase.Equals("e") && u.ecase.Equals(""))
                        {
                            if (u.suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].suf.Equals("At/NSUF") && Analyzed_Texts[Case_ID - 2].ecase.Equals("i") && !Analyzed_Texts[Case_ID - 2].pr.Contains("PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("PREP") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.2";
                            }
                            else if ((Analyzed_Texts[Case_ID - 1].stem.Contains("y/") || Analyzed_Texts[Case_ID - 1].stem.Contains("w/")) && !Analyzed_Texts[Case_ID - 1].stem.Contains("oy/") && !Analyzed_Texts[Case_ID - 1].stem.Contains("ow/") && !Analyzed_Texts[Case_ID -1].suf.Equals("ap/N_SUF") && Analyzed_Texts[Case_ID - 1].ecase.Contains(""))
                            {
                                if (Analyzed_Texts[Case_ID - 2].ecase.Equals("a"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.1";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.2";
                                }
                            }
                            else if (Analyzed_Texts[Case_ID - 3].stem.Equals(">an~a/CONJ") && Analyzed_Texts[Case_ID - 3].suf.Equals("") && Analyzed_Texts[Case_ID - 2].stem.EndsWith("/NOUN") && Analyzed_Texts[Case_ID - 2].def.Equals("DEF") && Analyzed_Texts[Case_ID - 1].ecase.Contains(""))
                            {
                                if (Analyzed_Texts[Case_ID - 2].def.Equals("DEF") || Analyzed_Texts[Case_ID - 2].def.Equals("EDAFAH"))
                                {
                                    if (Analyzed_Texts[Case_ID - 1].suf.Equals("At/N_SUF"))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.1.1.1";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.1.1.2";
                                    }
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "e";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.3.2";
                                }
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.1.4.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("e") && u.ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 2].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("Ani/N_SUF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 2].suf.StartsWith("ayoni/N_SUF"))
                            {
                                if ((Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.2";
                                }
                                else if (!(Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") && !Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.suf.Equals("At/N_SUF") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.3";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.2.2.4";
                                }
                            }
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("F") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                    {
                        if (u.suf.Equals("At/N_SUF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.1";
                        }
                        else if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                            u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                            u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                            u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "a";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.2";
                        }
                        else
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "F";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.3.3";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && ((Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || (Analyzed_Texts[Case_ID - 2].def.Equals("EDAFAH") && !Analyzed_Texts[Case_ID - 2].suf.Contains("POSS")))) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("EDAFAH"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.4.1";
                        }
                        else if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.4.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("i") && (Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 2].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                    {
                        if (u.def.Equals("EDAFAH") || u.def.Equals("DEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.5.1";
                        }
                        else if (u.def.Equals("INDEF"))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.5.2";
                        }
                    }
                    else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") && (Analyzed_Texts[Case_ID - 2].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 2].def.Equals("INDEF")) && u.ecase.Equals(""))
                    {
                        if (Analyzed_Texts[Case_ID - 3].stem.Contains("<s>") && Analyzed_Texts[Case_ID - 2].spattern.Equals("mafaAE") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 1].def.Equals("DEF") || Analyzed_Texts[Case_ID - 1].def.Equals("EDAFAH"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.1";
                            }
                            else if (u.def.Equals("INDEF"))
                            {
                                if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                                u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                                u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.7.3";
                                }
                            }
                        }
                        else if (u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "i";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.6.1";
                        }
                        else if (u.def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                        {
                            Analyzed_Texts[Case_ID - 1].ecase = "K";
                            Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.6.2";
                        }
                        // وزيرة الدفاع الأمريكية
                    }
                    else if (u.ecase.Equals("") && !Analyzed_Texts[Case_ID - 2].stem.Contains("/ADJ"))
                    {
                        if (u.def.Equals("EDAFAH") && Analyzed_Texts[Case_ID - 2].def.Equals("INDEF"))
                        {
                            if ((u.stem.Contains("A/") || (u.stem.Contains("y/") && !u.stem.Contains("oy/")) || (u.stem.Contains("w/") && !u.stem.Contains("ow/")) || u.stem.Contains("Y/")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "e";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].ecase.EndsWith("F") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.2";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("N") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.3";
                            }
                            else if (Analyzed_Texts[Case_ID - 2].ecase.Equals("K") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.4";
                            }
                        }
                        else if (u.ecase.Equals(""))
                        {
                            if ((Analyzed_Texts[Case_ID - 2].ecase.Equals("u") || Analyzed_Texts[Case_ID - 2].ecase.Equals("a")) && Analyzed_Texts[Case_ID - 2].def.Equals("INDEF"))
                            {
                                if (Analyzed_Texts[Case_ID - 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals("") && Analyzed_Texts[Case_ID - 2].ecase.Equals("u"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.1";
                                }
                                else if (Analyzed_Texts[Case_ID - 1].def.Equals("INDEF") && Analyzed_Texts[Case_ID - 1].ecase.Equals("") && Analyzed_Texts[Case_ID - 2].ecase.Equals("a"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "K";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.3";
                                }
                            }
                            else
                            {
                                if (Analyzed_Texts[Case_ID - 2].stem.Contains("/PREP") && Analyzed_Texts[Case_ID - 3].stem.Contains("/ADJ"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 3].ecase;
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.1";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5.2";
                                }
                            }
                        }
                    }
                    else if (u.ecase.Equals("") && Analyzed_Texts[Case_ID - 1].affectedBy.EndsWith("_C:3.1.Default"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5";
                    }
                    else if (u.ecase.Equals("") && Analyzed_Texts[Case_ID - 1].affectedBy.EndsWith("_C:3.5.Default"))
                    {
                        Analyzed_Texts[Case_ID - 1].ecase = "F";
                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.1.8.5";
                    }
                    #endregion

                    #region ADJ After ADJ
                    else if (Analyzed_Texts[Case_ID - 2].stem.EndsWith("ADJ") && Analyzed_Texts[Case_ID - 1].ecase.Equals(""))
                    {
                        if (u.def.Equals("INDEF") && u.spattern.Equals(">afoEal"))
                        {
                            if (Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 3].ecase.Equals("N") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("Ani/N_SUF")) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "u";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.1.1";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") || Analyzed_Texts[Case_ID - 3].ecase.Equals("a") || (Analyzed_Texts[Case_ID - 3].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("ayoni/N_SUF") && (Analyzed_Texts[Case_ID - 3].pr.Contains("PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP")))) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.1.2";
                            }
                            else if (Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") || Analyzed_Texts[Case_ID - 3].ecase.Equals("F") || (Analyzed_Texts[Case_ID - 3].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("ayoni/N_SUF"))) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.1.3";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 3].def.Equals("DEF") && Analyzed_Texts[Case_ID - 3].stem.Contains("/NOUN") && u.ecase.Equals(""))
                        {
                            if (!Analyzed_Texts[Case_ID - 3].ecase.Equals("e") && u.ecase.Equals(""))
                            {
                                if (u.suf.Equals("At/N_SUF") && Analyzed_Texts[Case_ID - 2].ecase.Equals("a") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "i";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.1.1";
                                }
                                else if (Analyzed_Texts[Case_ID - 3].suf.Equals("At/NSUF") && Analyzed_Texts[Case_ID - 3].ecase.Equals("i") && !Analyzed_Texts[Case_ID - 3].pr.Contains("PREP") && !Analyzed_Texts[Case_ID - 3].stem.Contains("PREP") && u.ecase.Equals(""))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "a";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.1.2";
                                }
                                else
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = Analyzed_Texts[Case_ID - 2].ecase;
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.1.3";
                                }
                            }
                            else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("e") && u.ecase.Equals(""))
                            {
                                if (Analyzed_Texts[Case_ID - 3].suf.StartsWith("uwna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atAni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("Ani/N_SUF"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.2.1";
                                }
                                else if (Analyzed_Texts[Case_ID - 3].suf.StartsWith("iyna/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("atayoni/N_SUF") || Analyzed_Texts[Case_ID - 3].suf.StartsWith("ayoni/N_SUF"))
                                {
                                    if ((Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.2.2";
                                    }
                                    else if (!(Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") && !Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") && !Analyzed_Texts[Case_ID - 4].def.Equals("EDAFAH")) && u.suf.Equals("At/N_SUF") && u.ecase.Equals(""))
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "i";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.2.3";
                                    }
                                    else
                                    {
                                        Analyzed_Texts[Case_ID - 1].ecase = "a";
                                        Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.2.2.4";
                                    }
                                }
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("F") && u.def.Equals("INDEF") && u.ecase.Equals(""))
                        {
                            if (u.suf.Equals("At/N_SUF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "K";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.3.1";
                            }
                            else if ((u.spattern.Equals(">afaAEil") || u.spattern.Equals(">afaAEiyl") || u.spattern.Equals(">afoEal") || u.spattern.Equals("faEaA}il") || u.spattern.Equals("faEaAEiyl") || u.spattern.Equals("faEaAlil") ||
                                u.spattern.Equals("faEaAliyl") || u.spattern.Equals("mafaAEil") || u.spattern.Equals("mafaAEiyl") || u.spattern.Equals("tafaAEil") || u.spattern.Equals("tafaAEiyl") || u.spattern.Equals("fawaAEil") ||
                                u.spattern.Equals("fawaAEiyl") || u.spattern.Equals("fayaAEil") || u.spattern.Equals(">afoEilaA'") || u.spattern.Equals("faEolaA'") || u.spattern.Equals("fiEoliyaA'") || u.spattern.Equals("fuEalaA'") ||
                                u.spattern.Equals("lafoEaA'") || (u.spattern.Equals("faEolaAl") && u.stem.Contains("'/")) || ((Regex.IsMatch(u.spattern, ".(a).(A).(i).")) && u.stem.Contains("~/"))) && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "a";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.3.2";
                            }
                            else
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "F";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.3.3";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("a") && (Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                        {
                            if (u.def.Equals("EDAFAH"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.4.1";
                            }
                            else if (u.def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "K";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.4.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("i") && (Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("EDAFAH")) && u.ecase.Equals(""))
                        {
                            if (u.def.Equals("EDAFAH") || u.def.Equals("DEF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.5.1";
                            }
                            else if (u.def.Equals("INDEF"))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "K";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.5.2";
                            }
                        }
                        else if (Analyzed_Texts[Case_ID - 3].ecase.Equals("K") && (Analyzed_Texts[Case_ID - 3].pr.Contains("/PREP") || Analyzed_Texts[Case_ID - 4].stem.Contains("/PREP") || Analyzed_Texts[Case_ID - 3].def.Equals("INDEF")) && u.ecase.Equals(""))
                        {
                            if (Analyzed_Texts[Case_ID - 4].stem.Contains("<s>") && u.ecase.Equals(""))
                            {
                                if (u.def.Equals("EDAFAH"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "u";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.6.1";
                                }
                                else if (u.def.Equals("INDEF"))
                                {
                                    Analyzed_Texts[Case_ID - 1].ecase = "N";
                                    Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.6.2";
                                }
                            }
                            else if (u.def.Equals("EDAFAH") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "i";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.6.1";
                            }
                            else if (u.def.Equals("INDEF") && u.ecase.Equals(""))
                            {
                                Analyzed_Texts[Case_ID - 1].ecase = "K";
                                Analyzed_Texts[Case_ID - 1].affectedBy = Analyzed_Texts[Case_ID - 1].affectedBy + "_C:5.2.6.2";
                            } // وزيرة الدفاع الأمريكية
                        }
                    }
                }
                #endregion
            });
            #endregion

            Analyzed_Text = Analyzed_Texts;
        }
    }
}

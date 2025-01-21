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
    class Set_Definiteness
    {
        public Set_Definiteness()
        {

        }
        #region Variabls and Constructors
        public int Def_ID;
        public string spatterndet;
        public string tag_suf = "";
        public string tag_prf = "";
        #endregion
        public void Set_Definiteness_Classifier(List<Analyzed_Text> Analyzed_Texts, out List<Analyzed_Text> Analyzed_Text)
        {
            #region 1. Set_Analyzed_Text.def as "DEF"
            var DEF_List = Analyzed_Texts.Where(a => ((a.pr.Contains("Al/DET") && !a.stem.StartsWith(">amos/") && !a.stem.StartsWith("gad/")) || (a.stem.Contains("PRON") && !a.stem.Equals("mA/PRON")) ||
            a.stem.Contains("NOUN_PROP") || a.stem.StartsWith("Al|n/") || a.stem.StartsWith("hunA/") || a.stem.StartsWith("hunAk/") || a.stem.StartsWith("hunAlik/NOUN") ||
            a.stem.StartsWith("waqota*Ak/") || a.stem.StartsWith("Hiyna*Ak/") || a.stem.StartsWith("|na}i*/") || a.stem.StartsWith("baEoda}i*/") ||
            a.stem.StartsWith("Einoda}i*/") || a.stem.StartsWith("muno*u}i*/") || a.stem.StartsWith("Hiyna}i*/") || a.stem.StartsWith("|na*Ak/") ||
            a.stem.StartsWith("h`ka*A/") || ((a.stem.StartsWith(">amos/") || a.stem.StartsWith("gad/")) && !a.pr.Contains("Al/DET"))) && a.def.Equals("")).ToList();
            DEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "DEF";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:1.1";
            });

            DEF_List = Analyzed_Texts.Where(a => a.stem.Equals("mA/PRON") && a.def.Equals("")).ToList();
            DEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Def_ID].stem.Equals("<i*A/NOUN") && !Analyzed_Texts[Def_ID].stem.Equals("lam/PART") && !Analyzed_Texts[Def_ID].stem.Equals("<in/CONJ") && !Analyzed_Texts[Def_ID - 2].stem.Equals("<i*A/NOUN"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "DEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:1.2";
                }
            });

            DEF_List = Analyzed_Texts.Where(a => (a.stem.StartsWith("bAbA/") || a.stem.StartsWith("mAmA/")) && a.def.Equals("")).ToList();
            DEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Def_ID].stem.Contains("NOUN_PROP"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "DEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:1.3";
                }
            });
            #endregion

            #region 2. Set_Analyzed_Text.def as "INDEF"
            #region Rule: 2.1
            var INDEF_List = Analyzed_Texts.Where(a => (((a.suf.Equals("Ani/N_SUF") || a.suf.Equals("ayoni/N_SUF") || a.suf.Equals("atAni/N_SUF") || a.suf.Equals("atayoni/N_SUF") ||
            a.suf.Equals("uwna/N_SUF") || a.suf.Equals("iyna/N_SUF")) && !a.pr.Contains("Al/DET")) || (a.stem.Equals(">amos") && a.pr.Contains("Al/DET")) ||
            (a.stem.Equals("baEodamA/NOUN") || a.stem.Equals("bayonamA/NOUN") || a.stem.Equals("HiynamA") || a.stem.Equals("kul~amA") || a.stem.Equals("waqotamA") ||
            a.stem.Equals("mataY") || a.stem.Equals("faqaT"))) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "INDEF";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.1.1";
            });

            INDEF_List = Analyzed_Texts.Where(a => (a.word.EndsWith("ا") && (!a.stem.Contains("A/")) && (a.suf.Equals("")) && (a.stem.Contains("NOUN") || a.stem.Contains("ADV") || a.stem.Contains("ADJ"))) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "INDEF";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.1.2";
            });
            #endregion

            #region Rule: 2.2
            INDEF_List = Analyzed_Texts.Where(a => (a.stem.Contains("ADV") && (!a.suf.Contains("POSS") && !a.suf.Contains("iy/N_SUF") && !a.suf.Contains("atayo/N_SUF"))) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].pr.StartsWith("Al/DET"))
                {
                    spatterndet = u.spattern + "Al/DET";
                }
                else
                {
                    spatterndet = "";
                }
                if (!spatterndet.Equals("mafoEuwlAl/DET") && !spatterndet.Equals("faEiylAl/DET") && !spatterndet.Equals("mufaEolalAl/DET"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.3";
                }
            });
            #endregion

            #region Rule: 2.4
            INDEF_List = Analyzed_Texts.Where(a => ((a.stem.Contains("ADJ")) && (a.suf.Equals("") || a.suf.Equals("ap/N_SUF") || a.suf.Equals("At/N_SUF")) && (a.pr == "")) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Def_ID - 2].def.Equals("INDEF") && Analyzed_Texts[Def_ID - 2].stem.Equals("NOUN") && !Analyzed_Texts[Def_ID].stem.Contains("NOUN")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.4.1";
                }
            });

            INDEF_List = Analyzed_Texts.Where(a => (a.stem.Contains("NOUN") && !a.suf.Contains("POSS") && a.pr.Contains("")) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.Equals("bud~/NOUN"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.4.2";
                }
            });
            #endregion

            #region Rule: 2.5
            INDEF_List = Analyzed_Texts.Where(a => ((a.stem.Equals("baEod/NOUN") || a.stem.Equals("qabol/NOUN") || a.stem.Equals("fawoq/NOUN")) && (a.suf.Equals(""))) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID - 2].stem.Equals("min/PREP") && Analyzed_Texts[Def_ID - 2].suf.Equals(""))
                {
                    if (!Analyzed_Texts[Def_ID].stem.Contains("/NOUN") && !Analyzed_Texts[Def_ID].stem.Contains("/CONJ"))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.5.1";
                    }
                    else if ((Analyzed_Texts[Def_ID].stem.Contains("/PV") || Analyzed_Texts[Def_ID].stem.Contains("/IV")) && Analyzed_Texts[Def_ID].def.Equals(""))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.5.2";
                    }
                }
            });
            #endregion

            #region Rule: 2.6
            INDEF_List = Analyzed_Texts.Where(a => (!a.pr.Contains("Al/DET") && (a.stem.Contains("NOUN") || a.stem.Contains("ADJ"))) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (u.stem.Contains("NOUN") && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (!Analyzed_Texts[Def_ID].stem.Contains("NOUN") && (!Regex.IsMatch(Analyzed_Texts[Def_ID].word, @"^[0-9]+$")) && (Regex.IsMatch(Analyzed_Texts[Def_ID - 2].word, @"^[3-9]$"))))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.6.1";
                }
                else if (u.stem.Contains("NOUN") && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID - 2].word.Length == 2) && (Regex.IsMatch(Analyzed_Texts[Def_ID].word, @"^\d{2}$")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.6.2";
                }
                else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && Analyzed_Texts[Def_ID].stem.Equals("faqaT/NOUN"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.6.3";
                }
                else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && !u.stem.StartsWith(">ajol/") && !u.stem.StartsWith("xa$oy/") && ((Analyzed_Texts[Def_ID].pr.Contains("/PREP") && !Analyzed_Texts[Def_ID].pr.Contains("wa/PREP")) || Analyzed_Texts[Def_ID].stem.Contains("/PREP") || Analyzed_Texts[Def_ID].stem.Equals("maE/NOUN") || (Analyzed_Texts[Def_ID].stem.Equals("li>an~a/SUB_CONJ") || Analyzed_Texts[Def_ID].stem.Equals(">an/SUB_CONJ") || (Analyzed_Texts[Def_ID].stem.Equals("NEG_PART") && !Analyzed_Texts[Def_ID].pr.Contains("Al/DET")) || Analyzed_Texts[Def_ID].stem.Equals("<in~a/SUB_CONJ") || Analyzed_Texts[Def_ID].stem.Equals("qad/VERB_PART") || Analyzed_Texts[Def_ID].stem.Contains("IV") || Analyzed_Texts[Def_ID].stem.Contains("PV") ||
                        Analyzed_Texts[Def_ID].stem.StartsWith("mim~A/") || Analyzed_Texts[Def_ID].stem.StartsWith("mim~an") || Analyzed_Texts[Def_ID].stem.StartsWith(">am~A"))))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.5";
                }
                else if ((u.stem.Contains("NOUN")) && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID - 2].stem.StartsWith("Ea$ar") || Analyzed_Texts[Def_ID - 2].stem.StartsWith("Ea$or")))
                {
                    if (Analyzed_Texts[Def_ID].stem.EndsWith("/NOUN"))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.6.1";
                    }
                    else
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.6.2";
                    }
                }
                else if ((u.stem.Contains("NOUN")) && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID - 2].Equals(">aw/CONJ")) && (Analyzed_Texts[Def_ID - 3].def.Equals("INDEF")) && (Analyzed_Texts[Def_ID].stem.Equals("Punc")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.8";
                }
                else if (u.stem.Contains("ADJ") && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID - 2].def.Equals("INDEF") && Analyzed_Texts[Def_ID].stem.Contains("ADJ") && Analyzed_Texts[Def_ID + 1].stem.Contains("PREP")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.9";
                }
                else
                {//need to check
                    if (u.stem.Contains("NOUN") && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID].stem.Contains("ADJ") && !Analyzed_Texts[Def_ID].pr.Contains("Al/DET") && !Analyzed_Texts[Def_ID + 1].def.Equals("EDAFAH")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.10";
                    }
                    else if (Analyzed_Texts[Def_ID].stem.Contains("ADJ") && Analyzed_Texts[Def_ID].pr.StartsWith("wa/CONJ"))
                    {
                        tag_prf = "wa/CONJADJ";
                    }
                    if (u.stem.Contains("ADJ") && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID - 2].def.Equals("INDEF") && Analyzed_Texts[Def_ID - 2].stem.Contains("ADJ") && !Analyzed_Texts[Def_ID].stem.Contains("NOUN")) && (tag_prf.Equals("wa/CONJADJ")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.11";
                        tag_prf = "";
                    }
                    else if (u.stem.Contains("ADJ") && (u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID - 2].def.Equals("INDEF") && Analyzed_Texts[Def_ID - 2].stem.Contains("ADJ") && !Analyzed_Texts[Def_ID].stem.Contains("NOUN")) && (Analyzed_Texts[Def_ID].pr.StartsWith("wa/CONJ") || Analyzed_Texts[Def_ID].pr.StartsWith("PREP") || Analyzed_Texts[Def_ID].pr.StartsWith("fa/CONJ")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.12";
                    }
                    else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && (Analyzed_Texts[Def_ID].stem.Equals("<il~A/PART") || Analyzed_Texts[Def_ID].stem.Equals("vum~a/CONJ") || Analyzed_Texts[Def_ID].stem.Equals("<in~a/PART")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.13"; // and 2.6.14 and 2.6.15
                    }
                    else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && (Analyzed_Texts[Def_ID - 2].stem.StartsWith("siwaY/") && Analyzed_Texts[Def_ID].stem.Contains("ADJ")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.16";
                    }
                    else if (u.stem.Contains("ADJ") && Analyzed_Texts[Def_ID].stem.Contains("ADJ") && Analyzed_Texts[Def_ID].pr.Equals("") && !Analyzed_Texts[Def_ID + 1].pr.Equals("Al/DET"))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.17";
                    }
                    else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && (Analyzed_Texts[Def_ID].pr.StartsWith("wa/CONJ") && !Analyzed_Texts[Def_ID].pr.Contains("Al/DET") && Analyzed_Texts[Def_ID + 1].stem.Contains("PREP")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.18";
                    }
                    else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && (Analyzed_Texts[Def_ID].stem.StartsWith("gayor") && Analyzed_Texts[Def_ID].pr.Equals("") && Analyzed_Texts[Def_ID].suf.Equals("")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.19";
                    }
                    else if ((u.stem.Contains("NOUN") || u.stem.Contains("ADJ")) && !u.suf.Contains("POSS") && (Analyzed_Texts[Def_ID].stem.Contains("ADJ") && Analyzed_Texts[Def_ID].pr.Equals("")) && (!Analyzed_Texts[Def_ID].pr.Equals("") && Analyzed_Texts[Def_ID + 1].pr.StartsWith("Al/DET")))
                    {
                        Analyzed_Texts[Def_ID - 1].def = "INDEF";
                        Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D2.6.20";
                    }
                }
            });
            #endregion

            #region Rule: 2.9
            INDEF_List = Analyzed_Texts.Where(a => !a.pr.Contains("Al/DET") && !a.suf.Contains("POSS") && (a.stem.EndsWith("/NOUN") || a.stem.EndsWith("/ADJ")) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].word.Equals(".") || Analyzed_Texts[Def_ID].word.Equals("،") || Analyzed_Texts[Def_ID].stem.Contains("</s>"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.9.1";
                }
                else if ((Analyzed_Texts[Def_ID - 1].stem.EndsWith("/NOUN") || Analyzed_Texts[Def_ID - 1].stem.EndsWith("/ADJ")) && (Analyzed_Texts[Def_ID].stem.EndsWith("/NOUN") && Analyzed_Texts[Def_ID].word.EndsWith("ا") && Analyzed_Texts[Def_ID].suf.Equals("")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.9.2";
                }
            });
            #endregion

            #region Rule: 2.10
            INDEF_List = Analyzed_Texts.Where(a => (a.pr.Contains("/PREP") && !a.pr.Contains("Al/DET") && a.stem.Contains("NOUN") && (a.suf.Equals("") || a.suf.Equals("ap/N_SUF") || a.suf.Equals("At/N_SUF"))) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.Contains("ADJ") && Analyzed_Texts[Def_ID].def.Equals("INDEF"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.10";
                }
            });
            #endregion

            #region Rule: 2.11
            INDEF_List = Analyzed_Texts.Where(a => (a.stem.Equals("qabol/NOUN") || a.stem.Equals("baEod/NOUN") || a.stem.Equals("bayon/NOUN") || a.stem.Equals("xilAl/NOUN") || a.stem.Equals("Did~/NOUN")) &&
                !a.pr.Contains("Al/DET") && !a.suf.Contains("POSS")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID - 2].stem.Contains("NOUN") && !Analyzed_Texts[Def_ID - 2].pr.Contains("Al/DET") && !Analyzed_Texts[Def_ID - 2].suf.Contains("POSS") && Analyzed_Texts[Def_ID - 2].def.Equals(""))
                {
                    Analyzed_Texts[Def_ID - 2].def = "INDEF";
                    Analyzed_Texts[Def_ID - 2].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.11";
                }
            });
            #endregion

            #region 3.5.2 if the noun is found afetr numbers
            Regex regex = new Regex(@"^\d+$");
            INDEF_List = Analyzed_Texts.Where(a => (regex.IsMatch(a.word))).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.EndsWith("NOUN") && (Analyzed_Texts[Def_ID].pr.Equals("") || Analyzed_Texts[Def_ID].pr.Equals("Al/DET")) && Analyzed_Texts[Def_ID].def.Equals(""))
                {
                    if (Analyzed_Texts[Def_ID - 1].word.Length.Equals(1) || Analyzed_Texts[Def_ID - 1].word.Equals(10))
                    {
                        if (Regex.IsMatch(Analyzed_Texts[Def_ID - 1].word, "^[0-9]$") || Analyzed_Texts[Def_ID - 1].word.Equals(10))
                        {
                            if ((Analyzed_Texts[Def_ID].lemmaID.Equals(">alof") || Analyzed_Texts[Def_ID].lemmaID.Equals("miloyuwn")) && Analyzed_Texts[Def_ID].def.Equals(""))
                            {
                                Analyzed_Texts[Def_ID].def = "EDAFAH";
                                Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.1.1";
                            }
                            else
                            {
                                Analyzed_Texts[Def_ID].def = "INDEF";
                                Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.1.3";
                            }

                            if ((Analyzed_Texts[Def_ID].lemmaID.Equals(">alof") || Analyzed_Texts[Def_ID].lemmaID.Equals("miloyuwn")) && Analyzed_Texts[Def_ID + 1].def.Equals(""))
                            {
                                Analyzed_Texts[Def_ID + 1].def = "INDEF";
                                Analyzed_Texts[Def_ID + 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.1.2";
                            }

                        }
                    }

                    else if (Analyzed_Texts[Def_ID - 1].word.Length > 1 && !Analyzed_Texts[Def_ID - 1].word.Equals("10"))
                    {
                        string last_number_value = Analyzed_Texts[Def_ID - 1].word.Substring(Analyzed_Texts[Def_ID - 1].word.Length - 2);
                        if ((Regex.IsMatch(last_number_value, @"^\d[11-99]$")))
                        {
                            if ((Analyzed_Texts[Def_ID].lemmaID.Equals(">alof") || Analyzed_Texts[Def_ID].lemmaID.Equals("miloyuwn")) && Analyzed_Texts[Def_ID].def.Equals(""))
                            {
                                Analyzed_Texts[Def_ID].def = "EDAFAH";
                                Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2.1.1";
                            }
                            else
                            {
                                Analyzed_Texts[Def_ID].def = "INDEF";
                                Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2.1.3";
                            }

                            if ((Analyzed_Texts[Def_ID].lemmaID.Equals(">alof") || Analyzed_Texts[Def_ID].lemmaID.Equals("miloyuwn")) && Analyzed_Texts[Def_ID + 1].def.Equals(""))
                            {
                                Analyzed_Texts[Def_ID + 1].def = "INDEF";
                                Analyzed_Texts[Def_ID + 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2.1.2";
                            }
                        }
                        else if (last_number_value.Equals("10") || last_number_value.Equals("00"))
                        {
                            if ((Analyzed_Texts[Def_ID].lemmaID.Equals(">alof") || Analyzed_Texts[Def_ID].lemmaID.Equals("miloyuwn")) && Analyzed_Texts[Def_ID].def.Equals(""))
                            {
                                Analyzed_Texts[Def_ID].def = "EDAFAH";
                                Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2.2.1";
                            }
                            else
                            {
                                Analyzed_Texts[Def_ID].def = "INDEF";
                                Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2.2.3";
                            }
                            if ((Analyzed_Texts[Def_ID].lemmaID.Equals(">alof") || Analyzed_Texts[Def_ID].lemmaID.Equals("miloyuwn")) && Analyzed_Texts[Def_ID + 1].def.Equals(""))
                            {
                                Analyzed_Texts[Def_ID + 1].def = "INDEF";
                                Analyzed_Texts[Def_ID + 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2.2.2";
                            }
                        }
                    }
                }
            });
            #endregion

            #region Rule: 2.12
            INDEF_List = Analyzed_Texts.Where(a => (a.spattern.Equals("mafaAE") && (a.lemmaID.EndsWith("Y") || a.lemmaID.EndsWith("yap") || a.lemmaID.EndsWith("Ap")))).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID - 1].stem.Contains("NOUN") && !Analyzed_Texts[Def_ID - 1].pr.Contains("Al/DET") && !Analyzed_Texts[Def_ID - 1].suf.Contains("POSS") && Analyzed_Texts[Def_ID - 1].def.Equals(""))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.12";
                }
            });
            #endregion

            #region Rule: 2.13
            INDEF_List = Analyzed_Texts.Where(a => ((a.spattern.Equals("mufaAE") || a.spattern.EndsWith("faAE")) && (a.stem.EndsWith("/NOUN") || a.stem.EndsWith("ADJ")) && a.lemmaID.EndsWith("iy"))).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Def_ID - 1].pr.Contains("Al/DET") && !Analyzed_Texts[Def_ID - 1].suf.Contains("POSS") && Analyzed_Texts[Def_ID - 1].def.Equals(""))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.13";
                }
            });
            #endregion

            #region Rule: 2.14
            INDEF_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/NOUN") && a.def.Equals(""))).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Def_ID - 1].pr.Contains("Al/DET") && !Analyzed_Texts[Def_ID - 1].suf.Contains("POSS") && (Analyzed_Texts[Def_ID].stem.Equals(">aw/CONJ") || Analyzed_Texts[Def_ID].stem.Equals("bal/CONJ")) && Analyzed_Texts[Def_ID + 1].pr.Contains("/PREP"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.14";
                }
            });
            #endregion

            #region Rule: 2.15
            INDEF_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/ADJ") && a.pr.Equals("") && a.def.Equals(""))).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                    if (Analyzed_Texts[Def_ID - 2].def.Equals("INDEF") && !Analyzed_Texts[Def_ID].pr.Contains("Al/DET") && (Analyzed_Texts[Def_ID + 1].stem.Equals("dAxil/NOUN") || Analyzed_Texts[Def_ID + 1].stem.Equals("xArij/NOUN") || Analyzed_Texts[Def_ID + 1].stem.Equals("bayon/NOUN") ) )
                {
                    Analyzed_Texts[Def_ID].def = "INDEF";
                    Analyzed_Texts[Def_ID].affectedBy = Analyzed_Texts[Def_ID].affectedBy + "_D:2.15";
                }
            });
            #endregion

            #region Rule: 2.16
            INDEF_List = Analyzed_Texts.Where(a => (a.stem.EndsWith("/ADJ") && a.def.Equals(""))).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].def.Equals("INDEF") && Analyzed_Texts[Def_ID].stem.EndsWith("/ADJ") && Analyzed_Texts[Def_ID + 1].stem.Contains("</s>"))
                {
                    Analyzed_Texts[Def_ID - 1 ].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID].affectedBy + "_D:2.16";
                }
            });
            #endregion

            #region Rule: 2.17
            INDEF_List = Analyzed_Texts.Where(a => !a.pr.Contains("Al/DET") && !a.suf.Contains("POSS") && (a.stem.EndsWith("/NOUN") || a.stem.EndsWith("/ADJ")) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if ((Analyzed_Texts[Def_ID + 1].word.Equals(".") || Analyzed_Texts[Def_ID + 1].word.Equals("،") || Analyzed_Texts[Def_ID + 1].stem.Contains("</s>")) && (Analyzed_Texts[Def_ID].pr.Contains("wa/CONJ")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.17";
                }
            });
            #endregion

            #region Rule: 2.18
            INDEF_List = Analyzed_Texts.Where(a => !a.pr.Contains("Al/DET") && !a.suf.Contains("POSS") && (a.stem.EndsWith("/NOUN")) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.Equals("mivol/NOUN") && (Analyzed_Texts[Def_ID + 1].stem.EndsWith("/NOUN")))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.18";
                }
            });
            #endregion

            #region Rule: 2.19
            INDEF_List = Analyzed_Texts.Where(a => !a.pr.Contains("Al/DET") && !a.suf.Contains("POSS") && (a.stem.EndsWith("/NOUN") || a.stem.EndsWith("/ADJ")) && a.def.Equals("")).ToList();
            INDEF_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.StartsWith("<il~A/"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "INDEF";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:2.19";
                }
            });
            #endregion
            #endregion

            #region 3. Set_Analyzed_Text.def as "EDAFAH"
            #region Rule: 3.1
            var EDAFAH_List = Analyzed_Texts.Where(a => ((a.stem.Contains("NOUN") || a.stem.Contains("ADV")) && (a.suf.Contains("POSS"))) && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.1";
            });
            #endregion

            #region Rule: 3.2
            EDAFAH_List = Analyzed_Texts.Where(a => (a.suf.Contains("A/N_SUF") || a.suf.Contains("ayo/N_SUF") || a.suf.Contains("atA/N_SUF") || a.suf.Contains("atayo/N_SUF") || a.suf.Contains("uw/N_SUF") || a.suf.Contains("iy/N_SUF")) && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.2";
            });
            #endregion

            #region Rule: 3.3
            EDAFAH_List = Analyzed_Texts.Where(a => (a.stem.Contains("maE/NOUN") || a.stem.Contains("bayon/NOUN") || a.stem.Contains("Einod/NOUN") || a.stem.Contains("ladaY/NOUN") || a.stem.Contains("xilAl/NOUN") ||
                 a.stem.Contains("Eaqib/NOUN") || a.stem.Contains("naHow/NOUN") || a.stem.Contains("<ivor/NOUN") || a.stem.Contains("qubayol/NOUN") || a.stem.Contains("TawAl/NOUN") ||
                 a.stem.Contains("maTolaE/NOUN") || a.stem.Contains("buEayod/NOUN") || a.stem.Contains("|nA'/NOUN") || a.stem.Contains(">avonA'/NOUN") || a.stem.Contains("dAxil/NOUN") ||
                 a.stem.Contains("taHot/NOUN") || a.stem.Contains("xalof/NOUN") || a.stem.Contains("Hawol/NOUN") || a.stem.Contains("Did~/NOUN") || a.stem.Contains("xArij/NOUN") ||
                 a.stem.Contains(">amAm/NOUN") || a.stem.Contains("Eabor/NOUN") || a.stem.Contains("tijAh/NOUN") || a.stem.Contains("fawoq/NOUN") || a.stem.Contains("wasaT/NOUN") ||
                 a.stem.Contains("HawAlay/NOUN") || a.stem.Contains("Sawob/NOUN") || a.stem.Contains(">asofal/NOUN") || a.stem.Contains("HiyAl/NOUN") || a.stem.Contains("januwb/NOUN") ||
                 a.stem.Contains("$amAl/NOUN") || a.stem.Contains("garob/NOUN") || a.stem.Contains("$aroq/NOUN") || a.stem.Contains("januwbiy~/NOUN") || a.stem.Contains("$aroqiy~/NOUN") ||
                 a.stem.Contains("garobiy~/NOUN") || a.stem.Contains("$amAliy~/NOUN") || a.stem.Contains("Dimon/NOUN") || a.stem.Contains("warA'/NOUN") || a.stem.Contains("<izA'/NOUN") ||
                 a.stem.Contains("duwn/NOUN")) && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.3.1";
            });

            EDAFAH_List = Analyzed_Texts.Where(a => a.stem.Equals("muqAbil/NOUN") && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Def_ID].stem.Equals("lA/PART") && !Analyzed_Texts[Def_ID].stem.Equals(">an/CONJ") && !Analyzed_Texts[Def_ID].word.Equals(".") && !Analyzed_Texts[Def_ID].word.Equals("،"))
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.3.2";
            });

            EDAFAH_List = Analyzed_Texts.Where(a => a.stem.Equals("muno*/NOUN") && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (!Analyzed_Texts[Def_ID].stem.Contains("PV") && !Analyzed_Texts[Def_ID].stem.Equals(">an/CONJ"))
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.3.3";
            });

            EDAFAH_List = Analyzed_Texts.Where(a => a.stem.Equals("baEod/NOUN") && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.Contains("NOUN_PROP") || Analyzed_Texts[Def_ID].stem.Equals(">an/CONJ"))
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.3.4";
            });
            #endregion

            #region Rule: 3.4
            EDAFAH_List = Analyzed_Texts.Where(a => a.stem.Contains("kul~/NOUN") && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (Analyzed_Texts[Def_ID].stem.Equals("mA/PRON") || Analyzed_Texts[Def_ID].stem.Equals("man/PRON"))
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.4";
            });
            #endregion

            #region Rule: 3.5
            EDAFAH_List = Analyzed_Texts.Where(a => a.stem.Contains("NOUN") && !a.pr.Contains("Al/DET") && a.def.Equals("")).ToList();
            EDAFAH_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                if (u.stem.Equals("xAs~/NOUN") && u.suf.Equals("ap/N_SUF"))
                {
                    tag_suf = "xAS~ap/N_SUF";
                }
                else if ((u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (tag_suf.Equals("xAS~ap/N_SUF")) && (Analyzed_Texts[Def_ID].word.Equals("\"") && Analyzed_Texts[Def_ID + 1].pr.StartsWith("Al/DET") && !Analyzed_Texts[Def_ID + 1].stem.Contains("ADJ")))
                {
                    //Def = "EDAFAH";
                    //Surrounds += "/D3.5.1";
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.5.1";
                    tag_suf = "";
                }
                else if ((u.suf.Equals("") || u.suf.Equals("ap/N_SUF") || u.suf.Equals("At/N_SUF")) && (Analyzed_Texts[Def_ID].word.Equals("\"") && Analyzed_Texts[Def_ID + 1].stem.Contains("NOUN_PROP")))
                {
                    //Def = "EDAFAH";
                    //Surrounds += "/D3.5.2";
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.5.2";
                    tag_suf = "";
                }
                else if ((((u.stem.Equals("EAm/NOUN") || u.stem.Equals("Sayof/NOUN")) && (u.suf.Equals(""))) || (u.stem.Equals("san/NOUN") && u.suf.Equals("ap/N_SUF")) && (Regex.IsMatch(Analyzed_Texts[Def_ID].word, @"^[0-9]+$"))))
                {
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.5.3";
                }
                else if (Analyzed_Texts[Def_ID].stem.Equals("man/REL_PRON") && !Analyzed_Texts[Def_ID].pr.Contains("CONJ") && !Analyzed_Texts[Def_ID].pr.Contains("PREP") && Analyzed_Texts[Def_ID - 2].stem.Contains("PREP"))
                {
                    Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                    Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:3.5.4";
                }
            });
            #endregion
            #endregion

            #region 4. Default_Definiteness_Value
            var Default_List = Analyzed_Texts.Where(a => (a.stem.Contains("NOUN") || a.stem.Contains("ADJ") || a.stem.Contains("ADV")) && a.def.Equals("")).ToList();
            Default_List.ToList().ForEach(u =>
            {
                Def_ID = int.Parse(u.ID);
                Analyzed_Texts[Def_ID - 1].def = "EDAFAH";
                Analyzed_Texts[Def_ID - 1].affectedBy = Analyzed_Texts[Def_ID - 1].affectedBy + "_D:Default";
            });

            #endregion
            
            Analyzed_Text = Analyzed_Texts;
        }
    }
}

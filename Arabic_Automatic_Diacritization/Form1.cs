using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Serialization;

namespace Arabic_Automatic_Diacritization
{
    public partial class Form1 : Form
    {
        #region variables and constructors
        Preprocessing_Text Preprocessing_Text;
        Editing_Stage Editing_Stage;
        Running_BAMA_And_Get_Text_Solutions Running_BAMA;
        public List<Text_Solutions> Text_Solutions_List = new List<Text_Solutions>();
        public List<Distinct_Word_For_BAMA> Distinct_Word_For_BAMA = new List<Distinct_Word_For_BAMA>();
        public List<Text_Solutions> Text_Solutions;
        List<Analyzed_Text> Analyzed_Text;
        Morphological_Analysis_Stage Morphological_Analysis_Stage;
        Getting_Arabic_Diacritics_Stage Getting_Arabic_Diacritics_Stage;
        Set_Case_Ending_Stage Set_Case_Ending_Stage;
        Set_Definiteness Set_Definiteness;
        public string Internally_Diacritized_Text = "";
        public string Case_Ending_Diacritized_Text = "";
        string Diac_Word_Num; string Text_Arabic_Words; string Case_Ending_Num; string Need_Case_Ending_Words;
        
        public pos_model Offline_Phases = new pos_model();
        public solutions Diac_Lexicon = new solutions();
        public wforms Word_Lemma_Stem = new wforms();
        public sforms Word_Stem_Suf = new sforms();
        public Transitivity Lemma_Trans = new Transitivity();
        public pref_suf Compatibility = new pref_suf();
        public oovpattern OOV_Pattern = new oovpattern();

        public XmlDocument Diac_Lexicon_Table = new XmlDocument();
        public XmlDocument Offline_Phases_Table = new XmlDocument();
        public XmlDocument Word_Lemma_Stem_Table = new XmlDocument();
        public XmlDocument Word_Stem_Suf_Table = new XmlDocument();
        public XmlDocument Lemma_Trans_Table = new XmlDocument();
        public XmlDocument Compatibility_Table = new XmlDocument();
        public XmlDocument OOV_Pattern_Table = new XmlDocument();
        public DateTime start;
        public DateTime End;
        public enum WdDiacriticColor { wdDiacriticColorBidi };
        #endregion

        public Form1()
        {
            #region Classes_Objects
            Preprocessing_Text = new Preprocessing_Text();
            Editing_Stage = new Editing_Stage();
            Running_BAMA = new Running_BAMA_And_Get_Text_Solutions();
            Morphological_Analysis_Stage = new Morphological_Analysis_Stage();
            Getting_Arabic_Diacritics_Stage = new Getting_Arabic_Diacritics_Stage();
            Set_Definiteness = new Set_Definiteness();
            Set_Case_Ending_Stage = new Set_Case_Ending_Stage();
            #endregion

            #region Loading Diac_Lex_Solutions
            StreamReader Diac_Lexicon_Stream = new StreamReader("Diac_Lex.xml");
            Diac_Lexicon_Table.Load(Diac_Lexicon_Stream);
            XmlSerializer Diac_Lexicon_Serializer = new XmlSerializer(typeof(solutions));
            using (StringReader Diac_Lexicon_Reader = new StringReader(Diac_Lexicon_Table.InnerXml))
            {
                Diac_Lexicon = (solutions)(Diac_Lexicon_Serializer.Deserialize(Diac_Lexicon_Reader));
            }
            Diac_Lexicon_Stream.Close();
            #endregion

            #region Loading_Offline_Phase_Tables
            StreamReader Offline_Phases_Stream = new StreamReader("Offline_Phase.xml");
            Offline_Phases_Table.Load(Offline_Phases_Stream);
            XmlSerializer Offline_Phase_Serializer = new XmlSerializer(typeof(pos_model));
            using (StringReader Offline_Phases_Reader = new StringReader(Offline_Phases_Table.InnerXml))
            {
                Offline_Phases = (pos_model)(Offline_Phase_Serializer.Deserialize(Offline_Phases_Reader));
            }
            Offline_Phases_Stream.Close();
            #endregion

            #region Loading_Word_Lemma_Stem
            StreamReader Word_Lemma_Stem_Stream = new StreamReader("Word_Lemma_Stem.xml");
            Word_Lemma_Stem_Table.Load(Word_Lemma_Stem_Stream);
            XmlSerializer Word_Lemma_Stem_Serializer = new XmlSerializer(typeof(wforms));
            using (StringReader Word_Lemma_Stem_Reader = new StringReader(Word_Lemma_Stem_Table.InnerXml))
            {
                Word_Lemma_Stem = (wforms)(Word_Lemma_Stem_Serializer.Deserialize(Word_Lemma_Stem_Reader));
            }
            Word_Lemma_Stem_Stream.Close();
            #endregion

            #region Loading_Word_Stem_Suf
            StreamReader Word_Stem_Suf_Stream = new StreamReader("Word_Stem_Suf.xml");
            Word_Stem_Suf_Table.Load(Word_Stem_Suf_Stream);
            XmlSerializer Word_Stem_Suf_Serializer = new XmlSerializer(typeof(sforms));
            using (StringReader Word_Stem_Suf_Reader = new StringReader(Word_Stem_Suf_Table.InnerXml))
            {
                Word_Stem_Suf = (sforms)(Word_Stem_Suf_Serializer.Deserialize(Word_Stem_Suf_Reader));
            }
            Word_Stem_Suf_Stream.Close();
            #endregion

            #region Loading_Transitivity
            StreamReader Lemma_Trans_Stream = new StreamReader("Transitivity.xml");
            Lemma_Trans_Table.Load(Lemma_Trans_Stream);
            XmlSerializer Lemma_Trans_Serializer = new XmlSerializer(typeof(Transitivity));
            using (StringReader Lemma_Trans_Reader = new StringReader(Lemma_Trans_Table.InnerXml))
            {
                Lemma_Trans = (Transitivity)(Lemma_Trans_Serializer.Deserialize(Lemma_Trans_Reader));
            }
            Lemma_Trans_Stream.Close();
            #endregion

            #region Loading_Combatibilities
            StreamReader Compatibility_Stream = new StreamReader("Combatibilities.xml");
            Compatibility_Table.Load(Compatibility_Stream);
            XmlSerializer Compatibility_Serializer = new XmlSerializer(typeof(pref_suf));
            using (StringReader Compatibility_Reader = new StringReader(Compatibility_Table.InnerXml))
            {
                Compatibility = (pref_suf)(Compatibility_Serializer.Deserialize(Compatibility_Reader));
            }
            Compatibility_Stream.Close();
            #endregion

            #region OOV_Patterns
            StreamReader OOV_Pattern_Stream = new StreamReader("OOV_Pattern.xml");
            OOV_Pattern_Table.Load(OOV_Pattern_Stream);
            XmlSerializer OOV_Pattern_Serializer = new XmlSerializer(typeof(oovpattern));
            using (StringReader OOV_Pattern_Reader = new StringReader(OOV_Pattern_Table.InnerXml))
            {
                OOV_Pattern = (oovpattern)(OOV_Pattern_Serializer.Deserialize(OOV_Pattern_Reader));
            }
            OOV_Pattern_Stream.Close();
            #endregion
            InitializeComponent();
        }
        private void Button1_Click_1(object sender, EventArgs e)
        {
            Application.DoEvents();
            #region Preparing_Form
            RichTextBox2.Clear();
            label1.Visible = false;
            textBox1.Visible = false;
            label3.Visible = false;
            textBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox1.Visible = false;
            textBox1.Visible = false;
            textBox2.Visible = false;
            textBox3.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = false;
            textBox6.Visible = false;
            textBox8.Visible = false;

            Case_Ending_Diacritized_Text = "";
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox8.Text = "";
            groupBox1.Visible = true;
            #endregion

            #region To Scroll To The First Line Of The RichTextBox1
            RichTextBox1.Select(1, 0);
            RichTextBox1.ScrollToCaret();
            #endregion

            #region Preproccessing_Stage
            start = DateTime.Now;
            string Raw_Text = RichTextBox1.Text;
            string Proccessed_Text;
            Preprocessing_Text.Preproccess_Raw_Text(Raw_Text, out Proccessed_Text);
            End = DateTime.Now;
            textBox4.Visible = true;
            textBox4.Text = Math.Round(End.Subtract(start).TotalMinutes, 2).ToString() + " دقيقة ";
            Application.DoEvents();
            #endregion

            #region Editing_Stage
            string Edited_Text;
            start = DateTime.Now;
            Editing_Stage.Editing_Raw_Text(Proccessed_Text, out Edited_Text, this);
            End = DateTime.Now;
            textBox5.Visible = true;
            textBox5.Text = Math.Round(End.Subtract(start).TotalMinutes, 2).ToString() + " دقيقة ";
            Application.DoEvents();
            #endregion

            #region Running_BAMA_And_Get_Text_Solutions
            start = DateTime.Now;
            Running_BAMA.BAMA_Solutions(Edited_Text, out Text_Solutions, this);
            End = DateTime.Now;
            textBox6.Visible = true;
            textBox6.Text = Math.Round(End.Subtract(start).TotalMinutes, 2).ToString() + " دقيقة ";
            Application.DoEvents();
            #endregion

            #region Morphological_Analysis_Level
            start = DateTime.Now;
            Morphological_Analysis_Stage.Get_Morphologically_Analyzed_Text(Edited_Text, Text_Solutions, out Analyzed_Text, this);
            #endregion

            #region Diacritize_Internal_Diacritics
            Getting_Arabic_Diacritics_Stage.Get_Internal_Diacritics(out Internally_Diacritized_Text, out Text_Arabic_Words, out Diac_Word_Num, Analyzed_Text,this);
            End = DateTime.Now;
            textBox8.Visible = true;
            textBox8.Text = Math.Round(End.Subtract(start).TotalMinutes, 2).ToString() + " دقيقة ";
            Application.DoEvents();
            #endregion

            if (CheckBox1.Checked == true)
            {
                #region Diacritize_Case_Ending_Diacritics
                groupBox3.Visible = true;
                Set_Definiteness.Set_Definiteness_Classifier(Analyzed_Text, out Analyzed_Text);
                Set_Case_Ending_Stage.Get_Case_Ending_Diacritics(Analyzed_Text, out Analyzed_Text, this);
                Getting_Arabic_Diacritics_Stage.Get_Case_Ending_Diacritics(out Case_Ending_Diacritized_Text, out Need_Case_Ending_Words, out Case_Ending_Num, Analyzed_Text, this);
                RichTextBox2.Text = Case_Ending_Diacritized_Text;
                label1.Visible = true; textBox1.Visible = true;
                textBox3.Visible = true;
                textBox3.Text = Need_Case_Ending_Words + "/" + Case_Ending_Num;
                label3.Visible = true; textBox3.Visible = true;

                #endregion
            }
            else
            {
                #region Show_Diacritized_Text
                groupBox3.Visible = true;
                RichTextBox2.Text = Internally_Diacritized_Text;
                #endregion
            }
            #region Preparing_Form  
            //Text_Solutions.Clear();
            label1.Visible = true; textBox1.Visible = true;
            label4.Visible = true; textBox2.Visible = true;
            textBox1.Visible = true;
            textBox2.Visible = true;
            textBox1.Text = Text_Arabic_Words;
            textBox2.Text = Diac_Word_Num;
            #endregion

            #region Adjusting RichTextBox1_Font
            RichTextBox1.Select(0, RichTextBox1.Text.Length);
            RichTextBox1.SelectionFont = new System.Drawing.Font("Simplified Arabic", 18);
            // right to left How?
            RichTextBox1.Select(0, 0);
            RichTextBox1.ScrollToCaret();
            #endregion

            //RichTextBox2.Focus();
            //for (int i = 0; i < RichTextBox2.Text.Length; i++)
            //{
            //    if (RichTextBox2.Text[i].Equals('َ') || RichTextBox2.Text[i].Equals('ً') || RichTextBox2.Text[i].Equals('ُ') || RichTextBox2.Text[i].Equals('ٌ') || RichTextBox2.Text[i].Equals('ِ') || RichTextBox2.Text[i].Equals('ٍ') || RichTextBox2.Text[i].Equals('ْ') || RichTextBox2.Text[i].Equals('ّ'))
            //    {
            //        RichTextBox2.Select(i, 1);
            //        RichTextBox2.SelectionColor = Color.Red;
            //        //WdDiacriticColor Diac_Colr = new WdDiacriticColor();
            //        Diac_Colr.Equals(RichTextBox2.Text);
            //        RichTextBox2.Focus();
            //    }
            //}

            #region To Scroll To The First Line Of The RichTextBox2
            RichTextBox2.Select(1, 0);
            RichTextBox2.ScrollToCaret();
            #endregion
            Text_Solutions.Clear();
        }
        private void CheckBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (CheckBox1.Checked == true)
            {
                if (RichTextBox2.Text != "")
                {
                    if (Case_Ending_Diacritized_Text == "")
                    {
                        #region Diacritize_Case_Ending_Diacritics
                        Set_Definiteness.Set_Definiteness_Classifier(Analyzed_Text, out Analyzed_Text);
                        Set_Case_Ending_Stage.Get_Case_Ending_Diacritics(Analyzed_Text, out Analyzed_Text,this);
                        Getting_Arabic_Diacritics_Stage.Get_Case_Ending_Diacritics(out Case_Ending_Diacritized_Text, out Need_Case_Ending_Words, out Case_Ending_Num, Analyzed_Text, this);
                        RichTextBox2.Text = Case_Ending_Diacritized_Text;
                        label1.Visible = true; textBox3.Visible = true;
                        textBox3.Visible = true;
                        textBox3.Text = Need_Case_Ending_Words + "/" + Case_Ending_Num;
                        label3.Visible = true; textBox2.Visible = true;
                        #endregion
                    }
                    else
                    {
                        if (Case_Ending_Diacritized_Text != "")
                        {
                            RichTextBox2.Text = Case_Ending_Diacritized_Text;
                            label1.Visible = true; textBox1.Visible = true;
                            textBox3.Visible = true;
                            textBox3.Text = Need_Case_Ending_Words + "/" + Case_Ending_Num;
                            label3.Visible = true; textBox3.Visible = true;
                        }
                    }
                }
            }
            else
            {
                if (CheckBox1.Checked == false)
                {
                    if (RichTextBox2.Text != "")
                    {
                        #region Show_Diacritized_Text
                        RichTextBox2.Text = Internally_Diacritized_Text;
                        label1.Visible = true; textBox1.Visible = true;
                        label3.Visible = false; textBox3.Visible = false;
                        #endregion
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            #region Saving_Analyzed_Texts
            string Analyzed_Words_Values;
            Analyzed_Words_Values = "";
            for (int t = 0; t < Analyzed_Text.Count(); t++)
            {
                Analyzed_Text[t].stem = Analyzed_Text[t].stem.Replace("&", "&amp;");
                Analyzed_Text[t].stem = Analyzed_Text[t].stem.Replace("<", "&lt;");
                Analyzed_Text[t].stem = Analyzed_Text[t].stem.Replace(">", "&gt;");

                Analyzed_Text[t].lemmaID = Analyzed_Text[t].lemmaID.Replace("&", "&amp;");
                Analyzed_Text[t].lemmaID = Analyzed_Text[t].lemmaID.Replace("<", "&lt;");
                Analyzed_Text[t].lemmaID = Analyzed_Text[t].lemmaID.Replace(">", "&gt;");

                Analyzed_Text[t].spattern = Analyzed_Text[t].spattern.Replace("&", "&amp;");
                Analyzed_Text[t].spattern = Analyzed_Text[t].spattern.Replace("<", "&lt;");
                Analyzed_Text[t].spattern = Analyzed_Text[t].spattern.Replace(">", "&gt;");

                Analyzed_Words_Values = Analyzed_Words_Values + "\n<solution>" + "\n\t<ID>" + Analyzed_Text[t].ID + "</ID>" + "\n\t<word>" + Analyzed_Text[t].word + "</word>" + "\n\t<lemmaID>" + Analyzed_Text[t].lemmaID + "</lemmaID>" + "\n\t<pr>" + Analyzed_Text[t].pr + "</pr>" + "\n\t<stem>" + Analyzed_Text[t].stem + "</stem>" + "\n\t<suf>" + Analyzed_Text[t].suf + "</suf>" + "\n\t<spattern>" + Analyzed_Text[t].spattern + "</spattern>" + "\n\t<def>" + Analyzed_Text[t].def + "</def>" + "\n\t<ecase>" + Analyzed_Text[t].ecase + "</ecase>" + "\n\t<affectedBy>" + Analyzed_Text[t].affectedBy + "</affectedBy>" + "\n</solution>";
            }
            Analyzed_Words_Values = "<words>" + Analyzed_Words_Values + "\n</words>";
            using (StreamWriter Row_Words = new StreamWriter("Analyzed_Text.xml"))
            {
                Row_Words.WriteLine(Analyzed_Words_Values);
                Row_Words.Close();
            }
            #endregion
        }
    }
}

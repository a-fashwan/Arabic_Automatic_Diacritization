using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arabic_Automatic_Diacritization
{
    class Text_Solutions_And_Analyzed_Text
    {
    }
    public class Text_Solutions
    {
        public string word { set; get; }
        public string lemmaID { set; get; }
        public string stem { set; get; }
        public string pr { set; get; }
        public string suf { set; get; }
        public string spattern { set; get; }
        public string affectedBy { set; get; }
    }
    public class Distinct_Word_For_BAMA
    {
        public string word { set; get; }
    }
    public class Analyzed_Text
    {
        public string ID { set; get; }
        public string word { set; get; }
        public string lemmaID { set; get; }
        public string stem { set; get; }
        public string pr { set; get; }
        public string suf { set; get; }
        public string spattern { set; get; }
        public string def { set; get; }
        public string ecase { set; get; }
        public string affectedBy { set; get; }
    }
}

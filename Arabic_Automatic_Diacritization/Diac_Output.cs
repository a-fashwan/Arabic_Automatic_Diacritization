using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arabic_Automatic_Diacritization
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AraMorph_output
    {

        private AraMorph_outputSolution[] solutionField;

        private string generatedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("solution", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AraMorph_outputSolution[] solution
        {
            get
            {
                return this.solutionField;
            }
            set
            {
                this.solutionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string generated
        {
            get
            {
                return this.generatedField;
            }
            set
            {
                this.generatedField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class AraMorph_outputSolution
    {

        private string wordField;

        private string rootField;

        private string spatternField;

        private string lemmaIDField;

        private string vocField;

        private string prField;

        private string stemField;

        private string sufField;

        private string glossField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string word
        {
            get
            {
                return this.wordField;
            }
            set
            {
                this.wordField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string root
        {
            get
            {
                return this.rootField;
            }
            set
            {
                this.rootField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string spattern
        {
            get
            {
                return this.spatternField;
            }
            set
            {
                this.spatternField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string lemmaID
        {
            get
            {
                return this.lemmaIDField;
            }
            set
            {
                this.lemmaIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string voc
        {
            get
            {
                return this.vocField;
            }
            set
            {
                this.vocField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string pr
        {
            get
            {
                return this.prField;
            }
            set
            {
                this.prField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string stem
        {
            get
            {
                return this.stemField;
            }
            set
            {
                this.stemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string suf
        {
            get
            {
                return this.sufField;
            }
            set
            {
                this.sufField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string gloss
        {
            get
            {
                return this.glossField;
            }
            set
            {
                this.glossField = value;
            }
        }
    }

}

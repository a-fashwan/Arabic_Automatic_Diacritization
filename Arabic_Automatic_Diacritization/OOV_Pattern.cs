﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.8689
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class oovpattern {
    
    private oovpatternOOV_Pattern[] oOV_PatternField;
    
    private string generatedField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("OOV_Pattern", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public oovpatternOOV_Pattern[] OOV_Pattern {
        get {
            return this.oOV_PatternField;
        }
        set {
            this.oOV_PatternField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string generated {
        get {
            return this.generatedField;
        }
        set {
            this.generatedField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class oovpatternOOV_Pattern {
    
    private string oOV_PatternsField;
    
    private string diac_PatternsField;
    
    private string tagsField;
    
    private string countField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string OOV_Patterns {
        get {
            return this.oOV_PatternsField;
        }
        set {
            this.oOV_PatternsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Diac_Patterns {
        get {
            return this.diac_PatternsField;
        }
        set {
            this.diac_PatternsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Tags {
        get {
            return this.tagsField;
        }
        set {
            this.tagsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Count {
        get {
            return this.countField;
        }
        set {
            this.countField = value;
        }
    }
}

/// <remarks/>
//[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
//[System.SerializableAttribute()]
//[System.Diagnostics.DebuggerStepThroughAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
//[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class NewDataSet {
    
    private oovpattern[] itemsField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("oovpattern")]
    public oovpattern[] OOV_Pattern_Items
    {
        get {
            return this.itemsField;
        }
        set {
            this.itemsField = value;
        }
    }
}

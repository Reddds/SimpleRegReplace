//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XmlReplace
{
    using System;
    using System.Collections.Generic;
    
    public partial class is_RegModulesToPages
    {
        public long RegisteredModuleId { get; set; }
        public Nullable<long> PageId { get; set; }
        public Nullable<long> SectionId { get; set; }
        public long Id { get; set; }
        public Nullable<byte> ForMobile { get; set; }
    
        public virtual is_Pages is_Pages { get; set; }
        public virtual is_RegisteredModules is_RegisteredModules { get; set; }
        public virtual is_Sections is_Sections { get; set; }
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WordListPlugin
{
    using System;
    using System.Collections.Generic;
    
    public partial class is_Pages
    {
        public is_Pages()
        {
            this.is_RegModulesToPages = new HashSet<is_RegModulesToPages>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
        public string UrlName { get; set; }
        public Nullable<long> SectionId { get; set; }
        public string Template { get; set; }
        public string PageData { get; set; }
        public int Visible { get; set; }
        public string PageSettings { get; set; }
        public int Order { get; set; }
        public int Version { get; set; }
        public System.DateTime LastChange { get; set; }
        public string FontImage { get; set; }
        public string ImageName { get; set; }
    
        public virtual ICollection<is_RegModulesToPages> is_RegModulesToPages { get; set; }
        public virtual is_Sections is_Sections { get; set; }
    }
}

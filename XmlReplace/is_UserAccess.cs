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
    
    public partial class is_UserAccess
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SectionId { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    
        public virtual is_Sections is_Sections { get; set; }
        public virtual is_userdata is_userdata { get; set; }
    }
}
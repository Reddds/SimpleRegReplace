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
    
    public partial class Roles
    {
        public Roles()
        {
            this.Users = new HashSet<Users>();
        }
    
        public System.Guid ApplicationId { get; set; }
        public System.Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    
        public virtual Applications Applications { get; set; }
        public virtual ICollection<Users> Users { get; set; }
    }
}

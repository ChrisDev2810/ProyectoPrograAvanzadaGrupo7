//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Workspaces_Meeting_Rooms
{
    using System;
    using System.Collections.Generic;
    
    public partial class UsageStatistic
    {
        public int statsID { get; set; }
        public int roomID { get; set; }
        public System.DateTime date { get; set; }
        public decimal hoursBooked { get; set; }
        public int reservationID { get; set; }
    
        public virtual room room { get; set; }
        public virtual reservation reservation { get; set; }
    }
}

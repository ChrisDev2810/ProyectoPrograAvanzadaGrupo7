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
    
    public partial class reservation
    {
        public int reservationID { get; set; }
        public int roomId { get; set; }
        public int userID { get; set; }
        public System.DateTime startTime { get; set; }
        public System.DateTime endtime { get; set; }
        public int statusID { get; set; }
    
        public virtual room room { get; set; }
        public virtual status status { get; set; }
        public virtual user user { get; set; }
    }
}
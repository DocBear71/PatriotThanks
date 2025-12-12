using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class Enumerations
    {
        // Membership access levels for the Patriot Thanks application
        public enum MembershipLevel
        {
            Guest = 10,
            Member = 20,
            Premium = 30,      // Future implementation
            VIP = 40,          // Future implementation
            Admin = 100,
            TestAccess = 999
        }

        public enum AccountStatus
        {
            // Account status states
            Active = 10,
            Inactive = 20,
            Suspended = 30,
            PendingVerify = 40,
            Locked = 50
        }

    }

    public class AccountStatusInfo
    {
        public string AccountStatusID { get; set; }
        public string AccountStatusDesc { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class MembershipLevelInfo
    {
        public string MemLevelID { get; set; }
        public string MemLevelDescription { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

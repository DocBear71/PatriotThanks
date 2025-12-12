using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class User
    {
        public int UserID { get; set; }
        public string TitleID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string StatusID { get; set; }
        public string AccountStatusID { get; set; }
        public string MemLevelID { get; set; }
        public bool Is_Active { get; set; }
        public bool AccountLocked { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsForUpdate { get; set; } = false;
        public bool IsForDelete { get; set; } = false;
        public bool MustUpdatePassword { get; set; }
    }

    public class UserVM : User
    {
        public required List<string> Access { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class Business
    {
        public int BusinessID { get; set; }
        public string BusinessName { get; set; }
        public string BusinessTypeID { get; set; }
        public string BusinessType { get; set; }
        public int? ParentBusinessID { get; set; }
        public int? LocationID { get; set; }
        public string LocationName { get; set; }
        public string Phone { get; set; }
        public string StreetAddress { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateID { get; set; }
        public string StateName { get; set; }
        public string Zip { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Computed property for display purposes
        public string FullAddress
        {
            get
            {
                if (string.IsNullOrEmpty(StreetAddress))
                    return "No address on file";

                var addressParts = new List<string>();

                if (!string.IsNullOrEmpty(StreetAddress))
                    addressParts.Add(StreetAddress);

                if (!string.IsNullOrEmpty(Address2))
                    addressParts.Add(Address2);

                if (!string.IsNullOrEmpty(City))
                    addressParts.Add(City);

                if (!string.IsNullOrEmpty(StateID))
                {
                    if (!string.IsNullOrEmpty(Zip))
                        addressParts.Add(StateID + " " + Zip);
                    else
                        addressParts.Add(StateID);
                }
                else if (!string.IsNullOrEmpty(Zip))
                {
                    addressParts.Add(Zip);
                }

                return string.Join(", ", addressParts);
            }
        }
    }

    public class BusinessSearchCriteria
    {
        public string BusinessName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string StateID { get; set; }
        public string Zip { get; set; }
        public string BusinessTypeID { get; set; }
        public bool IsActive { get; set; } = true;
    }
}









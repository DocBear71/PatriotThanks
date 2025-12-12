using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class IncentiveSearchCriteria
    {
        public int? BusinessID { get; set; }
        public string IncentiveTypeID { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set;}
        public bool ActiveOnly { get; set; } = true;
        public string BusinessName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string StateID { get; set; }
        public string Zip { get; set; }
        public string BusinessTypeID { get; set; }
        public bool IsActive { get; set; } = true;

    }
}

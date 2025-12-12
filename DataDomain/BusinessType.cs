using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class BusinessType
    {
        public string BusTypeID { get; set; }
        public string BusTypeDescription { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

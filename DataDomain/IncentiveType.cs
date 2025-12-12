using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class IncentiveType
    {
        public string IncentiveTypeID { get; set; }
        public string IncentiveTypeDescription { get; set; }
        public int DisplayOrder {  get; set; }
        public bool IsActive { get; set; }
    }
}

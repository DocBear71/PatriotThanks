using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{
    public class Status
    {
        public string StatusID { get; set; }
        public string StatusDescription { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}

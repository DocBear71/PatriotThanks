using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace DataAccessInterfaces
{
    public interface IBusinessAccessor
    {
        List<Business> SearchBusinesses(BusinessSearchCriteria criteria);
        Business SelectBusinessByID(int businessID);
        int UpdateBusiness(Business business);
        int InsertBusiness(Business business);
    }
}

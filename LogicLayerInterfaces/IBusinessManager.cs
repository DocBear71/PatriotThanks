using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace LogicLayerInterfaces
{
    public interface IBusinessManager
    {
        List<Business> SearchBusinesses(BusinessSearchCriteria criteria);
        Business GetBusinessByID(int businessID);
        bool UpdateBusiness(Business business);
        int AddBusiness(Business business);
    }
}

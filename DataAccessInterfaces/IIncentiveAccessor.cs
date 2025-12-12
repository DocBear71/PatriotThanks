using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace DataAccessInterfaces
{
    public interface IIncentiveAccessor
    {
        List<Incentive> SelectIncentivesByBusinessID(int businessID);
        Incentive SelectIncentiveByID(int incentiveID);
        List<Incentive> SearchIncentives(IncentiveSearchCriteria criteria);
        int InsertIncentive(Incentive incentive, List<string> incentiveTypeIDs);
        int UpdateIncentive(Incentive incentive, List<string> incentiveTypeIDs);
    }
}

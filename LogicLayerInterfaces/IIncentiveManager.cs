using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace LogicLayerInterfaces
{
    public interface IIncentiveManager
    {
        List<Incentive> GetIncentivesByBusinessID(int businessID);

        Incentive GetIncentiveByID(int incentiveID);

        List<Incentive> SearchIncentives(IncentiveSearchCriteria criteria);
        int AddIncentive(Incentive incentive, List<string> incentiveTypeIDs);
        bool UpdateIncentive(Incentive incentive, List<string> incentiveTypeIDs);
    }
}

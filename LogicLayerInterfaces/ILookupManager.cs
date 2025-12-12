using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;
using static DataDomain.Enumerations;

namespace LogicLayerInterfaces
{
    public interface ILookupManager
    {
        List<BusinessType> GetAllBusinessTypes();
        List<BusinessType> GetActiveBusinessTypes();
        List<State> GetAllStates();
        List<IncentiveType> GetAllIncentiveTypes();
        List<Title> GetAllTitles();
        List<Status> GetAllStatuses();
        List<AccountStatusInfo> GetAllAccountStatuses();
        List<MembershipLevelInfo> GetAllMembershipLevels();
        List<MembershipLevelInfo> GetActiveMembershipLevels();
    }
}

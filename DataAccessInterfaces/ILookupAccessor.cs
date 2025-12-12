using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace DataAccessInterfaces
{
    public interface ILookupAccessor
    {
        List<BusinessType> SelectAllBusinessTypes();
        List<BusinessType> SelectActiveBusinessTypes();
        List<State> SelectAllStates();
        List<IncentiveType> SelectAllIncentiveTypes();
        List<Title> SelectAllTitles();
        List<Status> SelectAllStatuses();
        List<AccountStatusInfo> SelectAllAccountStatuses();
        List<MembershipLevelInfo> SelectAllMembershipLevels();
        List<MembershipLevelInfo> SelectActiveMembershipLevels();
    }
}

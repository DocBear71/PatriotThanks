using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataAccessLayer;
using DataDomain;
using LogicLayerInterfaces;
using static DataDomain.Enumerations;

namespace LogicLayer
{
    public class LookupManager : ILookupManager
    {
        private ILookupAccessor _lookupAccessor;

        // Dependency inversion/injection
        public LookupManager()
        {
            _lookupAccessor = new LookupAccessor();
        }

        public LookupManager(ILookupAccessor lookupAccessor)
        {
            _lookupAccessor = lookupAccessor;
        }

        public List<BusinessType> GetAllBusinessTypes()
        {
            List<BusinessType> businessTypes = null;

            try
            {
                businessTypes = _lookupAccessor.SelectAllBusinessTypes();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve business types", ex);
            }

            return businessTypes;
        }

        public List<BusinessType> GetActiveBusinessTypes()
        {
            List<BusinessType> businessTypes = null;

            try
            {
                businessTypes = _lookupAccessor.SelectActiveBusinessTypes();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve active business types", ex);
            }

            return businessTypes;
        }

        public List<State> GetAllStates()
        {
            List<State> states = null;

            try
            {
                states = _lookupAccessor.SelectAllStates();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve states", ex);
            }

            return states;
        }

        public List<IncentiveType> GetAllIncentiveTypes()
        {
            List<IncentiveType> incentiveTypes = null;

            try
            {
                incentiveTypes = _lookupAccessor.SelectAllIncentiveTypes();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve incentive types", ex);
            }

            return incentiveTypes;
        }

        public List<Title> GetAllTitles()
        {
            List<Title> titles = null;

            try
            {
                titles = _lookupAccessor.SelectAllTitles();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve titles", ex);
            }

            return titles;
        }

        public List<Status> GetAllStatuses()
        {
            List<Status> statuses = null;

            try
            {
                statuses = _lookupAccessor.SelectAllStatuses();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve statuses", ex);
            }

            return statuses;
        }

        public List<AccountStatusInfo> GetAllAccountStatuses()
        {
            List<AccountStatusInfo> accountStatuses = null;

            try
            {
                accountStatuses = _lookupAccessor.SelectAllAccountStatuses();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve account statuses", ex);
            }

            return accountStatuses;
        }

        public List<MembershipLevelInfo> GetAllMembershipLevels()
        {
            List<MembershipLevelInfo> membershipLevels = null;

            try
            {
                membershipLevels = _lookupAccessor.SelectAllMembershipLevels();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve membership levels", ex);
            }

            return membershipLevels;
        }

        public List<MembershipLevelInfo> GetActiveMembershipLevels()
        {
            List<MembershipLevelInfo> membershipLevels = null;

            try
            {
                membershipLevels = _lookupAccessor.SelectActiveMembershipLevels();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to retrieve active membership levels", ex);
            }

            return membershipLevels;
        }
    }
}



















using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;

namespace DataAccessFakes
{
    public class LookupAccessorFake : ILookupAccessor
    {
        private List<BusinessType> _businessTypes;
        private List<State> _states;
        private List<IncentiveType> _incentiveTypes;
        private List<Title> _titles;
        private List<Status> _statuses;
        private List<AccountStatusInfo> _accountStatuses;
        private List<MembershipLevelInfo> _membershipLevels;
        public bool ShouldThrowException { get; set; } = false;

        public LookupAccessorFake()
        {
            // Initialize business types
            _businessTypes = new List<BusinessType>
            {
                new BusinessType { BusTypeID = "Automotive", BusTypeDescription = "Automotive", DisplayOrder = 1, IsActive = true },
                new BusinessType { BusTypeID = "Restaurant", BusTypeDescription = "Restaurant", DisplayOrder = 2, IsActive = true },
                new BusinessType { BusTypeID = "Grocery", BusTypeDescription = "Grocery Store", DisplayOrder = 3, IsActive = true },
                new BusinessType { BusTypeID = "Retail", BusTypeDescription = "Retail", DisplayOrder = 4, IsActive = true },
                new BusinessType { BusTypeID = "Inactive", BusTypeDescription = "Inactive Type", DisplayOrder = 99, IsActive = false }
            };

            // Initialize states
            _states = new List<State>
            {
                new State { StateID = "IA", StateDescription = "Iowa", DisplayOrder = 15 },
                new State { StateID = "IL", StateDescription = "Illinois", DisplayOrder = 13 },
                new State { StateID = "MN", StateDescription = "Minnesota", DisplayOrder = 23 },
                new State { StateID = "WI", StateDescription = "Wisconsin", DisplayOrder = 49 }
            };

            // Initialize incentive types
            _incentiveTypes = new List<IncentiveType>
            {
                new IncentiveType { IncentiveTypeID = "Veteran", IncentiveTypeDescription = "Veterans special pricing and benefits.", DisplayOrder = 1, IsActive = true },
                new IncentiveType { IncentiveTypeID = "Active Duty", IncentiveTypeDescription = "Active duty military personnel discount.", DisplayOrder = 2, IsActive = true },
                new IncentiveType { IncentiveTypeID = "First Responder", IncentiveTypeDescription = "Discounts for emergency services professionals.", DisplayOrder = 3, IsActive = true },
                new IncentiveType { IncentiveTypeID = "Spouse", IncentiveTypeDescription = "Benefits extended to military/responder spouses.", DisplayOrder = 4, IsActive = true },
                new IncentiveType { IncentiveTypeID = "Other", IncentiveTypeDescription = "Alternative special discounts or promotional offers.", DisplayOrder = 5, IsActive = true }
            };

            // Initialize titles
            _titles = new List<Title>
            {
                new Title { TitleID = "Dr.", TitleDescription = "Doctor", DisplayOrder = 1 },
                new Title { TitleID = "Mr.", TitleDescription = "Mister", DisplayOrder = 2 },
                new Title { TitleID = "Mrs.", TitleDescription = "Mrs.", DisplayOrder = 3 },
                new Title { TitleID = "Ms.", TitleDescription = "Ms.", DisplayOrder = 4 },
                new Title { TitleID = "Miss.", TitleDescription = "Miss", DisplayOrder = 5 },
                new Title { TitleID = "Mx.", TitleDescription = "Mx.", DisplayOrder = 6 },
                new Title { TitleID = "None", TitleDescription = "No Title Chosen", DisplayOrder = 7 }
            };

            // Initialize statuses (user types)
            _statuses = new List<Status>
            {
                new Status { StatusID = "Veteran", StatusDescription = "A veteran of the uniformed services.", DisplayOrder = 1, IsActive = true },
                new Status { StatusID = "Active Duty", StatusDescription = "An active duty member of the uniformed services.", DisplayOrder = 2, IsActive = true },
                new Status { StatusID = "First Responder", StatusDescription = "An active Fire, Police, or Emergency personnel", DisplayOrder = 3, IsActive = true },
                new Status { StatusID = "Spouse", StatusDescription = "A spouse of a veteran, military, or first responder.", DisplayOrder = 4, IsActive = true },
                new Status { StatusID = "Business Owner", StatusDescription = "A business owner", DisplayOrder = 5, IsActive = true },
                new Status { StatusID = "Supporter", StatusDescription = "A person that supports veterans, military, and first responders.", DisplayOrder = 6, IsActive = true }
            };

            // Initialize account statuses
            _accountStatuses = new List<AccountStatusInfo>
            {
                new AccountStatusInfo { AccountStatusID = "Active", AccountStatusDesc = "Active user account", DisplayOrder = 10, IsActive = true },
                new AccountStatusInfo { AccountStatusID = "Inactive", AccountStatusDesc = "Inactive user account", DisplayOrder = 20, IsActive = true },
                new AccountStatusInfo { AccountStatusID = "Suspended", AccountStatusDesc = "Temporarily suspended account", DisplayOrder = 30, IsActive = true },
                new AccountStatusInfo { AccountStatusID = "PendingVerify", AccountStatusDesc = "Pending military/veteran verification", DisplayOrder = 40, IsActive = true },
                new AccountStatusInfo { AccountStatusID = "Locked", AccountStatusDesc = "Account locked due to security reasons", DisplayOrder = 50, IsActive = true }
            };

            // Initialize membership levels
            _membershipLevels = new List<MembershipLevelInfo>
            {
                new MembershipLevelInfo { MemLevelID = "Guest", MemLevelDescription = "Guest Access - Read-only access", DisplayOrder = 10, IsActive = true },
                new MembershipLevelInfo { MemLevelID = "Member", MemLevelDescription = "Verified Member - Full CRUD access", DisplayOrder = 20, IsActive = true },
                new MembershipLevelInfo { MemLevelID = "Premium", MemLevelDescription = "Premium Member - Enhanced features", DisplayOrder = 30, IsActive = false },
                new MembershipLevelInfo { MemLevelID = "VIP", MemLevelDescription = "VIP Member - Unlimited access", DisplayOrder = 40, IsActive = false },
                new MembershipLevelInfo { MemLevelID = "Admin", MemLevelDescription = "Administrator - Full system access", DisplayOrder = 100, IsActive = true },
                new MembershipLevelInfo { MemLevelID = "TestAccess", MemLevelDescription = "Test Access - Beta features", DisplayOrder = 999, IsActive = true }
            };
        }

        public void AddFakeBusinessType(BusinessType businessType)
        {
            _businessTypes.Add(businessType);
        }

        public void AddFakeState(State state)
        {
            _states.Add(state);
        }

        public void AddFakeIncentiveType(IncentiveType incentiveType)
        {
            _incentiveTypes.Add(incentiveType);
        }

        public void AddFakeTitle(Title title)
        {
            _titles.Add(title);
        }

        public void AddFakeStatus(Status status)
        {
            _statuses.Add(status);
        }

        public void AddFakeAccountStatus(AccountStatusInfo accountStatus)
        {
            _accountStatuses.Add(accountStatus);
        }

        public void AddFakeMembershipLevel(MembershipLevelInfo membershipLevel)
        {
            _membershipLevels.Add(membershipLevel);
        }

        public void ClearFakeData()
        {
            _businessTypes.Clear();
            _states.Clear();
            _incentiveTypes.Clear();
            _titles.Clear();
            _statuses.Clear();
            _accountStatuses.Clear();
            _membershipLevels.Clear();
        }

        public List<BusinessType> SelectAllBusinessTypes()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _businessTypes.OrderBy(bt => bt.DisplayOrder).ToList();
        }

        public List<BusinessType> SelectActiveBusinessTypes()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _businessTypes
                .Where(bt => bt.IsActive)
                .OrderBy(bt => bt.DisplayOrder)
                .ToList();
        }

        public List<State> SelectAllStates()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _states.OrderBy(s => s.DisplayOrder).ToList();
        }

        public List<IncentiveType> SelectAllIncentiveTypes()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _incentiveTypes
                .Where(it => it.IsActive)
                .OrderBy(it => it.DisplayOrder)
                .ToList();
        }

        // ========================================
        // NEW METHODS FOR USER MANAGEMENT
        // ========================================

        public List<Title> SelectAllTitles()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _titles.OrderBy(t => t.DisplayOrder).ToList();
        }

        public List<Status> SelectAllStatuses()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _statuses
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToList();
        }

        public List<AccountStatusInfo> SelectAllAccountStatuses()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _accountStatuses
                .Where(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ToList();
        }

        public List<MembershipLevelInfo> SelectAllMembershipLevels()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _membershipLevels.OrderBy(m => m.DisplayOrder).ToList();
        }

        public List<MembershipLevelInfo> SelectActiveMembershipLevels()
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _membershipLevels
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;

namespace DataAccessFakes
{
    public class IncentiveAccessorFake : IIncentiveAccessor
    {
        private List<Incentive> _incentives;
        private Dictionary<int, List<string>> _incentiveTypes;
        public bool ShouldThrowException { get; set; } = false;
        private int _nextIncentiveID = 100;

        public IncentiveAccessorFake()
        {
            _incentives = new List<Incentive>();
            _incentiveTypes = new Dictionary<int, List<string>>();

            _incentives.Add(new Incentive
            {
                IncentiveID = 1,
                BusinessID = 1,
                BusinessName = "Test Restaurant",
                BusinessType = "Restaurant",
                LocationName = "Downtown Location",
                IncentiveAmount = 10.00m,
                IsPercentage = true,
                IncentiveDescription = "10% off entire meal for active duty",
                StartDate = DateTime.Now.AddMonths(-6),
                EndDate = DateTime.Now.AddMonths(6),
                Limitations = "Valid Monday-Thursday only. Dine-in only.",
                IncentiveTypesDisplay = "Active Duty, Veteran",
                CreatedAt = DateTime.Now.AddMonths(-6),
                LastUpdated = DateTime.Now.AddMonths(-6)
            });
            _incentiveTypes[1] = new List<string> { "Active Duty", "Veteran" };

            _incentives.Add(new Incentive
            {
                IncentiveID = 2,
                BusinessID = 1,
                BusinessName = "Test Restaurant",
                BusinessType = "Restaurant",
                LocationName = "Downtown Location",
                IncentiveAmount = 5.00m,
                IsPercentage = false,
                IncentiveDescription = "$5 off appetizers for veterans",
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = null,
                Limitations = "Not valid with other offers.",
                IncentiveTypesDisplay = "Veteran",
                CreatedAt = DateTime.Now.AddMonths(-3),
                LastUpdated = DateTime.Now.AddMonths(-3)
            });
            _incentiveTypes[2] = new List<string> { "Veteran" };

            _incentives.Add(new Incentive
            {
                IncentiveID = 3,
                BusinessID = 2,
                BusinessName = "Test Grocery",
                BusinessType = "Grocery",
                LocationName = "Westside Location",
                IncentiveAmount = 15.00m,
                IsPercentage = true,
                IncentiveDescription = "15% off groceries for military families",
                StartDate = DateTime.Now.AddYears(-1),
                EndDate = DateTime.Now.AddYears(1),
                Limitations = "Must show valid military ID.",
                IncentiveTypesDisplay = "Active Duty, Veteran, Spouse",
                CreatedAt = DateTime.Now.AddYears(-1),
                LastUpdated = DateTime.Now.AddYears(-1)
            });
            _incentiveTypes[3] = new List<string> { "Active Duty", "Veteran", "Spouse" };

            _incentives.Add(new Incentive
            {
                IncentiveID = 4,
                BusinessID = 3,
                BusinessName = "Test Auto Shop",
                BusinessType = "Automotive",
                LocationName = "Main Branch",
                IncentiveAmount = 25.00m,
                IsPercentage = false,
                IncentiveDescription = "$25 off oil change for first responders",
                StartDate = DateTime.Now.AddMonths(-2),
                EndDate = DateTime.Now.AddMonths(10),
                Limitations = "Regular oil change only.",
                IncentiveTypesDisplay = "First Responder",
                CreatedAt = DateTime.Now.AddMonths(-2),
                LastUpdated = DateTime.Now.AddMonths(-2)
            });
            _incentiveTypes[4] = new List<string> { "First Responder" };

            _incentives.Add(new Incentive
            {
                IncentiveID = 5,
                BusinessID = 1,
                BusinessName = "Test Restaurant",
                BusinessType = "Restaurant",
                LocationName = "Downtown Location",
                IncentiveAmount = 20.00m,
                IsPercentage = true,
                IncentiveDescription = "20% off for Veterans Day - EXPIRED",
                StartDate = DateTime.Now.AddMonths(-13),
                EndDate = DateTime.Now.AddMonths(-12),
                Limitations = "Veterans Day only.",
                IncentiveTypesDisplay = "Veteran",
                CreatedAt = DateTime.Now.AddMonths(-13),
                LastUpdated = DateTime.Now.AddMonths(-13)
            });
            _incentiveTypes[5] = new List<string> { "Veteran" };
        }

        public void AddFakeIncentive(Incentive incentive)
        {
            _incentives.Add(incentive);
        }

        public void AddFakeIncentiveTypes(int incentiveID, List<string> types)
        {
            _incentiveTypes[incentiveID] = new List<string>(types);
        }


        public void ClearFakeData()
        {
            _incentives.Clear();
            _incentiveTypes.Clear();
        }

        public List<Incentive> SelectIncentivesByBusinessID(int businessID)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _incentives
                .Where(i => i.BusinessID == businessID)
                .OrderByDescending(i => i.StartDate)
                .ThenByDescending(i => i.IncentiveID)
                .ToList();
        }

        public Incentive SelectIncentiveByID(int incentiveID)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _incentives.FirstOrDefault(i => i.IncentiveID == incentiveID);
        }

        public List<Incentive> SearchIncentives(IncentiveSearchCriteria criteria)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria), "Search criteria cannot be null");
            }

            var results = new List<Incentive>(_incentives);

            // Filter by BusinessID
            if (criteria.BusinessID.HasValue)
            {
                results = results.FindAll(i => i.BusinessID == criteria.BusinessID.Value);
            }

            // Filter by IncentiveTypeID (check if the type is in the display string)
            if (!string.IsNullOrWhiteSpace(criteria.IncentiveTypeID))
            {
                results = results.FindAll(i =>
                    i.IncentiveTypesDisplay != null &&
                    i.IncentiveTypesDisplay.Contains(criteria.IncentiveTypeID));
            }

            // Filter by MinAmount
            if (criteria.MinAmount.HasValue)
            {
                results = results.FindAll(i => i.IncentiveAmount >= criteria.MinAmount.Value);
            }

            // Filter by MaxAmount
            if (criteria.MaxAmount.HasValue)
            {
                results = results.FindAll(i => i.IncentiveAmount <= criteria.MaxAmount.Value);
            }

            // Filter by ActiveOnly
            if (criteria.ActiveOnly)
            {
                DateTime now = DateTime.Now;
                results = results.FindAll(i =>
                    i.StartDate <= now &&
                    (!i.EndDate.HasValue || i.EndDate.Value >= now));
            }

            return results.OrderBy(i => i.BusinessName)
                         .ThenByDescending(i => i.StartDate)
                         .ToList();
        }

        public int InsertIncentive(Incentive incentive, List<string> incentiveTypeIDs)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            if (incentive == null)
            {
                throw new ArgumentNullException(nameof(incentive), "Incentive cannot be null");
            }

            // Validate required fields
            if (incentive.BusinessID <= 0)
            {
                throw new ArgumentException("BusinessID is required");
            }

            if (string.IsNullOrWhiteSpace(incentive.IncentiveDescription))
            {
                throw new ArgumentException("Incentive description is required");
            }

            if (incentive.IncentiveAmount < 0)
            {
                throw new ArgumentException("Incentive amount cannot be negative");
            }

            if (incentiveTypeIDs == null || incentiveTypeIDs.Count == 0)
            {
                throw new ArgumentException("At least one incentive type is required");
            }

            // Assign a new ID
            incentive.IncentiveID = _nextIncentiveID++;
            incentive.CreatedAt = DateTime.Now;
            incentive.LastUpdated = DateTime.Now;

            // Build the IncentiveTypesDisplay string
            incentive.IncentiveTypesDisplay = string.Join(", ", incentiveTypeIDs);

            // Add to the fake data store
            _incentives.Add(incentive);
            _incentiveTypes[incentive.IncentiveID] = new List<string>(incentiveTypeIDs);

            return incentive.IncentiveID;
        }

        public int UpdateIncentive(Incentive incentive, List<string> incentiveTypeIDs)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            if (incentive == null)
            {
                throw new ArgumentNullException(nameof(incentive), "Incentive cannot be null");
            }

            // Find existing incentive
            var existingIncentive = _incentives.FirstOrDefault(i => i.IncentiveID == incentive.IncentiveID);
            if (existingIncentive == null)
            {
                return 0; // No rows affected - incentive not found
            }

            // Validate required fields
            if (incentive.BusinessID <= 0)
            {
                throw new ArgumentException("BusinessID is required");
            }

            if (string.IsNullOrWhiteSpace(incentive.IncentiveDescription))
            {
                throw new ArgumentException("Incentive description is required");
            }

            if (incentive.IncentiveAmount < 0)
            {
                throw new ArgumentException("Incentive amount cannot be negative");
            }

            if (incentiveTypeIDs == null || incentiveTypeIDs.Count == 0)
            {
                throw new ArgumentException("At least one incentive type is required");
            }

            // Update the existing incentive
            existingIncentive.BusinessID = incentive.BusinessID;
            existingIncentive.IncentiveAmount = incentive.IncentiveAmount;
            existingIncentive.IsPercentage = incentive.IsPercentage;
            existingIncentive.IncentiveDescription = incentive.IncentiveDescription;
            existingIncentive.StartDate = incentive.StartDate;
            existingIncentive.EndDate = incentive.EndDate;
            existingIncentive.Limitations = incentive.Limitations;
            existingIncentive.LastUpdated = DateTime.Now;

            // Update the IncentiveTypesDisplay string
            existingIncentive.IncentiveTypesDisplay = string.Join(", ", incentiveTypeIDs);

            // Update the incentive types dictionary
            _incentiveTypes[incentive.IncentiveID] = new List<string>(incentiveTypeIDs);

            return 1; // One row affected
        }

        // Helper method to get incentive types for an incentive (useful for testing)
        public List<string> GetIncentiveTypesForIncentive(int incentiveID)
        {
            if (_incentiveTypes.ContainsKey(incentiveID))
            {
                return _incentiveTypes[incentiveID];
            }
            return new List<string>();
        }
    }
}
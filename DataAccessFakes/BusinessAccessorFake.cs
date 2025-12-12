using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;

namespace DataAccessFakes
{
    public class BusinessAccessorFake : IBusinessAccessor
    {
        private List<Business> _businesses;
        public bool ShouldThrowException { get; set; } = false;
        private int _nextBusinessID = 100;

        public BusinessAccessorFake()
        {
            _businesses = new List<Business>();

            // Add sample test businesses
            _businesses.Add(new Business
            {
                BusinessID = 1,
                BusinessName = "Test Restaurant",
                BusinessTypeID = "Restaurant",
                BusinessType = "Restaurant",
                LocationID = 1,
                LocationName = "Downtown Location",
                Phone = "(319) 555-0101",
                StreetAddress = "123 Main St",
                Address2 = "",
                City = "Cedar Rapids",
                StateID = "IA",
                StateName = "Iowa",
                Zip = "52402",
                CreatedAt = DateTime.Now,
                IsActive = true
            });

            _businesses.Add(new Business
            {
                BusinessID = 2,
                BusinessName = "Test Grocery",
                BusinessTypeID = "Grocery",
                BusinessType = "Grocery Store",
                LocationID = 2,
                LocationName = "Westside Location",
                Phone = "(319) 555-0102",
                StreetAddress = "456 Oak Ave",
                Address2 = "Suite 100",
                City = "Iowa City",
                StateID = "IA",
                StateName = "Iowa",
                Zip = "52240",
                CreatedAt = DateTime.Now,
                IsActive = true
            });

            _businesses.Add(new Business
            {
                BusinessID = 3,
                BusinessName = "Test Auto Shop",
                BusinessTypeID = "Automotive",
                BusinessType = "Automotive",
                LocationID = 3,
                LocationName = "Main Branch",
                Phone = "(319) 555-0103",
                StreetAddress = "789 Elm St",
                Address2 = "",
                City = "Cedar Rapids",
                StateID = "IA",
                StateName = "Iowa",
                Zip = "52404",
                CreatedAt = DateTime.Now,
                IsActive = true
            });

            _businesses.Add(new Business
            {
                BusinessID = 4,
                BusinessName = "Inactive Business",
                BusinessTypeID = "Retail",
                BusinessType = "Retail",
                LocationID = 4,
                LocationName = "Closed Location",
                Phone = "(319) 555-0104",
                StreetAddress = "999 Closed St",
                Address2 = "",
                City = "Cedar Rapids",
                StateID = "IA",
                StateName = "Iowa",
                Zip = "52402",
                CreatedAt = DateTime.Now,
                IsActive = false
            });
        }

        public void AddFakeBusiness(Business business)
        {
            _businesses.Add(business);
        }

        public void ClearFakeData()
        {
            _businesses.Clear();
        }

        public List<Business> SearchBusinesses(BusinessSearchCriteria criteria)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria), "Search criteria cannot be null");
            }

            var results = new List<Business>(_businesses);

            // Filter by IsActive
            results = results.FindAll(b => b.IsActive == criteria.IsActive);

            // Filter by BusinessName (fuzzy match - case insensitive)
            if (!string.IsNullOrWhiteSpace(criteria.BusinessName))
            {
                results = results.FindAll(b =>
                    b.BusinessName != null &&
                    b.BusinessName.IndexOf(criteria.BusinessName, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Filter by City (fuzzy match - case insensitive)
            if (!string.IsNullOrWhiteSpace(criteria.City))
            {
                results = results.FindAll(b =>
                    b.City != null &&
                    b.City.IndexOf(criteria.City, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Filter by StateID (exact match - case insensitive)
            if (!string.IsNullOrWhiteSpace(criteria.StateID))
            {
                results = results.FindAll(b =>
                    b.StateID != null &&
                    b.StateID.Equals(criteria.StateID, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Zip (prefix match)
            if (!string.IsNullOrWhiteSpace(criteria.Zip))
            {
                results = results.FindAll(b =>
                    b.Zip != null &&
                    b.Zip.StartsWith(criteria.Zip));
            }

            // Filter by BusinessTypeID (exact match - case insensitive)
            if (!string.IsNullOrWhiteSpace(criteria.BusinessTypeID))
            {
                results = results.FindAll(b =>
                    b.BusinessTypeID != null &&
                    b.BusinessTypeID.Equals(criteria.BusinessTypeID, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by StreetAddress (fuzzy match - case insensitive)
            if (!string.IsNullOrWhiteSpace(criteria.StreetAddress))
            {
                results = results.FindAll(b =>
                    b.StreetAddress != null &&
                    b.StreetAddress.IndexOf(criteria.StreetAddress, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return results;
        }

        public Business SelectBusinessByID(int businessID)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            return _businesses.FirstOrDefault(b => b.BusinessID == businessID);
        }

        public int UpdateBusiness(Business business)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            if (business == null)
            {
                throw new ArgumentNullException(nameof(business));
            }

            var existingBusiness = _businesses.FirstOrDefault(b => b.BusinessID == business.BusinessID);

            if (existingBusiness == null)
            {
                return 0; // No rows affected
            }

            // Update the business properties
            existingBusiness.BusinessName = business.BusinessName;
            existingBusiness.BusinessTypeID = business.BusinessTypeID;
            existingBusiness.Phone = business.Phone;
            existingBusiness.StreetAddress = business.StreetAddress;
            existingBusiness.Address2 = business.Address2;
            existingBusiness.City = business.City;
            existingBusiness.StateID = business.StateID;
            existingBusiness.Zip = business.Zip;
            existingBusiness.IsActive = business.IsActive;

            return 1; // One row affected
        }

        public int InsertBusiness(Business business)
        {
            if (ShouldThrowException)
            {
                throw new Exception("Simulated database error");
            }

            if (business == null)
            {
                throw new ArgumentNullException(nameof(business));
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(business.BusinessName))
            {
                throw new ArgumentException("Business name is required");
            }

            if (string.IsNullOrWhiteSpace(business.BusinessTypeID))
            {
                throw new ArgumentException("Business type is required");
            }

            if (string.IsNullOrWhiteSpace(business.StreetAddress))
            {
                throw new ArgumentException("Street address is required");
            }

            if (string.IsNullOrWhiteSpace(business.City))
            {
                throw new ArgumentException("City is required");
            }

            if (string.IsNullOrWhiteSpace(business.StateID))
            {
                throw new ArgumentException("State is required");
            }

            // Assign a new ID
            business.BusinessID = _nextBusinessID++;
            business.LocationID = _nextBusinessID;
            business.CreatedAt = DateTime.Now;
            business.IsActive = true;

            // Add to the fake data store
            _businesses.Add(business);

            return business.BusinessID;
        }
    }
}











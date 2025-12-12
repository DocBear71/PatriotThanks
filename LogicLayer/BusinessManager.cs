using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;
using LogicLayerInterfaces;
using DataAccessLayer;



namespace LogicLayer
{
    public class BusinessManager : IBusinessManager
    {
        private IBusinessAccessor _businessAccessor;

        public BusinessManager()
        {
            _businessAccessor = new BusinessAccessor();
        }

        public BusinessManager(IBusinessAccessor businessAccessor)
        {
            _businessAccessor = businessAccessor;
        }

        public Business GetBusinessByID(int businessID)
        {
            Business business = null;

            try
            {
                business = _businessAccessor.SelectBusinessByID(businessID);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving business", ex);
            }

            return business;
        }

        public List<Business> SearchBusinesses(BusinessSearchCriteria criteria)
        {
            List<Business> businesses = null;

            try
            {
                businesses = _businessAccessor.SearchBusinesses(criteria);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error searching businesses", ex);
            }

            return businesses;
        }

        public bool UpdateBusiness(Business business)
        {
            bool success = false;

            try
            {
                if (business == null)
                {
                    throw new ApplicationException("Business cannot be null");
                }

                int rowsAffected = _businessAccessor.UpdateBusiness(business);
                success = rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating business", ex);
            }

            return success;
        }

        public int AddBusiness(Business business)
        {
            int newBusinessID = 0;

            try
            {
                // Validation
                if (business == null)
                {
                    throw new ArgumentNullException(nameof(business), "Business cannot be null");
                }

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

                // Ensure new businesses are active
                business.IsActive = true;

                newBusinessID = _businessAccessor.InsertBusiness(business);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding business", ex);
            }

            return newBusinessID;
        }
    }
}

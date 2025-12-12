using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataAccessLayer;
using DataDomain;
using LogicLayerInterfaces;

namespace LogicLayer
{
    public class IncentiveManager : IIncentiveManager
    {
        private IIncentiveAccessor _incentiveAccessor;

        public IncentiveManager()
        {
            _incentiveAccessor = new IncentiveAccessor();
        }

        public IncentiveManager(IIncentiveAccessor incentiveAccessor)
        {
            _incentiveAccessor = incentiveAccessor;
        }

        public List<Incentive> GetIncentivesByBusinessID(int businessID)
        {
            List<Incentive> incentives = null;

            try
            {
                incentives = _incentiveAccessor.SelectIncentivesByBusinessID(businessID);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving incentives for business", ex);
            }

            return incentives;
        }

        public Incentive GetIncentiveByID(int incentiveID)
        {
            Incentive incentive = null;

            try
            {
                incentive = _incentiveAccessor.SelectIncentiveByID(incentiveID);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving incentive", ex);
            }

            return incentive;
        }
        
        public List<Incentive> SearchIncentives(IncentiveSearchCriteria criteria)
        {
            List<Incentive> incentives = null;

            try
            {
                incentives = _incentiveAccessor.SearchIncentives(criteria);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error searching incentives", ex);
            }

            return incentives;
        }

        public int AddIncentive(Incentive incentive, List<string> incentiveTypeIDs)
        {
            int newIncentiveID = 0;

            try
            {
                // Validation
                if (incentive == null)
                {
                    throw new ArgumentNullException(nameof(incentive), "Incentive cannot be null");
                }

                if (incentive.BusinessID <= 0)
                {
                    throw new ArgumentException("BusinessID is required and must be positive");
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

                // Set default StartDate if not provided
                if (incentive.StartDate == default(DateTime))
                {
                    incentive.StartDate = DateTime.Now;
                }

                newIncentiveID = _incentiveAccessor.InsertIncentive(incentive, incentiveTypeIDs);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error adding incentive", ex);
            }

            return newIncentiveID;
        }

        public bool UpdateIncentive(Incentive incentive, List<string> incentiveTypeIDs)
        {
            bool success = false;

            try
            {
                // Validation
                if (incentive == null)
                {
                    throw new ArgumentNullException(nameof(incentive), "Incentive cannot be null");
                }

                if (incentive.BusinessID <= 0)
                {
                    throw new ArgumentException("BusinessID is required and must be positive");
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

                int rowsAffected = _incentiveAccessor.UpdateIncentive(incentive, incentiveTypeIDs);
                success = rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error updating incentive", ex);
            }

            return success;
        }
    }
}

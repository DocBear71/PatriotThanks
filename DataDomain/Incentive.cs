using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDomain
{

    public class Incentive
    {
        public int IncentiveID { get; set; }
        public int BusinessID { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string LocationName { get; set; }
        public decimal IncentiveAmount { get; set; }
        public bool IsPercentage { get; set; }
        public string IncentiveDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Limitations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public string IncentiveTypesDisplay { get; set; }

        public string FormattedAmount
        {
            get
            {
                if (IsPercentage)
                {
                    return IncentiveAmount.ToString("0.##") + "%";
                }
                else
                {
                    return IncentiveAmount.ToString("C");
                }
            }
        }

        public string DateRangeDisplay
        {
            get
            {
                string start = StartDate.ToString("M/d/yyyy");
                string end = EndDate.HasValue ? EndDate.Value.ToString("M/d/yyyy") : "Ongoing";
                return $"{start} - {end}";
            }
        }

        public bool IsCurrentlyActive
        {
            get
            {
                DateTime now = DateTime.Now;
                return StartDate <= now && (!EndDate.HasValue || EndDate.Value >= now);
            }
        }
    }



    public class IncentiveVM : Incentive
    {
        
        public List<string> IncentiveTypes { get; set; }

    }
}
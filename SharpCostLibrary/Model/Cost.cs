using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCostLibrary.Model
{
    public class Cost
    {
        // Unique id
        public string CostId { get; set; }

        public Month Month { get; set; }

        // E.g. AWS or Azure
        public string Service { get; set; }

        // Resource id
        public string ResourceId { get; set; }

        // Cost Center id
        public string CostCenterId { get; set; }

        public decimal Amount { get; set; }

        public decimal Total { get; set; }

        public decimal Adjustment { get; set; }

        public decimal ExchangedAmount { get; set; }

        // Original currency
        public string Currency { get; set; }

        // Total currency
        public string TotalCurrency { get; set; }

        public decimal? ExchangeRate { get; set; }

        // Cost name is used to match the cost to a resource 
        public string Name { get; set; }

        public void Calculate()
        {
            if (ExchangeRate == null) ExchangeRate = 1;

            ExchangedAmount = (decimal)(Amount * ExchangeRate);

            Total = Adjustment * ExchangedAmount;
        }
    }
}

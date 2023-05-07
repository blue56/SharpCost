using Amazon;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using Amazon.Organizations;
using SharpCostLibrary.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCostLibrary.Providers
{
    public class AWSCostExplorerProvider : Provider
    {
        public string ProviderId { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string Metric { get; set; }
        public bool Tax { get; set; }
        public string CostCategoryName { get; set; }

        public override string Name
        {
            get
            {
                return "AWS Cost Explorer Provider";
            }
        }

        public override string GetServiceId()
        {
            return "AWS";
        }

        public override Cost[] Execute(Month Month)
        {
            List<GroupDefinition> _groupDefinitions = new List<GroupDefinition>();

            GroupDefinition groupDefinition1 = new GroupDefinition();
            groupDefinition1.Type = "COST_CATEGORY";
            groupDefinition1.Key = CostCategoryName;
            _groupDefinitions.Add(groupDefinition1);

            string groupBy2Type = "DIMENSION";
            string groupBy2Key = "LINKED_ACCOUNT";

            GroupDefinition groupDefinition2 = new GroupDefinition();
            groupDefinition2.Type = groupBy2Type;
            groupDefinition2.Key = groupBy2Key;

            _groupDefinitions.Add(groupDefinition2);

            RegionEndpoint _region = RegionEndpoint.GetBySystemName(Region);

            Amazon.Runtime.BasicAWSCredentials credentials =
                new Amazon.Runtime.BasicAWSCredentials(AccessKey, SecretKey);

            // Call AWS Organization
            AmazonOrganizationsClient amazonOrganizationsClient =
                new AmazonOrganizationsClient(credentials, _region);

            Amazon.Organizations.Model.ListAccountsRequest listAccountsRequest
                = new Amazon.Organizations.Model.ListAccountsRequest();

            var accountsResponse = amazonOrganizationsClient.ListAccountsAsync(listAccountsRequest).Result;

            var accounts = accountsResponse.Accounts;

            // Start Cost Explorer

            AmazonCostExplorerClient amazonCostExplorerClient
                = new AmazonCostExplorerClient(credentials, _region);

            GetCostAndUsageRequest getCostAndUsageRequest = new GetCostAndUsageRequest();

            getCostAndUsageRequest.Granularity = Granularity.MONTHLY;

            Amazon.CostExplorer.Model.DateInterval dateInterval = new Amazon.CostExplorer.Model.DateInterval();

            // e.g. "2022-12-12"
            string startDate = Month.Year + "-" + Month.MonthNumber.ToString("D2") + "-01";

            dateInterval.Start = startDate;

            int endMonth = (Month.MonthNumber + 1) % 12;

            if (endMonth == 0)
            {
                endMonth = 12;
            }

            int endYear = Month.Year;
            if (Month.MonthNumber == 12)
            {
                endYear = Month.Year + 1;
            }

            string endDate = endYear + "-" + endMonth.ToString("D2") + "-01";

            dateInterval.End = endDate;

            getCostAndUsageRequest.TimePeriod = dateInterval;

            getCostAndUsageRequest.Metrics.Add(Metric);

            getCostAndUsageRequest.GroupBy.Clear();
            getCostAndUsageRequest.GroupBy.AddRange(_groupDefinitions);

            // Tax
            if (Tax == false)
            {
                var taxExpression = new Amazon.CostExplorer.Model.Expression();
                taxExpression.Dimensions = new DimensionValues();
                taxExpression.Dimensions.Key = Amazon.CostExplorer.Dimension.SERVICE;
                taxExpression.Dimensions.Values = new List<string>();
                taxExpression.Dimensions.Values.Add("Tax");

                var notTaxExpression = new Amazon.CostExplorer.Model.Expression();
                notTaxExpression.Not = taxExpression;

                getCostAndUsageRequest.Filter = notTaxExpression;
            }

            var f = amazonCostExplorerClient.GetCostAndUsageAsync(getCostAndUsageRequest).Result;

            var usCulture = new CultureInfo("en-US");

            List<Cost> costList = new List<Cost>();

            foreach (var item in f.ResultsByTime[0].Groups)
            {
                var costCategoryId = item.Keys[0];
                var accountId = item.Keys[1];
                var amountStr = item.Metrics.First().Value.Amount;

                var account = accounts.FirstOrDefault(x => x.Id == accountId);

                Cost cost = new Cost();
                cost.CostId = Guid.NewGuid().ToString();
                cost.Amount = decimal.Parse(amountStr, usCulture);

                cost.Amount = System.Math.Round(cost.Amount, 2);

                cost.Adjustment = 1;
                cost.ExchangeRate = 1;
                cost.ResourceId = accountId;
                cost.Name = account.Name + " (" + account.Id + ")";
                cost.Currency = item.Metrics.First().Value.Unit;
                cost.Service = "AWS";
                cost.Month = Month;

                costList.Add(cost);
            }

            return costList.ToArray();
        }
    }
}

﻿using SharpCostLibrary.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCostLibrary.Model
{
    public class Month
    {
        public string Id { get; set; }
        public int MonthNumber { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
        public string LastUpdated { get; set; }

        private List<Provider> _providers;

        public string MonthName
        {
            get
            {
                DateTime monthDateTime = new DateTime(Year, MonthNumber, 1);

                string a = monthDateTime.ToString("yyyy MMMM", CultureInfo.CreateSpecificCulture("en-US"));
                return a;
            }
        }

        public string MonthShortName
        {
            get
            {
                DateTime monthDateTime = new DateTime(Year, MonthNumber, 1);

                string a = monthDateTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("en-US"));
                return a;
            }
        }

        public Cost[] GetCost()
        {
            List<Cost> costList = new List<Cost>();

            foreach (var provider in _providers)
            {
                var cost = provider.Execute(this);

                costList.AddRange(cost);
            }

            return costList.ToArray();
        }
    }
}

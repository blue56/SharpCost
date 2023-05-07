using SharpCostLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpCostLibrary.Providers
{
    public class Provider
    {
        public string ProviderId { get; set; }
        public string ProviderType { get; set; }
        public virtual string Name { get; }

        public virtual Cost[] Execute(Month Month)
        {
            return null;
        }

        public virtual string GetServiceId()
        {
            return null;
        }
    }
}

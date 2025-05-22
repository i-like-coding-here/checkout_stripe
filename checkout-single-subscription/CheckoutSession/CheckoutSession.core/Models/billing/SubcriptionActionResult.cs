using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckoutSession.core.Models.dbOp;

namespace CheckoutSession.core.Models.billing
{
    public class SubscriptionDecision
    {
        public bool IsDuplicate { get; set; }
        public bool IsEnterpriseUpgrade { get; set; }
        public Tenant Tenant { get; set; }
        public string Message { get; set; }
    }

}

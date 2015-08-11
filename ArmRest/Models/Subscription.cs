using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmRest.Models
{
    public class SubscriptionPolicies
    {
        public string locationPlacementId { get; set; }
        public string quotaId { get; set; }
    }

    public class Subscription
    {
        public string id { get; set; }
        public string subscriptionId { get; set; }
        public string displayName { get; set; }
        public string state { get; set; }
        public SubscriptionPolicies subscriptionPolicies { get; set; }
    }

    public class Subscriptions
    {
        public List<Subscription> value { get; set; }
    }
}
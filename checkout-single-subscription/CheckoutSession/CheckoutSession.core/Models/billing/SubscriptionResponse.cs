using Stripe;
using Newtonsoft.Json;


namespace CheckoutSession.core.Models.billing
{
    public class SubscriptionResponse
    {
        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }
    }

}
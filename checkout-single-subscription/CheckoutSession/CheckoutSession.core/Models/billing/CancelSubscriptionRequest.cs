using Newtonsoft.Json;

namespace CheckoutSession.core.Models.billing
{
    public class CancelSubscriptionRequest
    {
        [JsonProperty("subscriptionId")]
        public string Subscription { get; set; }
    }
}
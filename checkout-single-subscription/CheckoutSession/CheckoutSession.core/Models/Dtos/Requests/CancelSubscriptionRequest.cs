using Newtonsoft.Json;

namespace CheckoutSession.core.Models.Dtos.Requests
{
    public class CancelSubscriptionRequest
    {
        [JsonProperty("subscriptionId")]
        public string Subscription { get; set; }
    }
}
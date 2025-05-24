using Stripe;
using Newtonsoft.Json;


namespace CheckoutSession.core.Models.Dtos.Responses
{
    public class SubscriptionCreateResponse
    {
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
    }
}

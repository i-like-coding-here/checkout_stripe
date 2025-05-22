using Stripe;
using Newtonsoft.Json;


namespace CheckoutSession.core.Models.billing
{
    public class SubscriptionCreateResponse
    {
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        // [JsonProperty("checkoutSession")]
        // public string CheckoutSessionId { get; set; }

    }
}

using Newtonsoft.Json;
using Stripe;

namespace CheckoutSession.core.Models.Dtos.Responses
{
    public class SubscriptionsResponse
    {
        [JsonProperty("subscriptions")]
        public StripeList<Subscription> Subscriptions { get; set; }
    }
}

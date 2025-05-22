using Newtonsoft.Json;
using Stripe;

namespace CheckoutSession.core.Models.billing
{
    public class CreateCustomerResponse
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
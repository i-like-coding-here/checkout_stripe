using Stripe;
using Newtonsoft.Json;

namespace CheckoutSession.core.Models.Dtos.Responses
{
    public class InvoiceResponse
    {
        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }
    }

}
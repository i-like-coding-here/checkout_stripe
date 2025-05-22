using Stripe;
using Newtonsoft.Json;

namespace CheckoutSession.core.Models.billing
{
    public class InvoiceResponse
    {
        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }
    }

}
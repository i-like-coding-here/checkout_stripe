using Newtonsoft.Json;

namespace CheckoutSession.core.Models.Dtos.Requests
{
    public class PriceRequest
    {
        [JsonProperty("priceId")]
       public string PriceId { get; set; }
    }
}

using Newtonsoft.Json;
using Stripe;
using System.Collections.Generic;

namespace CheckoutSession.core.Models.billing
{
    public class ConfigResponse
    {
        [JsonProperty("publishableKey")]
        public string PublishableKey { get; set; }

        [JsonProperty("prices")]
        public List<Price> Prices { get; set; }

        //[JsonProperty("proPrice")]
        //public string ProPrice { get; set; }

        //[JsonProperty("basicPrice")]
        //public string BasicPrice { get; set; }

        //[JsonProperty("description")]
        //public string? Description { get; set; }
    }

}
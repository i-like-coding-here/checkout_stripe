using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CheckoutSession.core.Models.billing
{
    public class NewConfigResponse
    {
        [JsonProperty("publishableKey")]
        public string PublishableKey { get; set; }

        [JsonProperty("proPrice")]
        public string ProPrice { get; set; }

        [JsonProperty("basicPrice")]
        public string BasicPrice { get; set; }
    }
}

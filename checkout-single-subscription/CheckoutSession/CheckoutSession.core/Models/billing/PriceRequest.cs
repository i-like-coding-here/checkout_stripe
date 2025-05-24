using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CheckoutSession.core.Models.billing
{
    public class PriceRequest
    {
        //public string Tenantid { get; set; }
        [JsonProperty("priceId")]
       public string PriceId { get; set; }
    }
}

﻿using Newtonsoft.Json;

namespace CheckoutSession.core.Models.Dtos.Requests
{
    public class UpdateSubscriptionRequest
    {
        [JsonProperty("subscriptionId")]
        public string Subscription { get; set; }

        [JsonProperty("newPriceLookupKey")]
        public string NewPrice { get; set; }
    }
}

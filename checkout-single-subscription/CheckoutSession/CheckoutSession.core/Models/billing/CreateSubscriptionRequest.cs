﻿using Newtonsoft.Json;

namespace CheckoutSession.core.Models.billing
{
    public class CreateSubscriptionRequest
    {
        [JsonProperty("priceId")]
        public string PriceId { get; set; }

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("tenantId")]
        public Guid TenantId { get; set; }
    }
}
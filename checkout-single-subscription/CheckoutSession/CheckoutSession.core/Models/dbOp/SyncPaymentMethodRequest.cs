using Newtonsoft.Json;

namespace CheckoutSession.core.Models.dbOp
{
    public class SyncPaymentMethodRequest
    {
        [JsonProperty("tenantId")]
        public Guid TenantId { get; set; }

        [JsonProperty("paymentIntentId")]
        public string PaymentIntentId { get; set; }
    }
}
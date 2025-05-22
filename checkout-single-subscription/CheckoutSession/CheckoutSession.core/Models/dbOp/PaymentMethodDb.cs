namespace CheckoutSession.core.Models.dbOp
{
    public class PaymentMethodDb
    {
        public int Id { get; set; }
        public string StripePaymentMethodId { get; set; }

        public string Brand { get; set; }
        public string Last4 { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }

}
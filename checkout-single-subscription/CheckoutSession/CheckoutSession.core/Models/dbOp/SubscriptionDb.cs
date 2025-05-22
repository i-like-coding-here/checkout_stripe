namespace CheckoutSession.core.Models.dbOp
{
    public class SubscriptionDb
    {
        public int Id { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string PlanId { get; set; }

        public string Status { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsApproved { get; set; }
        public bool CancelAtPeriodEnd { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }

}
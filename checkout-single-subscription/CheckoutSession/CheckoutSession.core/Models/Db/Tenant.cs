using Stripe;
using Newtonsoft;
using Newtonsoft.Json;

namespace CheckoutSession.core.Models.Db
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public string StripeCustomerId { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public ICollection<Subscription> Subscription { get; set; }
        public ICollection<PaymentMethod> PaymentMethod { get; set; }
    }


}
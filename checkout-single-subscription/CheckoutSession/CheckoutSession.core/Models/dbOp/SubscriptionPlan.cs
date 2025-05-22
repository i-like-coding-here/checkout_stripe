using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutSession.core.Models.dbOp
{
    public class SubscriptionPlan
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LookupKey { get; set; }

        [Required]
        public int Amount { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Currency { get; set; } = "inr";

        [Required]
        public string Interval { get; set; } = "month";

        public string? StripeProductId { get; set; }

        public string? StripePriceId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

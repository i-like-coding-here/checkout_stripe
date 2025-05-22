using Microsoft.EntityFrameworkCore;
using CheckoutSession.core.Models.dbOp;
using Stripe;

namespace CheckoutSession.core.Data
{
    public class CheckoutSessionDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<SubscriptionDb> Subscriptions { get; set; }
        public DbSet<PaymentMethodDb> PaymentMethodDb { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

        public CheckoutSessionDbContext(DbContextOptions<CheckoutSessionDbContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder )
        {
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.Subscriptions)
                .WithOne(s => s.Tenant)
                .HasForeignKey("TenantId");
            //.HasForeignKey(t => t.TenantId);

            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.PaymentMethodDb)
                .WithOne(s => s.Tenant)
                .HasForeignKey("TenantId");
        }
    }

}
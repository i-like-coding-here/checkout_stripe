using CheckoutSession.core.Models.Db;
using Microsoft.EntityFrameworkCore;
//using PaymentMethod = CheckoutSession.core.Models.Db.PaymentMethod;

namespace CheckoutSession.core.Data
{
    public class CheckoutSessionDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Models.Db.Subscription> Subscriptions { get; set; }
        public DbSet<Models.Db.PaymentMethod> PaymentMethodDb { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

        public CheckoutSessionDbContext(DbContextOptions<CheckoutSessionDbContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder )
        {
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.Subscription)
                .WithOne(s => s.Tenant)
                .HasForeignKey("TenantId");
            //.HasForeignKey(t => t.TenantId);

            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.PaymentMethod)
                .WithOne(s => s.Tenant)
                .HasForeignKey("TenantId");
        }
    }

}
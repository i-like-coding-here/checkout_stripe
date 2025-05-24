using Microsoft.Extensions.Options;
using CheckoutSession.core.Interfaces;
using CheckoutSession.core.Models.billing;
using CheckoutSession.core.Configuration;
using Stripe;
using CheckoutSession.core.Models.dbOp;
using CheckoutSession.core.Data;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace CheckoutSession.core.Services
{
    public class CheckoutSessionService : ICheckoutSessionService
    {
        private readonly StripeOptions _options;
        private readonly CheckoutSessionDbContext _dbContext;
        private readonly IStripeClient _client;

        public CheckoutSessionService(IOptions<StripeOptions> optionConfig, CheckoutSessionDbContext context)
        {
            _options = optionConfig.Value;
            _dbContext = context;
            _client = new StripeClient(_options.SecretKey);
        }

        public ConfigResponse GetConfig()
        {
            var priceService = new PriceService();
            var prices = priceService.List(new PriceListOptions
            {
                LookupKeys = new List<string> { "help_desk_pro", "project_pro", "enterprise" }
            });

            return new ConfigResponse
            {
                PublishableKey = _options.PublishableKey,
                Prices = prices.Data
            };
        }

        public async Task<bool> CanSubscribeToPlan(string customerId, string requestedPriceId)
        {
            var priceService = new PriceService();
            var requestedPrice = await priceService.GetAsync(requestedPriceId);
            var requestedLookupKey = requestedPrice.LookupKey;

            var subscriptionService = new SubscriptionService(_client);
            var activeSubs = await subscriptionService.ListAsync(new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "active"
            });

            bool hasEnterprise = false;
            bool requestedIsEnterprise = requestedLookupKey == "enterprise";

            foreach (var sub in activeSubs)
            {
                foreach (var item in sub.Items.Data)
                {
                    var existingKey = item.Price.LookupKey;

                    if (existingKey == "enterprise")
                        hasEnterprise = true;

                    if (existingKey == requestedLookupKey)
                        return false;
                }
            }

            if (hasEnterprise && !requestedIsEnterprise)
            {
                return true;
            }

            if (hasEnterprise && requestedIsEnterprise)
            {
                return false;
            }

            return true;
        }

        public async Task<string> CreateCheckoutSession(PriceRequest req)
        {
            var customerId = "cus_SMCCo7Si4WiZwO";

            var requestedPrice = await new PriceService().GetAsync(req.PriceId);
            var requestedLookupKey = requestedPrice.LookupKey;

            var subscriptionService = new SubscriptionService(_client);
            var activeSubs = await subscriptionService.ListAsync(new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "active"
            });

            bool hasEnterprise = activeSubs.Any(sub =>
                sub.Items.Data.Any(item => item.Price.LookupKey == "enterprise"));

            var isEnterprise = requestedLookupKey == "enterprise";

            if (!await CanSubscribeToPlan(customerId, req.PriceId))
                throw new InvalidOperationException("You cannot subscribe to this plan based on your current subscriptions.");

            if (isEnterprise)
            {
                foreach (var sub in activeSubs)
                {
                    bool isSubEnterprise = sub.Items.Data.Any(item => item.Price.LookupKey == "enterprise");
                    if (!isSubEnterprise)
                    {
                        await subscriptionService.CancelAsync(sub.Id);
                    }
                }
            }
            else if (hasEnterprise && !isEnterprise)
            {
                foreach (var sub in activeSubs)
                {
                    bool isSubEnterprise = sub.Items.Data.Any(item => item.Price.LookupKey == "enterprise");
                    if (isSubEnterprise)
                    {
                        await subscriptionService.CancelAsync(sub.Id);
                    }
                }
            }

            var sessionOptions = new SessionCreateOptions
            {
                SuccessUrl = $"{_options.Domain}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_options.Domain}/canceled",
                Mode = "subscription",
                Customer = customerId,
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = req.PriceId,
                Quantity = 1,
            },
        },
                BillingAddressCollection = "required"
            };

            var sessionService = new SessionService(_client);
            var session = await sessionService.CreateAsync(sessionOptions);
            return session.Url;
        }


        public async Task<CheckoutSessionDto> CheckoutSession(string sessionId)
        {
            var service = new SessionService(_client);
            var session = await service.GetAsync(sessionId);

            return new CheckoutSessionDto
            {
                Id = session.Id,
                CustomerId = session.CustomerId,
                PaymentStatus = session.PaymentStatus,
            };
        }

        public async Task SyncPlansToStripe()
        {
            var plans = await _dbContext.SubscriptionPlans.Where(p => string.IsNullOrEmpty(p.StripeProductId) || string.IsNullOrEmpty(p.StripePriceId)).ToListAsync();

            foreach (var plan in plans)
            {
                var productOptions = new ProductCreateOptions
                {
                    Name = plan.Name,
                    Description = plan.Description
                };
                var productService = new ProductService();
                var product = await productService.CreateAsync(productOptions);

                var priceOptions = new PriceCreateOptions
                {
                    Product = product.Id,
                    UnitAmount = plan.Amount,
                    Currency = plan.Currency,
                    LookupKey = plan.LookupKey,
                    Recurring = new PriceRecurringOptions { Interval = plan.Interval }
                };

                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);

                plan.StripeProductId = product.Id;
                plan.StripePriceId = price.Id;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> GetTenantIdByStripeCustomerId(string stripeCustomerId)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == stripeCustomerId);

            if (tenant == null)
            {
                throw new Exception($"Tenant not found for Stripe CustomerId {stripeCustomerId}");
            }

            return tenant.TenantId;
        }

        public async Task<SubscriptionResponse> UpdateSubscription(UpdateSubscriptionRequest req)
        {
            var subscription = new SubscriptionService().Get( req.Subscription);

            var stripeSubscriptionId = req.Subscription;

            var subscriptionFromDb = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);


            //var newPriceId = Environment.GetEnvironmentVariable(req.NewPrice.ToUpper());
            var newPriceId = req.NewPrice;

            var updated = new SubscriptionService().Update(req.Subscription, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                Items = new List<SubscriptionItemOptions>
                {   
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items?.Data[0]?.Id,
                        Price = newPriceId
                    }
                }
            });

            if (subscription != null)
            {
                subscriptionFromDb.UpdatedAt = DateTime.UtcNow;
                subscriptionFromDb.PlanId = newPriceId;
                await _dbContext.SaveChangesAsync();
            }

            return new SubscriptionResponse { Subscription = updated };
        }

        public async Task<SubscriptionResponse> CancelSubscription(string subscriptionId)
        {
            var subscription = new SubscriptionService().Cancel(subscriptionId);

            var subscriptionFromDb = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);
            if (subscriptionFromDb != null)
            {
                subscription.Status = "canceled";
                subscriptionFromDb.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return new SubscriptionResponse { Subscription = subscription };
        }

        public async Task<SubscriptionResponse> PauseSubscription(string subscriptionId)
        {
            var updated = new SubscriptionService().Update(subscriptionId, new SubscriptionUpdateOptions
            {
                PauseCollection = new SubscriptionPauseCollectionOptions
                {
                    Behavior = "mark_uncollectible"
                }
            });

            var subscriptionFromDb = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);

            if (subscriptionFromDb != null)
            {
                subscriptionFromDb.Status = "paused";
                subscriptionFromDb.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return new SubscriptionResponse { Subscription = updated };
        }

        public async Task<SubscriptionResponse> ResumeSubscription(string subscriptionId)
        {

            var options = new SubscriptionResumeOptions
            {
                BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now,
            };

            var subscription = new SubscriptionService().Resume(subscriptionId, options);

            var subscriptionFromDb = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscriptionId);
            if (subscriptionFromDb != null)
            {
                subscription.Status = "active";
                subscriptionFromDb.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return new SubscriptionResponse { Subscription = subscription };
        }

        public SubscriptionsResponse ListSubscriptions(string customerId, Guid tenantId)
        {
            var tenant = _dbContext.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null || tenant.StripeCustomerId != customerId)
            {
                throw new UnauthorizedAccessException("Customer does not belong to tenant.");
            }

            var subscriptions = new SubscriptionService().List(new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "all"
            });

            return new SubscriptionsResponse { Subscriptions = subscriptions };
        }

        public async Task<string> CustomerPortal(string sessionId)
        {
            var checkoutService = new SessionService(_client);
            var checkoutSession = await checkoutService.GetAsync(sessionId);

            var returnUrl = _options.Domain;

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = checkoutSession.CustomerId,
                ReturnUrl = returnUrl,
            };

            var service = new Stripe.BillingPortal.SessionService(_client);
            var session = await service.CreateAsync(options);

            return session.Url;
        }

        public async Task HandleWebhook(string json, string signatureHeader)
        {
            Event stripeEvent;

            try
            {
                Console.WriteLine("Stripe webhook received:");
                Console.WriteLine(json);

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _options.WebhookSecret);

                Console.WriteLine($"Event type: {stripeEvent.Type}");
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Webhook verification failed: {e.Message}");
                throw new ApplicationException("Webhook signature verification failed.", e);
            }

            switch (stripeEvent.Type)
            {
                case "invoice.created":
                    var invoice = stripeEvent.Data.Object as Invoice;
                    if (invoice == null) break;

                    var stripeCustomerId = invoice.CustomerId;
                    if (string.IsNullOrEmpty(stripeCustomerId))
                    {
                        Console.WriteLine("invoice.CustomerId is null or empty");
                        break;
                    }

                    var foundExistingTenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == stripeCustomerId);

                    if (foundExistingTenant == null)
                    {
                        var newTenant = new Tenant
                        {
                            TenantId = Guid.NewGuid(),
                            StripeCustomerId = stripeCustomerId,
                            Email = invoice.CustomerEmail ?? "noemail@example.com",
                            Name = invoice.CustomerName ?? "Unknown",
                            AddressLine1 = invoice.CustomerAddress?.Line1 ?? "N/A",
                            City = invoice.CustomerAddress?.City ?? "N/A",
                            State = invoice.CustomerAddress?.State ?? "N/A",
                            PostalCode = invoice.CustomerAddress?.PostalCode ?? "000000",
                            Country = invoice.CustomerAddress?.Country ?? "N/A"
                        };

                        _dbContext.Tenants.Add(newTenant);
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($" New tenant created: {stripeCustomerId}");
                    }
                    else
                    {
                        Console.WriteLine($" Tenant already exists for: {stripeCustomerId}");
                    }

                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.deleted":

                    var subscription = stripeEvent.Data.Object as Subscription;
                    if (subscription == null) break;

                    var existingTenant = await _dbContext.Tenants
                        .FirstOrDefaultAsync(t => t.StripeCustomerId == subscription.CustomerId);

                    if (existingTenant == null)
                    {
                        var newTenant = new Tenant
                        {
                            TenantId = Guid.NewGuid(),
                            StripeCustomerId = subscription.CustomerId,
                            Email = subscription.Customer?.Email ?? "noemail@example.com",
                            Name = subscription.Customer?.Name ?? "Unknown",
                            AddressLine1 = subscription.Customer?.Address?.Line1 ?? "N/A",
                            City = subscription.Customer?.Address?.City ?? "N/A",
                            State = subscription.Customer?.Address?.State ?? "N/A",
                            PostalCode = subscription.Customer?.Address?.PostalCode ?? "000000",
                            Country = subscription.Customer?.Address?.Country ?? "N/A"
                        };
                        _dbContext.Tenants.Add(newTenant);
                        await _dbContext.SaveChangesAsync();

                        Console.WriteLine("Tenant (from subscription) saved to database");
                        existingTenant = newTenant;
                    }

                    var firstItem = subscription.Items?.Data?.FirstOrDefault();
                    var priceId = firstItem?.Price?.Id;

                    if (string.IsNullOrEmpty(priceId))
                    {
                        Console.WriteLine("Subscription has no valid price item.");
                        break;
                    }

                    var existingSubscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == subscription.Id);

                    if (existingSubscription == null)
                    {
                        var newSubscription = new SubscriptionDb
                        {
                            StripeSubscriptionId = subscription.Id,
                            PlanId = priceId,
                            Status = subscription.Status,
                            IsRecurring = subscription.Items.Data.Any(item => item.Price.Recurring != null),
                            IsApproved = subscription.Status == "active",
                            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                            StartDate = subscription.StartDate,
                            EndDate = subscription.CanceledAt,
                            CreatedAt = DateTime.UtcNow,
                            TenantId = existingTenant.TenantId
                        };
                        _dbContext.Subscriptions.Add(newSubscription);
                    }
                    else
                    {
                        existingSubscription.Status = subscription.Status;
                        existingSubscription.IsRecurring = subscription.Items.Data.Any(item => item.Price.Recurring != null);
                        existingSubscription.IsApproved = subscription.Status == "active";
                        existingSubscription.CancelAtPeriodEnd = subscription.CancelAtPeriodEnd;
                        existingSubscription.EndDate = subscription.CanceledAt;
                        existingSubscription.UpdatedAt = DateTime.UtcNow;
                    }

                    await _dbContext.SaveChangesAsync();

                    // Enforce plan rules: cancel duplicates and enforce enterprise exclusivity
                    //await EnforceEnterprisePlanRulesAsync(subscription.CustomerId);

                    break;


                default:
                    Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }
        }
    }
}

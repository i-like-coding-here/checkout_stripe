        //public async Task<SubscriptionResponse> UpgradeOrAddPlan(string tenantId, string newPriceId)
        //{
        //    var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantId.ToString() == tenantId);
        //    if (tenant == null)
        //        throw new Exception("Tenant not found");

        //    var stripeCustomerId = tenant.StripeCustomerId;
        //    var existingSubscriptions = await _dbContext.Subscriptions.Where(s => s.TenantId == tenant.TenantId && s.Status != "canceled").ToListAsync();

        //    var subscriptionService = new SubscriptionService();

        //    var activeSubscriptionId = existingSubscriptions.FirstOrDefault()?.StripeSubscriptionId;

        //    Subscription currentStripeSubscription = null;
        //    if(!string.IsNullOrEmpty(activeSubscriptionId))
        //    {
        //        currentStripeSubscription = await subscriptionService.GetAsync(activeSubscriptionId);
        //    }

        //    if (existingSubscriptions.Any(s => s.PlanId == newPriceId))
        //        throw new Exception("Already have this plan");

        //    var isEnterprise = newPriceId == "enterprise";

        //    if(isEnterprise)
        //    {
        //        if(currentStripeSubscription != null)
        //        {
        //            await subscriptionService.CancelAsync(currentStripeSubscription.Id);
        //        }

        //        var newSub = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
        //        {
        //            Customer = stripeCustomerId,
        //            Items = new List<SubscriptionItemOptions>
        //            {
        //                new SubscriptionItemOptions { Price = newPriceId }
        //            }
        //        });
        //        foreach ( var sub in existingSubscriptions)
        //        {
        //            sub.Status = "canceled";
        //            sub.CancelAtPeriodEnd = false;
        //            sub.EndDate = DateTime.UtcNow;
        //            sub.UpdatedAt = DateTime.UtcNow;
        //        }

        //        _dbContext.Subscriptions.Add(new SubscriptionDb
        //        {
        //            StripeSubscriptionId = newSub.Id,
        //            PlanId = newPriceId,
        //            Status = newSub.Status,
        //            TenantId = tenant.TenantId,
        //            CreatedAt = DateTime.UtcNow,
        //            IsApproved = newSub.Status == "active",
        //            IsRecurring = newSub.Items.Data.Any(i => i.Price.Recurring != null),
        //            CancelAtPeriodEnd = newSub.CancelAtPeriodEnd,
        //            EndDate = newSub.CanceledAt
        //        });

        //        await _dbContext.SaveChangesAsync();

        //        return new SubscriptionResponse { Subscription = newSub };
        //    }
        //    else
        //    {
        //        if (currentStripeSubscription == null)
        //            throw new Exception("No active subscription to add plan to");

        //        var updated = await subscriptionService.UpdateAsync(currentStripeSubscription.Id, new SubscriptionUpdateOptions
        //        {
        //            CancelAtPeriodEnd = false,
        //            Items = new List<SubscriptionItemOptions>
        //            {
        //                new SubscriptionItemOptions
        //                {
        //                    Price = newPriceId,
        //                    Quantity = 1
        //                }
        //            }
        //        });

        //        _dbContext.Subscriptions.Add(new SubscriptionDb
        //        {
        //            StripeSubscriptionId = updated.Id,
        //            PlanId = newPriceId,
        //            Status = updated.Status,
        //            TenantId = tenant.TenantId,
        //            CreatedAt = DateTime.UtcNow,
        //            IsApproved = updated.Status == "active",
        //            IsRecurring = updated.Items.Data.Any(i => i.Price.Recurring != null),
        //            CancelAtPeriodEnd = updated.CancelAtPeriodEnd,
        //            EndDate = updated.CanceledAt
        //        });

        //        await _dbContext.SaveChangesAsync();

        //        return new SubscriptionResponse { Subscription = updated };
        //    }
        //}

        public async Task<SubscriptionDecision> DetermineSubscriptionActionAsync(string tenantId, string newPriceId)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantId.ToString() == tenantId);
            if (tenant == null)
                throw new Exception("Tenant not found.");

            var existingSubs = await _dbContext.Subscriptions
                .Where(s => s.TenantId == tenant.TenantId && s.Status != "canceled")
                .ToListAsync();

            if (existingSubs.Any(s => s.PlanId == newPriceId))
            {
                return new SubscriptionDecision
                {
                    IsDuplicate = true,
                    Message = "You already have this plan.",
                    Tenant = tenant
                };
            }

            var isEnterprise = newPriceId == "enterprise"; 

            return new SubscriptionDecision
            {
                IsEnterpriseUpgrade = isEnterprise,
                IsDuplicate = false,
                Tenant = tenant
            };
        }

        public async Task<SubscriptionResponse> ApplySubscriptionDecisionAsync(SubscriptionDecision decision, string newPriceId)
        {
            var tenant = decision.Tenant;

            if (decision.IsDuplicate)
                throw new Exception(decision.Message);

            // Cancel existing and create new Enterprise subscription
            if (decision.IsEnterpriseUpgrade)
            {
                foreach (var existing in await _dbContext.Subscriptions
                             .Where(s => s.TenantId == tenant.TenantId && s.Status != "canceled")
                             .ToListAsync())
                {
                    await _subscriptionService.CancelAsync(existing.StripeSubscriptionId);
                }

                var newSub = await _subscriptionService.CreateAsync(new SubscriptionCreateOptions
                {
                    Customer = tenant.StripeCustomerId,
                    Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions { Price = newPriceId }
            }
                });

                // Remove old subscriptions from DB
                var oldSubs = await _dbContext.Subscriptions
                    .Where(s => s.TenantId == tenant.TenantId)
                    .ToListAsync();

                _dbContext.Subscriptions.RemoveRange(oldSubs);

                // Add new subscription record
                _dbContext.Subscriptions.Add(new SubscriptionDb
                {
                    StripeSubscriptionId = newSub.Id,
                    PlanId = newPriceId,
                    Status = newSub.Status,
                    TenantId = tenant.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = newSub.Status == "active",
                    IsRecurring = newSub.Items.Data.Any(i => i.Price.Recurring != null),
                    CancelAtPeriodEnd = newSub.CancelAtPeriodEnd,
                    EndDate = newSub.CanceledAt
                });

                await _dbContext.SaveChangesAsync();

                return new SubscriptionResponse { Subscription = newSub };
            }

            // Add a new plan to existing subscription
            var existingActiveSub = await _dbContext.Subscriptions
                .Where(s => s.TenantId == tenant.TenantId && s.Status != "canceled")
                .FirstOrDefaultAsync();

            if (existingActiveSub == null)
                throw new Exception("No active subscription found to add new plan.");

            var updated = await _subscriptionService.UpdateAsync(existingActiveSub.StripeSubscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                Items = new List<SubscriptionItemOptions>
        {
            new SubscriptionItemOptions { Price = newPriceId }
        }
            });

            _dbContext.Subscriptions.Add(new SubscriptionDb
            {
                StripeSubscriptionId = updated.Id,
                PlanId = newPriceId,
                Status = updated.Status,
                TenantId = tenant.TenantId,
                CreatedAt = DateTime.UtcNow,
                IsApproved = updated.Status == "active",
                IsRecurring = updated.Items.Data.Any(i => i.Price.Recurring != null),
                CancelAtPeriodEnd = updated.CancelAtPeriodEnd,
                EndDate = updated.CanceledAt
            });

            await _dbContext.SaveChangesAsync();

            return new SubscriptionResponse { Subscription = updated };
        }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckoutSession.core.Models.billing;
using Microsoft.AspNetCore.Mvc;

namespace CheckoutSession.core.Interfaces
{
    public interface ICheckoutSessionService
    {
        ConfigResponse GetConfig();
        //Task<string> CreateCheckoutSession(string priceId);
        //Task<SubscriptionResponse> UpgradeOrAddPlan(string tenantId, string newPriceId);
        Task<string> CreateCheckoutSession(PriceRequest req);
        //Task EnforceEnterprisePlanRulesAsync(string customerId);
        Task<bool> CanSubscribeToPlan(string customerId, string requestedPriceId);

        Task<CheckoutSessionDto> CheckoutSession(string sessionId);
        Task SyncPlansToStripe();
        Task<Guid> GetTenantIdByStripeCustomerId(string stripeCustomerId);
        //SubscriptionResponse UpdateSubscription(string subscriptionId, string newPriceKey);
        Task<SubscriptionResponse> UpdateSubscription(UpdateSubscriptionRequest req);
        Task<SubscriptionResponse> CancelSubscription(string subscriptionId);
        Task<SubscriptionResponse> PauseSubscription(string subscriptionId);

        Task<SubscriptionResponse> ResumeSubscription(string subscriptionId);


        SubscriptionsResponse ListSubscriptions(string customerId, Guid tenantId);

        Task<string> CustomerPortal(string sessionId);
        Task HandleWebhook(string json, string signatureHeader);
    }

}

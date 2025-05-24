using CheckoutSession.core.Interfaces;
using CheckoutSession.core.Models.Dtos.Requests;
using CheckoutSession.core.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CheckoutSession.api.Controllers
{
    [ApiController]
    [Route("/")]
    public class CheckoutSessionController : ControllerBase
    {
        private readonly ICheckoutSessionService _checkoutSessionService;

        public CheckoutSessionController(ICheckoutSessionService checkoutSessionService)
        {
            _checkoutSessionService = checkoutSessionService;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var config = _checkoutSessionService.GetConfig();
            return Ok(config);  
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] PriceRequest req)
        {
            if (string.IsNullOrEmpty(req.PriceId))
                return BadRequest(new { error = "priceId is required" });

            try
            {
                var url = await _checkoutSessionService.CreateCheckoutSession(req);
                return Ok(new { url });
                //return Redirect(url);

            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("checkout-session")]
        public async Task<IActionResult> GetCheckoutSession([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest(new { error = "sessionId is required" });

            var session = await _checkoutSessionService.CheckoutSession(sessionId);
            return Ok(session); 
        }

        [HttpPost("update-subscription")]
        public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionRequest req)
        {
            if (string.IsNullOrEmpty(req.Subscription))
                return BadRequest(new { error = " Subscription Id is required"});
            var subsription = await _checkoutSessionService.UpdateSubscription(req);
            return Ok(subsription);
        }


        [HttpPost("cancel-subscription")]
        public async Task<ActionResult<SubscriptionResponse>> CancelSubscription([FromBody] CancelSubscriptionRequest req)
        {
            if (string.IsNullOrEmpty(req.Subscription))
                return BadRequest(new { error = " Subscription Id is required" });
            var subsription = await _checkoutSessionService.CancelSubscription(req.Subscription);
            return Ok(subsription);
        }

        [HttpPost("pause-subscription")]
        public async Task<ActionResult<SubscriptionResponse>> PauseSubscription([FromBody] ResumeSubscriptionRequest req)
        {
            if (string.IsNullOrEmpty(req.Subscription))
                return BadRequest(new { error = "Subscription Id is required" });
            var subscription = await _checkoutSessionService.PauseSubscription(req.Subscription);
            return Ok(subscription);
        }

        [HttpPost("resume-subscription")]
        public async Task<ActionResult<SubscriptionResponse>> ResumeSubscription(ResumeSubscriptionRequest req)
        {
            if (string.IsNullOrEmpty(req.Subscription))
                return BadRequest(new { error = " Subscription Id is required" });
            var subsription = await _checkoutSessionService.ResumeSubscription(req.Subscription);
            return Ok(subsription);
        }

        [HttpGet("subscriptions")]
        public ActionResult<SubscriptionsResponse> ListSubscriptions([FromQuery] string customerId, [FromQuery] Guid tenantId)
        {
            if (string.IsNullOrEmpty(customerId) || tenantId == Guid.Empty)
            {
                return BadRequest("Missing tenantId or customerId.");
            }

            var options = new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "all",
            };

            // Include payment method details in the response
            options.AddExpand("data.default_payment_method");

            var service = new SubscriptionService();
            var subscriptions = service.List(options);

            return new SubscriptionsResponse
            {
                Subscriptions = subscriptions,
            };
        }

        [HttpPost("customer-portal")]
        public async Task<IActionResult> CustomerPortal([FromForm] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest(new { error = "sessionId is required" });

            var url = await _checkoutSessionService.CustomerPortal(sessionId);
            return Ok(new { url });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];

            try
            {
                await _checkoutSessionService.HandleWebhook(json, signature);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

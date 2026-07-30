using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Baskets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace E_Commerce.API.Controllers
{

    public class PaymentsController : ApiBaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly PaymentGatewaySettings _stripeSettings;

        public PaymentsController(IPaymentService paymentService , IOptions<PaymentGatewaySettings> options)
        {
            _paymentService = paymentService;
            _stripeSettings = options.Value;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent(string basketId, CancellationToken ct)
        {
            var Result = await _paymentService.CreateOrUpdatePaymentIntentAsync(basketId, ct);

            return ToActionResult(Result);
        }


        // POST: BaseUrl/api/Payments/webhook

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebHook()
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
              
                try
                {
                
                    var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _stripeSettings.WebHookSecret);

                // Handle the event
                // If on SDK version < 46, use class Events instead of EventTypes
                switch (stripeEvent.Type)
                {
                    case EventTypes.PaymentIntentSucceeded:
                        {
                            var succeededPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            if (succeededPaymentIntent is not null)
                                await _paymentService.PaymentSucceeded(succeededPaymentIntent.Id);
                            break;
                            // Then define and call a method to handle the successful payment intent.
                            // handlePaymentIntentSucceeded(paymentIntent);
                        }

                    case EventTypes.PaymentIntentPaymentFailed:
                        {
                            var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            if (failedPaymentIntent is not null)
                                await _paymentService.PaymentFailed(failedPaymentIntent.Id);
                            break;
                            // Then define and call a method to handle the successful attachment of a PaymentMethod.
                            // handlePaymentMethodAttached(paymentMethod);
                        }
                    // ... handle other event types
                    default:
                    
                        break;
                }
                return Ok();
                }
                catch (StripeException e)
                {
                    return BadRequest();
                }
            }

        }
    
}

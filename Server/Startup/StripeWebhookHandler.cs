using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Startup
{
    // Handles the Stripe webhook as raw terminal middleware, registered in Program.cs
    // before UseOqtane runs at all - see the comment there for why. Uses the framework's
    // own ILogger, not Oqtane's ILogManager: ILogManager needs the current Site/Alias
    // resolved to know which site's log to write to, and that resolution happens inside
    // UseOqtane - which hasn't run yet at this point in the pipeline. Signature
    // verification below is this endpoint's actual authentication - nothing else guards
    // it, which is exactly why it must never trust anything about the request except
    // what the signature proves.
    public static class StripeWebhookHandler
    {
        public static async Task HandleAsync(HttpContext context, IStripeService stripeService, IMSSA_DogManager dogManager, ILogger logger)
        {
            string json;
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                json = await reader.ReadToEndAsync();
            }

            Event stripeEvent;
            try
            {
                stripeEvent = stripeService.ConstructWebhookEvent(json, context.Request.Headers["Stripe-Signature"]);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook signature verification failed");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            try
            {
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    await HandleCheckoutCompletedAsync(session, dogManager, logger);
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error processing Stripe webhook event {EventType}", stripeEvent.Type);
                // 500 is correct here - Stripe will retry delivery.
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        private static async Task HandleCheckoutCompletedAsync(Session session, IMSSA_DogManager dogManager, ILogger logger)
        {
            if (session?.Metadata == null
                || !session.Metadata.TryGetValue("Purpose", out var purpose)
                || purpose != "FuturityNomination")
            {
                return;
            }

            if (!session.Metadata.TryGetValue("ParticipationId", out var participationIdText)
                || !int.TryParse(participationIdText, out var participationId))
            {
                logger.LogError("Futurity checkout session {SessionId} completed with no valid ParticipationId in metadata", session.Id);
                return;
            }

            // AmountTotal is in the smallest currency unit (cents for USD).
            var amount = (session.AmountTotal ?? 0) / 100m;

            var updated = await dogManager.MarkFuturityPaymentReceivedAsync(participationId, session.PaymentIntentId, amount, moduleId: 0);
            if (updated == null)
            {
                logger.LogError("Futurity participation {ParticipationId} not found - could not mark Paid", participationId);
            }
            else
            {
                logger.LogInformation("Futurity participation {ParticipationId} marked Paid via Stripe session {SessionId}", participationId, session.Id);
            }
        }
    }
}

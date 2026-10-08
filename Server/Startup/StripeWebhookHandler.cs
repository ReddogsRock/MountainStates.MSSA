using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Events.Manager;
using MountainStates.MSSA.Server.Startup;

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
        public static async Task HandleAsync(HttpContext context, IStripeService stripeService, IMSSA_DogManager dogManager, IMSSA_HandlerManager handlerManager, IMSSA_EventManager eventManager, IMSSA_AdminNotificationService adminNotificationService, ILogger logger)
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
                    await HandleCheckoutCompletedAsync(session, dogManager, handlerManager, eventManager, adminNotificationService, logger);
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

        private static async Task HandleCheckoutCompletedAsync(Session session, IMSSA_DogManager dogManager, IMSSA_HandlerManager handlerManager, IMSSA_EventManager eventManager, IMSSA_AdminNotificationService adminNotificationService, ILogger logger)
        {
            if (session?.Metadata == null || !session.Metadata.TryGetValue("Purpose", out var purpose))
            {
                return;
            }

            switch (purpose)
            {
                case "FuturityNomination":
                    await HandleFuturityNominationAsync(session, dogManager, logger);
                    break;
                case "MembershipPurchase":
                    await HandleMembershipPurchaseAsync(session, handlerManager, logger);
                    break;
                case "SanctioningFee":
                    await HandleSanctioningFeeAsync(session, eventManager, adminNotificationService, logger);
                    break;
            }
        }

        private static async Task HandleFuturityNominationAsync(Session session, IMSSA_DogManager dogManager, ILogger logger)
        {
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

        private static async Task HandleMembershipPurchaseAsync(Session session, IMSSA_HandlerManager handlerManager, ILogger logger)
        {
            if (!session.Metadata.TryGetValue("MembershipId", out var membershipIdText)
                || !int.TryParse(membershipIdText, out var membershipId))
            {
                logger.LogError("Membership checkout session {SessionId} completed with no valid MembershipId in metadata", session.Id);
                return;
            }

            // AmountTotal is in the smallest currency unit (cents for USD).
            var amount = (session.AmountTotal ?? 0) / 100m;

            var updated = await handlerManager.MarkMembershipPaymentReceivedAsync(membershipId, session.PaymentIntentId, amount, moduleId: 0);
            if (updated == null)
            {
                logger.LogError("Membership {MembershipId} not found - could not mark Paid", membershipId);
            }
            else
            {
                logger.LogInformation("Membership {MembershipId} marked Paid via Stripe session {SessionId}", membershipId, session.Id);
            }
        }

        private static async Task HandleSanctioningFeeAsync(Session session, IMSSA_EventManager eventManager, IMSSA_AdminNotificationService adminNotificationService, ILogger logger)
        {
            if (!session.Metadata.TryGetValue("EventId", out var eventIdText)
                || !int.TryParse(eventIdText, out var eventId))
            {
                logger.LogError("Sanctioning fee checkout session {SessionId} completed with no valid EventId in metadata", session.Id);
                return;
            }

            // AmountTotal is in the smallest currency unit (cents for USD).
            var amount = (session.AmountTotal ?? 0) / 100m;

            var updated = await eventManager.MarkSanctionFeePaidAsync(eventId, session.PaymentIntentId, amount, moduleId: 0);
            if (updated == null)
            {
                logger.LogError("Event {EventId} not found - could not mark sanctioning fee Paid", eventId);
            }
            else
            {
                logger.LogInformation("Event {EventId} sanctioning fee marked Paid via Stripe session {SessionId}", eventId, session.Id);

                // This middleware runs before UseOqtane, so there's no resolved Alias to
                // read a real SiteId from (same reason this uses a plain ILogger above,
                // not ILogManager) - siteId 1 is hardcoded same as moduleId: 0 above,
                // fine for this single-site install.
                adminNotificationService.NotifyAdmins(1, $"Sanctioning Fee Paid: {updated.EventName}",
                    $"{updated.EventName} paid a sanctioning fee of {amount:C}.");
            }
        }
    }
}

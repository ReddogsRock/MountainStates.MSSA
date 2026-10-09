using System.Threading.Tasks;
using Stripe;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Manager
{
    public interface IStripeService
    {
        // Creates a Checkout Session for a Futurity nomination fee and returns the URL
        // to redirect the browser to. The Session's metadata carries the ParticipationId
        // so the webhook can find its way back to the right record. dogName/ownerName
        // are looked up by the caller and used to build the PaymentIntent's Description
        // (e.g. "Futurity Nomination – Dog (Owner)"), visible in the Stripe Dashboard.
        Task<string> CreateFuturityCheckoutSessionAsync(int participationId, string dogName, string ownerName, int year, string successUrl, string cancelUrl);

        // Creates a Checkout Session for a membership purchase/renewal and returns the
        // URL to redirect the browser to. membershipType selects which of the
        // Stripe:MembershipProductIds products to charge. The Session's metadata carries
        // the MembershipId so the webhook can find its way back to the right record.
        // memberName is looked up by the caller and used to build the PaymentIntent's
        // Description (e.g. "Membership Dues 2027 – Name").
        Task<string> CreateMembershipCheckoutSessionAsync(int membershipId, string membershipType, string memberName, int year, string successUrl, string cancelUrl);

        // Creates a Checkout Session for an event's sanctioning fee ($ per run, sanctioned
        // + unsanctioned) and returns the URL to redirect the browser to. Unlike the other
        // two flows, quantity varies per event rather than always being 1. The Session's
        // metadata carries the EventId so the webhook can find its way back to the record.
        // eventName is looked up by the caller and used to build the PaymentIntent's
        // Description (e.g. "Sanctioning Fee – Event Name").
        Task<string> CreateSanctioningFeeCheckoutSessionAsync(int eventId, string eventName, int quantity, int year, string successUrl, string cancelUrl);

        // Verifies the Stripe-Signature header and parses the event. Throws if the
        // signature doesn't check out - never process a webhook body without this.
        Event ConstructWebhookEvent(string json, string stripeSignatureHeader);
    }
}

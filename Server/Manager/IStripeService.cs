using System.Threading.Tasks;
using Stripe;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Manager
{
    public interface IStripeService
    {
        // Creates a Checkout Session for a Futurity nomination fee and returns the URL
        // to redirect the browser to. The Session's metadata carries the ParticipationId
        // so the webhook can find its way back to the right record.
        Task<string> CreateFuturityCheckoutSessionAsync(int participationId, string successUrl, string cancelUrl);

        // Creates a Checkout Session for a membership purchase/renewal and returns the
        // URL to redirect the browser to. membershipType selects which of the
        // Stripe:MembershipProductIds products to charge. The Session's metadata carries
        // the MembershipId so the webhook can find its way back to the right record.
        Task<string> CreateMembershipCheckoutSessionAsync(int membershipId, string membershipType, string successUrl, string cancelUrl);

        // Verifies the Stripe-Signature header and parses the event. Throws if the
        // signature doesn't check out - never process a webhook body without this.
        Event ConstructWebhookEvent(string json, string stripeSignatureHeader);
    }
}

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    public class MembershipCheckoutResult
    {
        public string CheckoutUrl { get; set; }

        // Set instead of CheckoutUrl when the membership can't be checked out as-is
        // (e.g. its MembershipType has no Stripe product configured) - a handled,
        // user-facing condition rather than a server error.
        public string ErrorMessage { get; set; }
    }
}

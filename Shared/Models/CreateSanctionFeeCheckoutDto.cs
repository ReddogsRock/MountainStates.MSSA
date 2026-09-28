namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    // The client sends only the honor-system Unsanctioned Runs count - the Sanctioned
    // Runs portion of the quantity is always derived server-side from actual scored
    // entries, never trusted from the client. Client builds the success/cancel URLs
    // itself (via Oqtane's NavigateUrl), same reasoning as CreateFuturityCheckoutDto.
    public class CreateSanctionFeeCheckoutDto
    {
        public int EventId { get; set; }
        public int UnsanctionedRuns { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}

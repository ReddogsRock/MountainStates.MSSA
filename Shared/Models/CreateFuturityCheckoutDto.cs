namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    // The client builds the success/cancel URLs itself (via Oqtane's NavigateUrl), since
    // the server has no reliable way to know the site's page routing structure.
    public class CreateFuturityCheckoutDto
    {
        public int ParticipationId { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
}

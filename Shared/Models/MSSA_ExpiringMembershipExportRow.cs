namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // CSV-export shape for the expiring-membership segment - includes HandlerId (looked
    // up by email at export time, not stored on MSSA_MailingListSubscriber itself) so
    // the export can build a personalized renewal link straight to that person's own
    // Handler Detail page.
    public class MSSA_ExpiringMembershipExportRow
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? HandlerId { get; set; }
    }
}

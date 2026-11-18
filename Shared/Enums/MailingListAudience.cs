namespace MountainStates.MSSA.Module.MSSA_Handlers.Enums
{
    // Who a "Send a Campaign" send targets. Always resolved to active (IsActive)
    // subscribers only, regardless of which of these is picked - see
    // MSSA_MailingListRepository.GetCampaignRecipientsAsync.
    public static class MailingListAudience
    {
        public const string All = "All";
        public const string Expiring = "Expiring";
        public const string NamedList = "NamedList";
    }
}

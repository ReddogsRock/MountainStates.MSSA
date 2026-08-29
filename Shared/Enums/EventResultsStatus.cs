namespace MountainStates.MSSA.Module.MSSA_Results.Enums
{
    // An Event's results move forward through this one-way flow only - there's no
    // "rejected" state, since approval is a sanctioning-fee sign-off, not a data-quality
    // check. A Trial Secretary can still edit scores while PendingApproval (in case
    // something needs fixing before the fee clears); nothing is editable once Approved.
    public static class EventResultsStatus
    {
        public const string NotSubmitted = "NotSubmitted";
        public const string PendingApproval = "PendingApproval";
        public const string Approved = "Approved";
    }
}

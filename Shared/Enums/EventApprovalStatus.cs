namespace MountainStates.MSSA.Module.MSSA_Events.Enums
{
    // An event created by anyone other than an Admin starts Pending and stays hidden
    // from public view (and from other Trial Secretaries) until an Admin approves it.
    // Events an Admin creates are Approved immediately - there's no need for an Admin
    // to approve their own event.
    public static class EventApprovalStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
    }
}

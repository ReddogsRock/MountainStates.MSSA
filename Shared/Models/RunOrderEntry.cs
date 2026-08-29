namespace MountainStates.MSSA.Module.MSSA_Entries.Models
{
    // One entry's position in a trial's run order. Used both for the in-memory proposal
    // (generated, editable, not yet saved) and for the finalized/persisted order.
    public class RunOrderEntry
    {
        public int EntryId { get; set; }
        public int TrialId { get; set; }
        public int RunOrder { get; set; }

        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }

        public string DogName { get; set; }
        public string HandlerName { get; set; }
    }
}

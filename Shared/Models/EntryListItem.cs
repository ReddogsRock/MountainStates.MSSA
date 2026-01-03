using System;

namespace MountainStates.MSSA.Module.MSSA_Entries.Models
{
    public class EntryListItem
    {
        public int EntryId { get; set; }
        public int TrialId { get; set; }
        public string HandlerName { get; set; }
        public string DogName { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        public int? RunOrder { get; set; }
        public int? Placing { get; set; }
        public int? TrialPoints { get; set; }
        public decimal? TotalScore { get; set; }
        public string EventName { get; set; }
        public DateTime TrialDate { get; set; }
        public string Stock { get; set; }
    }
}

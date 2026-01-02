using System;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // DTO for displaying handler's competition entries
    public class MSSA_HandlerEntry
    {
        public int EntryId { get; set; }
        public string DogName { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        public string Stock { get; set; }
        public int? Placing { get; set; }
        public int? TrialPoints { get; set; }
        public DateTime TrialDate { get; set; }
        public string EventName { get; set; }
        public int Year { get; set; }
    }
}
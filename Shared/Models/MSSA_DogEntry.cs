using System;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    // DTO for displaying dog's competition entries
    public class MSSA_DogEntry
    {
        public int EntryId { get; set; }
        public string HandlerName { get; set; }
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
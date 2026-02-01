using System;

namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    /// <summary>
    /// Lightweight view model for displaying entries in trial summaries
    /// </summary>
    public class EntryListItem
    {
        public int EntryId { get; set; }
        public int TrialId { get; set; }
        
        // Display Names
        public string HandlerName { get; set; }
        public string DogName { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        
        // Run Details
        public int? RunOrder { get; set; }
        public int? Placing { get; set; }
        
        // Times and Scoring
        public TimeSpan? RunTime { get; set; }
        public TimeSpan? TieBreakerTime { get; set; }
        public decimal? SumOfObstacles { get; set; }
        public int? TrialPoints { get; set; }
        public decimal? TotalScore { get; set; }
        public string EventName { get; set; }
        public DateTime TrialDate { get; set; }
        public string Stock { get; set; }

        // Helper property for sorting
        public bool IsComplete => Placing.HasValue;
    }
}

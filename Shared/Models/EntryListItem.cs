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
        public int DogId { get; set; }
        public int HandlerId { get; set; }

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

        // Individual obstacle scores + Penalty + Comments - needed for the scoring
        // export/import round trip (SumOfObstacles/TotalScore alone aren't enough to
        // fill in or edit each obstacle).
        public decimal? ObstacleScore1 { get; set; }
        public decimal? ObstacleScore2 { get; set; }
        public decimal? ObstacleScore3 { get; set; }
        public decimal? ObstacleScore4 { get; set; }
        public decimal? ObstacleScore5 { get; set; }
        public decimal? ObstacleScore6 { get; set; }
        public decimal? ObstacleScore7 { get; set; }
        public decimal? ObstacleScore8 { get; set; }
        public decimal? ObstacleScore9 { get; set; }
        public decimal? Penalty { get; set; }
        public string Comments { get; set; }

        public string EventName { get; set; }
        public DateTime TrialDate { get; set; }
        public string Stock { get; set; }

        // Competition year (Event.PointYear, falling back to TrialDate.Year) - used
        // to check Futurity enrollment for the "+" marker on Nursery-class entries.
        public int Year { get; set; }

        // Helper property for sorting
        public bool IsComplete => Placing.HasValue;
    }
}

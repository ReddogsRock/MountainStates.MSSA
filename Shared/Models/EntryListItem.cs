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

        // Raw score entered directly via the Results module, if that entry method was
        // used for this run instead of the 9-obstacle breakdown. TotalScore above already
        // resolves to this when present - kept separately so the Results grid can tell
        // whether a run was scored this way.
        public decimal? EnteredTotalScore { get; set; }

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

        // Owner of the Event this entry's Trial belongs to (MSSA_Event.CreatedByUserId).
        // Used to gate the per-entry Edit link to the Trial Secretary who owns the event.
        public int? EventCreatedByUserId { get; set; }

        // Who's assigned to score this entry's Trial (MSSA_Trial.ScorekeeperUserId).
        // Used to gate the per-entry Edit link to that Scorekeeper.
        public int? TrialScorekeeperUserId { get; set; }

        // MSSA_Event.ResultsApprovalStatus for this entry's Event. Public-facing views
        // (the Events Detail page) use this to hide results until they're Approved.
        public string EventResultsApprovalStatus { get; set; }

        // Helper property for sorting
        public bool IsComplete => Placing.HasValue;
    }
}

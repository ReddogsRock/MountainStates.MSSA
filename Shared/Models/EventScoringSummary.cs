using System;

namespace MountainStates.MSSA.Module.MSSA_Results.Models
{
    // One row in the Results module's Event dropdown / admin queue: an event plus how
    // far along its scoring is, so the UI can hide events with nothing left to do and
    // show progress ("42/50 runs scored") before Submit is enabled.
    public class EventScoringSummary
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string DateRange { get; set; }
        public string ResultsApprovalStatus { get; set; }

        public int TotalRuns { get; set; }
        public int ScoredRuns { get; set; }
        public bool AllScored => TotalRuns > 0 && ScoredRuns == TotalRuns;

        public DateTime? ResultsSubmittedDate { get; set; }
        public int? ResultsSubmittedByUserId { get; set; }
        public DateTime? ResultsApprovedDate { get; set; }
    }
}

using System;

namespace MountainStates.MSSA.Module.MSSA_Results.Models
{
    // One row in the Results entry grid: a single run, in run order, with just the
    // fields a Trial Secretary fills in by hand (Time, Tie Time, Total Points).
    public class ResultRunRow
    {
        public int EntryId { get; set; }
        public int TrialId { get; set; }
        public int? RunOrder { get; set; }

        // Lets the client target "Calculate Placing & Points" at just this one class -
        // ClassName alone isn't a unique key (e.g. Nursery On-foot vs Horseback are two
        // different ClassIds sharing the same ClassName).
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        public string HandlerName { get; set; }
        public string DogName { get; set; }

        // Editable, as typed - "." or ":" both accepted as the minutes/seconds separator.
        public string RunTimeStr { get; set; }
        public string TieBreakerTimeStr { get; set; }
        public decimal? TotalScore { get; set; }

        // Read-only until "Calculate Placing & Points" is run for the trial.
        public int? Placing { get; set; }
        public int? TrialPoints { get; set; }

        public bool IsScored => !string.IsNullOrEmpty(RunTimeStr) && TotalScore.HasValue;
    }
}

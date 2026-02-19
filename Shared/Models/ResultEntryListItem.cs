using System;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Models
{
    /// <summary>
    /// Represents one entry row in the results grid for a Trial + Class.
    /// Placing and TrialPoints remain null until Finalized.
    /// </summary>
    public class ResultEntryListItem
    {
        public int EntryId { get; set; }
        public string HandlerName { get; set; }
        public string DogName { get; set; }
        public bool HandlerIsMSSAMember { get; set; }

        // Times stored as pre-formatted strings ("MM:SS.ff" or "-") to avoid TimeSpan serialization issues
        public string RunTimeDisplay { get; set; } = "-";
        public string TieBreakerTimeDisplay { get; set; } = "-";

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

        // Calculated after Finalize
        public int? Placing { get; set; }
        public int? TrialPoints { get; set; }

        // Computed total score (sum of obstacles minus penalty)
        public decimal? TotalScore
        {
            get
            {
                decimal sum = 0;
                bool hasAny = false;
                if (ObstacleScore1.HasValue) { sum += ObstacleScore1.Value; hasAny = true; }
                if (ObstacleScore2.HasValue) { sum += ObstacleScore2.Value; hasAny = true; }
                if (ObstacleScore3.HasValue) { sum += ObstacleScore3.Value; hasAny = true; }
                if (ObstacleScore4.HasValue) { sum += ObstacleScore4.Value; hasAny = true; }
                if (ObstacleScore5.HasValue) { sum += ObstacleScore5.Value; hasAny = true; }
                if (ObstacleScore6.HasValue) { sum += ObstacleScore6.Value; hasAny = true; }
                if (ObstacleScore7.HasValue) { sum += ObstacleScore7.Value; hasAny = true; }
                if (ObstacleScore8.HasValue) { sum += ObstacleScore8.Value; hasAny = true; }
                if (ObstacleScore9.HasValue) { sum += ObstacleScore9.Value; hasAny = true; }
                if (Penalty.HasValue) sum -= Penalty.Value;
                return hasAny ? sum : (decimal?)null;
            }
        }

        public bool IsFinalized => Placing.HasValue;
    }
}

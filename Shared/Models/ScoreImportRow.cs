namespace MountainStates.MSSA.Module.MSSA_Entries.Models
{
    // One row from the re-uploaded scores file. EntryId is the only field used to match
    // back to the database record - the descriptive columns (RunOrder/Class/Handler/Dog)
    // in the export exist for the scorekeeper's reference only and aren't re-imported.
    public class ScoreImportRow
    {
        public int EntryId { get; set; }

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

        // Parsed as "M:SS" strings client-side before reaching the server, same pattern
        // as the existing single-entry Edit form's RunTime/TieBreakerTime handling.
        public string RunTime { get; set; }
        public string TieBreakerTime { get; set; }

        public int? Placing { get; set; }
        public int? TrialPoints { get; set; }
        public string Comments { get; set; }
    }
}

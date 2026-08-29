namespace MountainStates.MSSA.Module.MSSA_Results.Models
{
    // What the grid posts when a single row is saved (Enter pressed). Deliberately
    // narrow - just the three fields a Trial Secretary is allowed to touch here - rather
    // than round-tripping the whole MSSA_Entry.
    public class SaveResultRowDto
    {
        public int EntryId { get; set; }
        public string RunTimeStr { get; set; }
        public string TieBreakerTimeStr { get; set; }
        public decimal? TotalScore { get; set; }
    }
}

namespace MountainStates.MSSA.Module.MSSA_TopScores.Models
{
    public class TopScoreResult
    {
        public int Rank { get; set; }
        public string DogName { get; set; }
        public string HandlerName { get; set; }
        public int TotalPoints { get; set; }

        // Additional fields for display/sorting
        public int DogId { get; set; }
        public int? HandlerId { get; set; }  // Nullable for dog-only standings
    }
}

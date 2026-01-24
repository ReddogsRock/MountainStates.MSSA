namespace MountainStates.MSSA.Module.MSSA_TopScores.Models
{
    public class TopScoreParameters
    {
        public int Year { get; set; }  // 2-digit year (25, 26, etc.)
        public string ClassName { get; set; }  // Changed from ClassId to ClassName
        public string Stock { get; set; }  // "Cattle" or "Sheep"
        public int Quantity { get; set; } = 10;  // Default to top 10
    }
}

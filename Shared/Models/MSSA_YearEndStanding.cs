namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Models
{
    // A single ranked row: one Dog (or Dog+Handler, depending on the Class's
    // PointsAccumulateByDogOnly flag) and its total points for the selected
    // Year/Level/Species (or across all years, for Lifetime).
    public class MSSA_YearEndStanding
    {
        public int Rank { get; set; }

        public int DogId { get; set; }
        public string DogName { get; set; }

        // Null for classes scored by Dog only (e.g. Nursery) - there is no single
        // handler when points come from runs with potentially different handlers.
        public int? HandlerId { get; set; }
        public string HandlerName { get; set; }

        public decimal TotalPoints { get; set; }
    }
}

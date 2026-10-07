using System;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    // One row in the admin Futurity Roster report - every dog nominated for a given
    // year, regardless of whether they've run (or scored) yet. Distinct from Year End
    // Standings, which is a points leaderboard restricted to dogs with approved,
    // scored results - this is the plain enrollment list.
    public class FuturityRosterEntry
    {
        public int ParticipationId { get; set; }
        public int Year { get; set; }

        public int DogId { get; set; }
        public string DogName { get; set; }
        public string OwnerName { get; set; }

        public string Status { get; set; }
        public DateTime? DateReceived { get; set; }
        public decimal? Amount { get; set; }
        public bool HasDocument { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

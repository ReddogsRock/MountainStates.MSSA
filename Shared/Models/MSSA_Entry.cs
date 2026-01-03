using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Entries.Models
{
    public class MSSA_Entry
    {
        [Key]
        public int EntryId { get; set; }

        [Required]
        public int TrialId { get; set; }

        [Required]
        public int HandlerId { get; set; }

        [Required]
        public int DogId { get; set; }

        [Required]
        public int ClassId { get; set; }

        // Run Details
        public int? RunOrder { get; set; }

        // Results
        public int? Placing { get; set; }
        public TimeSpan? RunTime { get; set; }
        public TimeSpan? TieBreakerTime { get; set; }

        // Scoring (9 obstacles/tasks)
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
        public int? TrialPoints { get; set; }

        // Membership Status (affects scoring)
        public bool HandlerIsMSSAMember { get; set; } = false;

        public string Comments { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? EnteredBy { get; set; }
        public int? ModifiedBy { get; set; }

        // Navigation properties (not mapped to DB)
        [NotMapped]
        public string HandlerName { get; set; }

        [NotMapped]
        public string DogName { get; set; }

        [NotMapped]
        public string ClassName { get; set; }

        [NotMapped]
        public string SubClassName { get; set; }

        [NotMapped]
        public string Stock { get; set; }

        [NotMapped]
        public string EventName { get; set; }

        [NotMapped]
        public DateTime TrialDate { get; set; }

        [NotMapped]
        public decimal? TotalScore
        {
            get
            {
                decimal sum = 0;
                if (ObstacleScore1.HasValue) sum += ObstacleScore1.Value;
                if (ObstacleScore2.HasValue) sum += ObstacleScore2.Value;
                if (ObstacleScore3.HasValue) sum += ObstacleScore3.Value;
                if (ObstacleScore4.HasValue) sum += ObstacleScore4.Value;
                if (ObstacleScore5.HasValue) sum += ObstacleScore5.Value;
                if (ObstacleScore6.HasValue) sum += ObstacleScore6.Value;
                if (ObstacleScore7.HasValue) sum += ObstacleScore7.Value;
                if (ObstacleScore8.HasValue) sum += ObstacleScore8.Value;
                if (ObstacleScore9.HasValue) sum += ObstacleScore9.Value;
                if (Penalty.HasValue) sum -= Penalty.Value;
                return sum > 0 ? sum : (decimal?)null;
            }
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    public class MSSA_Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event identifier is required")]
        [StringLength(50)]
        public string EventIdentifier { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(255)]
        public string EventName { get; set; }

        // Location
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(2)]
        public string StateCode { get; set; }

        // Dates
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PointYear { get; set; }

        // Event Leadership
        [StringLength(255)]
        public string ChairmanName { get; set; }

        [StringLength(20)]
        public string ChairmanPhone { get; set; }

        // Sanctioning
        public bool IsMSSASanctioned { get; set; } = false;

        // Administrative Tracking
        public DateTime? ResultsReceivedDate { get; set; }
        public bool ResultsUploaded { get; set; } = false;
        public decimal? SanctionFee { get; set; }
        public DateTime? FeeReceivedDate { get; set; }

        // Planning flags
        public int? NumberOfRuns { get; set; }
        public bool Cattle { get; set; } = false;
        public bool Sheep { get; set; } = false;
        public bool Arena { get; set; } = false;
        public bool Field { get; set; } = false;
        public bool OnFoot { get; set; } = false;
        public bool Horseback { get; set; } = false;
        public bool Open { get; set; } = false;
        public bool Nursery { get; set; } = false;
        public bool Intermediate { get; set; } = false;
        public bool Novice { get; set; } = false;
        public bool Junior { get; set; } = false;
        public string Notes { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties (not mapped to DB)
        [NotMapped]
        public string StateName { get; set; }

        [NotMapped]
        public int TrialCount { get; set; }

        [NotMapped]
        public string DateRange
        {
            get
            {
                if (!StartDate.HasValue) return "Date TBD";
                if (!EndDate.HasValue || StartDate.Value.Date == EndDate.Value.Date)
                    return StartDate.Value.ToString("MMM dd, yyyy");
                return $"{StartDate.Value:MMM dd} - {EndDate.Value:MMM dd, yyyy}";
            }
        }
    }
}

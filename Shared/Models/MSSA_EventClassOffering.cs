using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    // Replaces the old flat boolean planning flags on MSSA_Event (Cattle, Sheep,
    // Arena, Field, Open, Nursery, etc.) with explicit, repeatable rows. Each row
    // says "this event offers <N> runs of <Class> on <Stock> in <Venue>".
    //
    // ClassId already carries Style (On-foot/Horseback) via MSSA_Class.SubClassName,
    // so no separate Style field is needed here.
    public class MSSA_EventClassOffering
    {
        [Key]
        public int OfferingId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        [StringLength(20)]
        public string Stock { get; set; } // Cattle, Sheep

        [Required]
        [StringLength(20)]
        public string Venue { get; set; } // Arena, Field

        [Required]
        [Range(1, 100)]
        public int PlannedRuns { get; set; }

        // Display helpers populated by the repository join - not persisted.
        [NotMapped]
        public string ClassName { get; set; }

        [NotMapped]
        public string SubClassName { get; set; }
    }
}

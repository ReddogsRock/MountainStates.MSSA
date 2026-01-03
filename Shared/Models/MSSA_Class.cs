using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Entries.Models
{
    public class MSSA_Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClassName { get; set; }

        [StringLength(50)]
        public string SubClassName { get; set; }

        // Points accumulation rule flag
        public bool PointsAccumulateByDogOnly { get; set; } = false;

        // Display ordering
        public int? PrintOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    public class MSSA_Trial
    {
        [Key]
        public int TrialId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Trial identifier is required")]
        [StringLength(50)]
        public string TrialIdentifier { get; set; }

        [Required(ErrorMessage = "Trial date is required")]
        public DateTime TrialDate { get; set; }

        [StringLength(255)]
        public string TrialName { get; set; }

        [StringLength(20)]
        public string Stock { get; set; } // Cattle, Sheep, Ducks

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

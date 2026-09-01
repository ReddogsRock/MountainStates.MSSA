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

        [StringLength(20)]
        public string Venue { get; set; } // Arena, Field

        // Who's entering scores for this trial (Oqtane UserId). Unlike Event ownership,
        // a Scorekeeper doesn't create anything - they're explicitly assigned per Trial by
        // the Trial Secretary/Admin, since a multi-day event can have a different
        // Scorekeeper each day. Null means no Scorekeeper has been assigned yet.
        public int? ScorekeeperUserId { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

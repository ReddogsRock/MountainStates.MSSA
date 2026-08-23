using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    public class MSSA_Handler
    {
        [Key]
        public int HandlerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        // Database-computed persisted column ("FirstName + ' ' + LastName") - read-only
        // from EF's perspective, never set directly.
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(20)]
        public string AlternatePhone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(2)]
        public string StateCode { get; set; }

        [StringLength(10)]
        public string ZipCode { get; set; }

        [StringLength(20)]
        public string HandlerLevel { get; set; }

        public DateTime? LevelMoveUpDate { get; set; }

        public bool PhotoReleaseConsent { get; set; } = false;

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties (not mapped to DB)
        [NotMapped]
        public string StateName { get; set; }

        // All membership periods this handler has ever been linked to (current and
        // historical), populated by the repository - newest first.
        [NotMapped]
        public List<MSSA_Membership> Memberships { get; set; } = new();

        [NotMapped]
        public bool HasActiveMembership => Memberships != null && Memberships.Any(m => m.IsCurrentlyActive);
    }
}

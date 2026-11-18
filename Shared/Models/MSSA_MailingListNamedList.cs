using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // A reusable, admin-named audience for "Send a Campaign" (e.g. "Board Members") -
    // membership is tracked separately in MSSA_MailingListNamedListMembers.
    public class MSSA_MailingListNamedList
    {
        [Key]
        public int NamedListId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Populated by the repository for display (not a persisted column) - how many
        // active subscribers currently belong to this list.
        [NotMapped]
        public int MemberCount { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

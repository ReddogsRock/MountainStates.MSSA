using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // One-time merge target for three sources - MSSA_Handlers, the Oqtane User table,
    // and an admin-provided Excel export - deduplicated by email. Not a live sync:
    // IsHandler is a snapshot taken at import time, not a link back to MSSA_Handlers,
    // so a Handler's info changing later doesn't retroactively update this table.
    public class MSSA_MailingListSubscriber
    {
        [Key]
        public int SubscriberId { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        // Set at import time if this email matched a row in MSSA_Handlers - lets a
        // future "Handlers only" announcement filter on this directly, with no join
        // back to MSSA_Handlers needed at send time.
        public bool IsHandler { get; set; } = false;

        // False once unsubscribed. Rows are kept (not deleted) so a re-import or
        // resubscribe doesn't silently re-add someone who opted out.
        public bool IsActive { get; set; } = true;
        public DateTime? UnsubscribedDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

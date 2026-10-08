using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    // A Trial Secretary other than the Event's creator who's been given the same
    // management access (trials, entries, submitting for approval) on this one Event.
    // The creator (MSSA_Event.CreatedByUserId) is always authorized and never appears
    // here - this table only holds the additional teammates.
    public class MSSA_EventTeamMember
    {
        [Key]
        public int EventTeamMemberId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

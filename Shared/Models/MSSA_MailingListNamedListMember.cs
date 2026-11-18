using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // One subscriber's membership in one named list (MSSA_MailingListNamedList). A
    // subscriber can belong to more than one list - membership is fully replaced on
    // each save, not individually tracked, so there's nothing else on this row.
    public class MSSA_MailingListNamedListMember
    {
        [Key]
        public int NamedListMemberId { get; set; }

        [Required]
        public int NamedListId { get; set; }

        [Required]
        public int SubscriberId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

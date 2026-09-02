using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    // One row per ownership transfer, oldest first. MSSA_Dog.OwnerName always holds
    // the current owner - this is purely the audit trail of who held it before.
    public class MSSA_DogOwnershipHistory
    {
        [Key]
        public int DogOwnershipHistoryId { get; set; }

        [Required]
        public int DogId { get; set; }

        [StringLength(255)]
        public string PreviousOwnerName { get; set; }

        [Required]
        [StringLength(255)]
        public string NewOwnerName { get; set; }

        public DateTime TransferDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

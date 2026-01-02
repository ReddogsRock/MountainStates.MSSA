using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    public class MSSA_HandlerMembership
    {
        [Key]
        public int MembershipId { get; set; }

        [Required]
        public int HandlerId { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int StartYear { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int EndYear { get; set; }

        [Range(0, 10000)]
        public decimal? Amount { get; set; }

        [StringLength(50)]
        public string PaidBy { get; set; }

        public DateTime? DateReceived { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
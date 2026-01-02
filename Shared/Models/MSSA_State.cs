using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    public class MSSA_State
    {
        [Key]
        [StringLength(2)]
        public string StateCode { get; set; }

        [Required]
        [StringLength(100)]
        public string StateName { get; set; }

        [Required]
        [StringLength(2)]
        public string Country { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

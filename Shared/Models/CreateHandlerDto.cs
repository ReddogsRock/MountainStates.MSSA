using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for creating a new handler from trial secretary
    /// </summary>
    public class CreateHandlerDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255)]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(2)]
        public string StateCode { get; set; }
    }
}

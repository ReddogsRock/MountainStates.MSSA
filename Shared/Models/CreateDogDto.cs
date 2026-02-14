using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for creating a new dog from trial secretary
    /// </summary>
    public class CreateDogDto
    {
        [Required(ErrorMessage = "Dog name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Breed { get; set; }

        [StringLength(255)]
        public string OwnerName { get; set; }
    }
}

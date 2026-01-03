using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    public class MSSA_DogFuturityParticipation
    {
        [Key]
        public int ParticipationId { get; set; }

        [Required]
        public int DogId { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }
    }
}

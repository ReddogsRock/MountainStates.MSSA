namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    public class UpdateDogStatusDto
    {
        public int DogId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeceased { get; set; }
    }
}

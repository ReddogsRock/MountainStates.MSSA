namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for dog search/selection
    /// </summary>
    public class DogSearchDto
    {
        public int DogId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public string OwnerName { get; set; }
        public int? Age { get; set; }
        
        public string DisplayInfo
        {
            get
            {
                var info = $"{Name} ({DogId})";
                if (!string.IsNullOrWhiteSpace(Breed))
                    info += $" - {Breed}";
                if (!string.IsNullOrWhiteSpace(OwnerName))
                    info += $" - Owner: {OwnerName}";
                return info;
            }
        }
    }
}

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for handler search/selection
    /// </summary>
    public class HandlerSearchDto
    {
        public int HandlerId { get; set; }
        public string FullName { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool HasActiveMembership { get; set; }
        
        public string DisplayInfo => $"{FullName} ({HandlerId}) - {City}, {StateCode}";
    }
}

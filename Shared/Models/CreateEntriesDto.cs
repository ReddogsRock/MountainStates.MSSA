using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for creating multiple trial entries at once
    /// </summary>
    public class CreateEntriesDto
    {
        [Required]
        public int HandlerId { get; set; }
        
        [Required]
        public int DogId { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "At least one trial entry is required")]
        public List<TrialEntryDto> TrialEntries { get; set; } = new List<TrialEntryDto>();
    }
    
    /// <summary>
    /// Individual trial entry
    /// </summary>
    public class TrialEntryDto
    {
        [Required]
        public int TrialId { get; set; }
        
        [Required]
        public int ClassId { get; set; }
    }
}

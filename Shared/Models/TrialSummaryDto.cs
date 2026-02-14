using System;
using System.Collections.Generic;

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for displaying trial information with available classes
    /// </summary>
    public class TrialSummaryDto
    {
        public int TrialId { get; set; }
        public string TrialIdentifier { get; set; }
        public string TrialName { get; set; }
        public DateTime TrialDate { get; set; }
        public string Stock { get; set; }
        
        public List<ClassOptionDto> AvailableClasses { get; set; } = new List<ClassOptionDto>();
    }
}

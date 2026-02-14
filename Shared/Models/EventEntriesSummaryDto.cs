using System.Collections.Generic;

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for displaying entries grouped by trial and class
    /// </summary>
    public class EventEntriesSummaryDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public List<TrialEntriesDto> Trials { get; set; } = new List<TrialEntriesDto>();
    }

    public class TrialEntriesDto
    {
        public int TrialId { get; set; }
        public string TrialName { get; set; }
        public string TrialIdentifier { get; set; }
        public string Stock { get; set; }
        public List<ClassEntriesDto> Classes { get; set; } = new List<ClassEntriesDto>();
    }

    public class ClassEntriesDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string SubClassName { get; set; }
        public List<EntryDetailDto> Entries { get; set; } = new List<EntryDetailDto>();
        
        public string DisplayName
        {
            get
            {
                var fullName = string.IsNullOrWhiteSpace(SubClassName) 
                    ? ClassName 
                    : $"{ClassName} - {SubClassName}";
                
                // Remove "On-Foot" variations since it's the inferred/default state
                fullName = fullName
                    .Replace("On-Foot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("On Foot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("Onfoot", "", System.StringComparison.OrdinalIgnoreCase)
                    .Replace("  ", " ")
                    .Replace("- -", "-")
                    .Replace(" -", "-")
                    .Replace("- ", "-")
                    .Trim()
                    .Trim('-')
                    .Trim();
                
                return fullName;
            }
        }
    }

    public class EntryDetailDto
    {
        public int EntryId { get; set; }
        public string HandlerName { get; set; }
        public string DogName { get; set; }
        public bool HandlerIsMSSAMember { get; set; }
        public int? RunOrder { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace MountainStates.MSSA.Module.TrialSecretary.Models
{
    /// <summary>
    /// DTO for displaying recent events with their trials
    /// </summary>
    public class RecentEventDto
    {
        public int EventId { get; set; }
        public string EventIdentifier { get; set; }
        public string EventName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        
        public List<TrialSummaryDto> Trials { get; set; } = new List<TrialSummaryDto>();
        
        public string DateRange
        {
            get
            {
                if (!StartDate.HasValue) return "Date TBD";
                if (!EndDate.HasValue || StartDate.Value.Date == EndDate.Value.Date)
                    return StartDate.Value.ToString("MMM dd, yyyy");
                return $"{StartDate.Value:MMM dd} - {EndDate.Value:MMM dd, yyyy}";
            }
        }
    }
}

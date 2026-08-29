using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MountainStates.MSSA.Module.MSSA_Results.Enums;

namespace MountainStates.MSSA.Module.MSSA_Events.Models
{
    public class MSSA_Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event identifier is required")]
        [StringLength(50)]
        public string EventIdentifier { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(255)]
        public string EventName { get; set; }

        // Location
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(2)]
        public string StateCode { get; set; }

        // Dates
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PointYear { get; set; }

        // Event Leadership
        [StringLength(255)]
        public string ChairmanName { get; set; }

        [StringLength(20)]
        public string ChairmanPhone { get; set; }

        [StringLength(255)]
        public string ChairmanEmail { get; set; }

        [StringLength(500)]
        public string EntryLink { get; set; }

        [StringLength(500)]
        public string FlyerFileName { get; set; }

        [StringLength(500)]
        public string FlyerPath { get; set; }

        // Sanctioning
        public bool IsMSSASanctioned { get; set; } = false;

        // Administrative Tracking
        public DateTime? ResultsReceivedDate { get; set; }
        public bool ResultsUploaded { get; set; } = false;
        public decimal? SanctionFee { get; set; }
        public DateTime? FeeReceivedDate { get; set; }

        // Planning
        public int? NumberOfRuns { get; set; }
        public string Notes { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Owner of this event (Oqtane UserId). Null for events created before this field
        // existed, or by an Admin acting outside the Trial Secretary ownership model.
        // Trial Secretaries may only edit/delete events where this matches their own UserId.
        public int? CreatedByUserId { get; set; }

        // Results approval workflow (see EventResultsStatus). One-way: NotSubmitted ->
        // PendingApproval -> Approved. Approval isn't a data-quality check - it's the
        // admin's sign-off that the event's sanctioning fee has been paid, which is why
        // there's no "rejected" state. Public views and cross-event rollups (TopScores)
        // only show results once Approved.
        [StringLength(20)]
        public string ResultsApprovalStatus { get; set; } = EventResultsStatus.NotSubmitted;
        public DateTime? ResultsSubmittedDate { get; set; }
        public int? ResultsSubmittedByUserId { get; set; }
        public DateTime? ResultsApprovedDate { get; set; }
        public int? ResultsApprovedByUserId { get; set; }

        // Navigation properties (not mapped to DB)
        [NotMapped]
        public string StateName { get; set; }

        [NotMapped]
        public int TrialCount { get; set; }

        // Planned runs per Class/Stock/Venue - replaces the old boolean flags.
        // Populated by the repository alongside the event; "does this event
        // offer Cattle/Open/Arena etc." is now derived by checking this list
        // rather than reading a dedicated boolean column.
        [NotMapped]
        public List<MSSA_EventClassOffering> Offerings { get; set; } = new();

        // Transient carriers used only for the flyer upload round-trip (not persisted).
        // Same pattern as MSSA_DogFuturityParticipation's upload fields - lets the client
        // post and receive the same type T through ServiceBase's PostJsonAsync<T>.
        [NotMapped]
        public string UploadFlyerFileName { get; set; }

        [NotMapped]
        public string UploadFlyerContentBase64 { get; set; }

        [NotMapped]
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

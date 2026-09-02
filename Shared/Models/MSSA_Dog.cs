using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    public class MSSA_Dog
    {
        [Key]
        public int DogId { get; set; }

        [Required(ErrorMessage = "Dog name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        // Dog Information
        [StringLength(100)]
        public string Breed { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string RegistrationNumber { get; set; }

        public int? FirstCompetitionYear { get; set; }

        // Ownership Information
        [StringLength(255)]
        public string OwnerName { get; set; }

        // No longer editable or displayed anywhere in the UI (removed from Edit and
        // Detail per request), but left on the model since Search/Repository/Manager/
        // Service/Controller still reference it for the search-by-member filter.
        // Removing the property outright would require touching all of those.
        public bool OwnerIsMSSAMember { get; set; } = false;

        // Status Flags
        public bool IsDeceased { get; set; } = false;

        // No longer settable from the UI - ownership changes are now tracked via
        // MSSA_DogOwnershipHistory instead of a boolean flag (see OwnershipHistory
        // below). Left on the model so any dogs already flagged sold keep showing
        // that way in the Index search grid's badge.
        public bool IsSold { get; set; } = false;

        // Nursery age-eligibility documentation. Unlike Futurity, there's no
        // per-year nomination record for Nursery - just one document attached
        // directly to the dog, uploadable/replaceable any time (Add or Edit).
        [StringLength(500)]
        public string NurseryDocumentFileName { get; set; }

        [StringLength(500)]
        public string NurseryDocumentPath { get; set; }

        public DateTime? NurseryDocumentUploadedDate { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Computed properties (not mapped to DB)
        [NotMapped]
        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return null;
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;
                if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [NotMapped]
        public string DisplayStatus
        {
            get
            {
                if (IsDeceased) return "Deceased";
                if (IsSold) return "Sold";
                if (!IsActive) return "Inactive";
                return "Active";
            }
        }

        // Transient carriers used only for the Nursery document upload round-trip (not
        // persisted). Same pattern as the Futurity document/Event flyer uploads - lets
        // the client post and receive the same type T through ServiceBase's PostJsonAsync<T>.
        [NotMapped]
        public string UploadNurseryDocFileName { get; set; }

        [NotMapped]
        public string UploadNurseryDocContentBase64 { get; set; }

        // Populated by the repository join when loading a single dog - not persisted here.
        [NotMapped]
        public List<MSSA_DogOwnershipHistory> OwnershipHistory { get; set; } = new();
    }
}

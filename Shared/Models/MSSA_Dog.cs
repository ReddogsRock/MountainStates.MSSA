using System;
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

        // No longer editable via the UI (checkbox removed from Edit.razor per request),
        // but left on the model since Search/Repository/Manager/Service/Controller and
        // the Index/Detail pages still reference it - removing the property outright
        // would require touching all of those. Existing data and search-by-member
        // capability stay intact; the field just can't be set from this page anymore.
        public bool OwnerIsMSSAMember { get; set; } = false;

        // Status Flags
        public bool IsDeceased { get; set; } = false;
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
    }
}

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

        public bool OwnerIsMSSAMember { get; set; } = false;

        // Status Flags
        public bool IsDeceased { get; set; } = false;
        public bool IsSold { get; set; } = false;

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
    }
}

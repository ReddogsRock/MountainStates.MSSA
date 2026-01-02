using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    public class MSSA_Handler
    {
        [Key]
        public int HandlerId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }

        // Contact Information
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(255)]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Phone(ErrorMessage = "Invalid alternate phone number")]
        [StringLength(20)]
        public string AlternatePhone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(2)]
        public string StateCode { get; set; }

        [StringLength(10)]
        public string ZipCode { get; set; }

        // Competition Information
        [StringLength(20)]
        public string HandlerLevel { get; set; }

        public DateTime? LevelMoveUpDate { get; set; }

        // Family Membership Link
        public int? FamilyMemberHandlerId { get; set; }

        // Membership
        public bool PhotoReleaseConsent { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties (not mapped to DB)
        [NotMapped]
        public string StateName { get; set; }

        [NotMapped]
        public string FamilyMemberName { get; set; }

        [NotMapped]
        public bool HasActiveMembership { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // One purchase/renewal period - NOT one row per handler, NOT one row per year.
    // A Family membership covers however many handlers are linked via
    // MSSA_MembershipHandlers; renewal history is preserved because renewing creates
    // a new row here rather than overwriting this one.
    public class MSSA_Membership
    {
        [Key]
        public int MembershipId { get; set; }

        // AI = Annual Individual, AF = Annual Family, 3I = 3-Year Individual,
        // 3F = 3-Year Family, Lifetime.
        [Required]
        [StringLength(10)]
        public string MembershipType { get; set; }

        [Required]
        public int StartYear { get; set; }

        // Null = Lifetime (never expires). Computed server-side from
        // MembershipType + StartYear when the membership is created.
        public int? EndYear { get; set; }

        public decimal? Amount { get; set; }

        [StringLength(50)]
        public string PaidBy { get; set; }

        public DateTime? DateReceived { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Populated by the repository join - not persisted here.
        [NotMapped]
        public List<MembershipMemberInfo> Members { get; set; } = new();

        // Transient carriers used only when creating a membership (not persisted as
        // columns) - the list of every handler this membership covers, and which one
        // is the primary/purchasing account holder. Same pattern as the Futurity
        // document/Event flyer uploads: lets the client post and receive the same
        // type T through ServiceBase's PostJsonAsync<T>.
        [NotMapped]
        public List<int> MemberHandlerIds { get; set; } = new();

        [NotMapped]
        public int PrimaryHandlerId { get; set; }

        // Membership years run Jan 1 - Dec 31 (calendar year), confirmed against the
        // legacy system's convention - a plain integer-year comparison, not date math.
        [NotMapped]
        public bool IsCurrentlyActive
        {
            get
            {
                var currentYear = DateTime.Today.Year;
                return StartYear <= currentYear && (EndYear == null || EndYear >= currentYear);
            }
        }

        // Display-friendly name for the stored code - one place to maintain this
        // mapping so every page that shows a membership uses the same wording.
        [NotMapped]
        public string MembershipTypeName
        {
            get
            {
                return MembershipType switch
                {
                    "AI" => "Annual Individual",
                    "AF" => "Annual Family",
                    "3I" => "3 Year Individual",
                    "3F" => "3 Year Family",
                    "Lifetime" => "Lifetime",
                    "Unknown" => "Unknown",
                    _ => MembershipType
                };
            }
        }
    }

    // The join table itself - kept as its own mapped entity for direct add/remove
    // operations on membership.
    public class MSSA_MembershipHandler
    {
        [Key]
        public int MembershipHandlerId { get; set; }

        [Required]
        public int MembershipId { get; set; }

        [Required]
        public int HandlerId { get; set; }

        // Flags the purchasing/primary account holder vs. a family member riding
        // along on the same membership - useful for display, not for eligibility
        // (every linked handler is equally "covered" regardless of this flag).
        public bool IsPrimary { get; set; }
    }

    // Display-only shape for one member row on a membership (used inside
    // MSSA_Membership.Members) - not a database entity itself.
    public class MembershipMemberInfo
    {
        public int HandlerId { get; set; }
        public string HandlerName { get; set; }
        public string Email { get; set; }
        public bool IsPrimary { get; set; }
    }
}

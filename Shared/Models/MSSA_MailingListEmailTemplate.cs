using System;
using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // Saved email content for "Send a Campaign" (e.g. "Annual Renewal") - lets the
    // admin reuse a Subject + Body on future sends instead of recomposing it from
    // scratch. Separate from MSSA_MailingListNamedList, which saves the audience
    // (who), not the content (what).
    public class MSSA_MailingListEmailTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Subject { get; set; }

        [Required]
        public string BodyHtml { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

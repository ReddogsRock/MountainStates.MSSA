using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // What the admin submits from the "Send a Campaign" composer. Subject/BodyHtml may
    // contain the friendly {FirstName}/{RenewalLink} placeholders - see
    // MailgunService.SendCampaignAsync for where those get swapped for Mailgun's own
    // merge syntax.
    public class SendCampaignDto
    {
        [Required]
        [StringLength(255)]
        public string Subject { get; set; }

        [Required]
        public string BodyHtml { get; set; }

        // One of MailingListAudience's constants.
        [Required]
        public string Audience { get; set; }

        // Required when Audience == MailingListAudience.NamedList, ignored otherwise.
        public int? NamedListId { get; set; }
    }
}

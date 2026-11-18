using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // Just the audience-selection part of SendCampaignDto - used for the live
    // recipient-count preview, which doesn't need Subject/BodyHtml yet.
    public class CampaignAudienceDto
    {
        [Required]
        public string Audience { get; set; }

        public int? NamedListId { get; set; }
    }
}

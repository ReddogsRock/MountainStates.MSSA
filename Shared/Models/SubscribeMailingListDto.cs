using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // Deliberately minimal - this is what an anonymous site visitor can submit through
    // the public "Subscribe" module. Never expose the full MSSA_MailingListSubscriber
    // model on a public endpoint: that would let an anonymous caller set things like
    // IsHandler or target an arbitrary SubscriberId.
    public class SubscribeMailingListDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        // Honeypot - a real visitor never sees or fills this field (hidden via CSS in
        // the form). If it's non-empty, the submission is silently dropped as a bot.
        public string Website { get; set; }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Manager
{
    public interface IMailgunService
    {
        // Sends the "new event" announcement to the MSSA mailing list via Mailgun.
        // baseUrl is the site's own public base URL (scheme + host of the current
        // request) - used to build an absolute link back to the event's flyer, since
        // a relative link means nothing in an email client. Fire-and-forget by design,
        // same contract as IMSSA_AdminNotificationService: never throws, so a Mailgun
        // outage never fails the approval action that triggered it.
        Task SendEventAnnouncementAsync(MSSA_Event evt, string baseUrl, int moduleId);

        // Sends an admin-composed "Send a Campaign" email (subject + bodyHtml, each
        // optionally containing the {FirstName}/{RenewalLink} placeholders) to the
        // given recipients. Unlike SendEventAnnouncementAsync, this is a deliberate,
        // in-the-moment admin action - the admin needs to know if it actually worked,
        // so failures are NOT swallowed here; they propagate so the controller can
        // report a real error instead of a false "sent" success. Returns the number
        // of recipients sent to.
        Task<int> SendCampaignAsync(string subject, string bodyHtml, List<MSSA_MailingListSubscriber> recipients, int moduleId);
    }
}

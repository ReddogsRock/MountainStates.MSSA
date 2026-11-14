using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Events.Models;

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
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public interface IMSSA_MailingListRepository
    {
        // Everyone currently subscribed (IsActive) - what an announcement send goes to.
        Task<List<MSSA_MailingListSubscriber>> GetActiveSubscribersAsync();

        // Admin screen search - matches Email/FirstName/LastName (contains), active and
        // unsubscribed alike, capped at 100 results so a blank/short search doesn't try
        // to render the whole list at once.
        Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, bool expiringOnly);

        Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber);
        Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber);
        Task DeleteSubscriberAsync(int subscriberId);

        // Public self-service signup. Idempotent by design - an email that's already
        // active just succeeds quietly, and one that previously unsubscribed is
        // reactivated, since this is reachable by anyone without logging in and a
        // visitor re-submitting shouldn't see an error either way.
        Task SubscribeAsync(string email, string firstName, string lastName);

        // Admin-triggered annual refresh - flags subscribers whose latest real
        // membership expires at the end of the current calendar year, inserting them
        // into the subscriber table if they're not already there, and clearing the
        // flag from anyone who's since renewed. Returns the count flagged.
        Task<int> SyncExpiringMembershipSegmentAsync();

        // CSV export target for the segment above - active subscribers only, with
        // HandlerId attached (looked up by email) for building a renewal link.
        Task<List<MSSA_ExpiringMembershipExportRow>> GetExpiringMembershipExportRowsAsync();

        // Email -> HandlerId, lowercased/trimmed key. Shared by the CSV export above
        // and the {RenewalLink} merge in MailgunService.SendCampaignAsync so both
        // build the same link the same way.
        Task<Dictionary<string, int>> GetHandlerIdsByEmailAsync();

        // Reusable, admin-named "Send a Campaign" audiences (e.g. "Board Members").
        Task<List<MSSA_MailingListNamedList>> GetNamedListsAsync();
        Task<List<MSSA_MailingListSubscriber>> GetNamedListMembersAsync(int namedListId);
        Task<MSSA_MailingListNamedList> AddNamedListAsync(string name);
        Task<MSSA_MailingListNamedList> RenameNamedListAsync(int namedListId, string name);
        Task DeleteNamedListAsync(int namedListId);

        // Replaces a named list's full membership in one go - simplest model for a
        // "pick who's in this list" UI, not an incremental add/remove.
        Task SetNamedListMembersAsync(int namedListId, List<int> subscriberIds);

        // Resolves a "Send a Campaign" audience (MailingListAudience.All/Expiring/NamedList)
        // to the subscribers it actually goes to - always IsActive-filtered, regardless
        // of audience, so unsubscribes are respected everywhere.
        Task<List<MSSA_MailingListSubscriber>> GetCampaignRecipientsAsync(string audience, int? namedListId);

        // Saved email content ("Annual Renewal", etc.) so a campaign doesn't need to be
        // recomposed from scratch every time it's sent.
        Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplatesAsync();
        Task<MSSA_MailingListEmailTemplate> GetEmailTemplateAsync(int templateId);
        Task<MSSA_MailingListEmailTemplate> AddEmailTemplateAsync(MSSA_MailingListEmailTemplate template);
        Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplateAsync(MSSA_MailingListEmailTemplate template);
        Task DeleteEmailTemplateAsync(int templateId);
    }
}

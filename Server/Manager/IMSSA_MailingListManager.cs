using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Manager
{
    public interface IMSSA_MailingListManager
    {
        Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, bool expiringOnly, int moduleId);
        Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId);
        Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId);
        Task DeleteSubscriberAsync(int subscriberId, int moduleId);
        Task SubscribeAsync(SubscribeMailingListDto dto, int moduleId);
        Task<int> SyncExpiringMembershipSegmentAsync(int moduleId);
        Task<List<MSSA_ExpiringMembershipExportRow>> GetExpiringMembershipExportRowsAsync(int moduleId);

        Task<List<MSSA_MailingListNamedList>> GetNamedListsAsync(int moduleId);
        Task<List<MSSA_MailingListSubscriber>> GetNamedListMembersAsync(int namedListId, int moduleId);
        Task<MSSA_MailingListNamedList> AddNamedListAsync(string name, int moduleId);
        Task<MSSA_MailingListNamedList> RenameNamedListAsync(int namedListId, string name, int moduleId);
        Task DeleteNamedListAsync(int namedListId, int moduleId);
        Task SetNamedListMembersAsync(int namedListId, List<int> subscriberIds, int moduleId);

        Task<List<MSSA_MailingListSubscriber>> GetCampaignRecipientsAsync(string audience, int? namedListId, int moduleId);

        Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplatesAsync(int moduleId);
        Task<MSSA_MailingListEmailTemplate> GetEmailTemplateAsync(int templateId, int moduleId);
        Task<MSSA_MailingListEmailTemplate> AddEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId);
        Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId);
        Task DeleteEmailTemplateAsync(int templateId, int moduleId);

        // Ensures a subscriber exists for each role-holder, then replaces the named
        // list's membership with exactly that set. Returns the count synced.
        Task<int> SyncNamedListFromRoleAsync(int namedListId, List<(string Email, string DisplayName)> people, int moduleId);
    }
}

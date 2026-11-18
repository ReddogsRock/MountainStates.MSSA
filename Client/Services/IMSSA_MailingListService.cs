using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Services
{
    public interface IMSSA_MailingListService
    {
        Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, bool expiringOnly, int moduleId);
        Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId);
        Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId);
        Task DeleteSubscriberAsync(int subscriberId, int moduleId);

        // Public self-service signup - no auth policy involved, unlike everything else
        // on this service.
        Task SubscribeAsync(SubscribeMailingListDto dto, int moduleId);

        // Refreshes the expiring-membership segment and returns how many were flagged.
        // The CSV export itself is a direct browser download link, not a service call -
        // see ExpiringMembersExportUrl in Index.razor.
        Task<int> SyncExpiringMembershipSegmentAsync(int moduleId);

        // Reusable, admin-named "Send a Campaign" audiences.
        Task<List<MSSA_MailingListNamedList>> GetNamedListsAsync(int moduleId);
        Task<List<MSSA_MailingListSubscriber>> GetNamedListMembersAsync(int namedListId, int moduleId);
        Task<MSSA_MailingListNamedList> AddNamedListAsync(string name, int moduleId);
        Task<MSSA_MailingListNamedList> RenameNamedListAsync(int namedListId, string name, int moduleId);
        Task DeleteNamedListAsync(int namedListId, int moduleId);
        Task SetNamedListMembersAsync(int namedListId, List<int> subscriberIds, int moduleId);

        // Saved email content ("Annual Renewal", etc.) reusable across sends.
        Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplatesAsync(int moduleId);
        Task<MSSA_MailingListEmailTemplate> AddEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId);
        Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId);
        Task DeleteEmailTemplateAsync(int templateId, int moduleId);

        Task<int> GetCampaignRecipientCountAsync(CampaignAudienceDto dto, int moduleId);
        Task<int> SendCampaignAsync(SendCampaignDto dto, int moduleId);
    }
}

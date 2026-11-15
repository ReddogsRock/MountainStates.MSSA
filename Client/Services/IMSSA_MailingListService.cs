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
    }
}

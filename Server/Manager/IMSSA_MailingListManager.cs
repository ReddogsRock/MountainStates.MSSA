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
        Task<List<MSSA_MailingListSubscriber>> GetExpiringMembershipSubscribersAsync(int moduleId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Services
{
    public interface IMSSA_MailingListService
    {
        Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, int moduleId);
        Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId);
        Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId);
        Task DeleteSubscriberAsync(int subscriberId, int moduleId);

        // Public self-service signup - no auth policy involved, unlike everything else
        // on this service.
        Task SubscribeAsync(SubscribeMailingListDto dto, int moduleId);
    }
}

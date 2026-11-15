using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Repository;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Manager
{
    public class MSSA_MailingListManager : IMSSA_MailingListManager, ITransientService
    {
        private readonly IMSSA_MailingListRepository _repository;

        public MSSA_MailingListManager(IMSSA_MailingListRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, bool expiringOnly, int moduleId)
        {
            return await _repository.SearchSubscribersAsync(searchTerm, expiringOnly);
        }

        public async Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId)
        {
            return await _repository.AddSubscriberAsync(subscriber);
        }

        public async Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId)
        {
            return await _repository.UpdateSubscriberAsync(subscriber);
        }

        public async Task DeleteSubscriberAsync(int subscriberId, int moduleId)
        {
            await _repository.DeleteSubscriberAsync(subscriberId);
        }

        public async Task SubscribeAsync(SubscribeMailingListDto dto, int moduleId)
        {
            await _repository.SubscribeAsync(dto.Email, dto.FirstName, dto.LastName);
        }

        public async Task<int> SyncExpiringMembershipSegmentAsync(int moduleId)
        {
            return await _repository.SyncExpiringMembershipSegmentAsync();
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetExpiringMembershipSubscribersAsync(int moduleId)
        {
            return await _repository.GetExpiringMembershipSubscribersAsync();
        }
    }
}

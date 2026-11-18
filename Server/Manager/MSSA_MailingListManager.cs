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

        public async Task<List<MSSA_ExpiringMembershipExportRow>> GetExpiringMembershipExportRowsAsync(int moduleId)
        {
            return await _repository.GetExpiringMembershipExportRowsAsync();
        }

        public async Task<List<MSSA_MailingListNamedList>> GetNamedListsAsync(int moduleId)
        {
            return await _repository.GetNamedListsAsync();
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetNamedListMembersAsync(int namedListId, int moduleId)
        {
            return await _repository.GetNamedListMembersAsync(namedListId);
        }

        public async Task<MSSA_MailingListNamedList> AddNamedListAsync(string name, int moduleId)
        {
            return await _repository.AddNamedListAsync(name);
        }

        public async Task<MSSA_MailingListNamedList> RenameNamedListAsync(int namedListId, string name, int moduleId)
        {
            return await _repository.RenameNamedListAsync(namedListId, name);
        }

        public async Task DeleteNamedListAsync(int namedListId, int moduleId)
        {
            await _repository.DeleteNamedListAsync(namedListId);
        }

        public async Task SetNamedListMembersAsync(int namedListId, List<int> subscriberIds, int moduleId)
        {
            await _repository.SetNamedListMembersAsync(namedListId, subscriberIds);
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetCampaignRecipientsAsync(string audience, int? namedListId, int moduleId)
        {
            return await _repository.GetCampaignRecipientsAsync(audience, namedListId);
        }

        public async Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplatesAsync(int moduleId)
        {
            return await _repository.GetEmailTemplatesAsync();
        }

        public async Task<MSSA_MailingListEmailTemplate> GetEmailTemplateAsync(int templateId, int moduleId)
        {
            return await _repository.GetEmailTemplateAsync(templateId);
        }

        public async Task<MSSA_MailingListEmailTemplate> AddEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId)
        {
            return await _repository.AddEmailTemplateAsync(template);
        }

        public async Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId)
        {
            return await _repository.UpdateEmailTemplateAsync(template);
        }

        public async Task DeleteEmailTemplateAsync(int templateId, int moduleId)
        {
            await _repository.DeleteEmailTemplateAsync(templateId);
        }
    }
}

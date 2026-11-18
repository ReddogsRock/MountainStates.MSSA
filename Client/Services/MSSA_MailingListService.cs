using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Services
{
    public class MSSA_MailingListService : ServiceBase, IMSSA_MailingListService, IService
    {
        public MSSA_MailingListService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_MailingList");

        public async Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, bool expiringOnly, int moduleId)
        {
            var encoded = WebUtility.UrlEncode(searchTerm ?? "");
            return await GetJsonAsync<List<MSSA_MailingListSubscriber>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/search?searchTerm={encoded}&expiringOnly={expiringOnly}&moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId)
        {
            return await PostJsonAsync<MSSA_MailingListSubscriber>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId), subscriber);
        }

        public async Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber, int moduleId)
        {
            return await PutJsonAsync<MSSA_MailingListSubscriber>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/{subscriber.SubscriberId}?moduleid={moduleId}", EntityNames.Module, moduleId), subscriber);
        }

        public async Task DeleteSubscriberAsync(int subscriberId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{subscriberId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task SubscribeAsync(SubscribeMailingListDto dto, int moduleId)
        {
            // Plain CreateApiUrl deliberately - no CreateAuthorizationPolicyUrl wrapper,
            // since the server action is [AllowAnonymous] and isn't gated by a module
            // permission policy at all.
            await PostJsonAsync<SubscribeMailingListDto, bool>($"{ApiUrl}/subscribe?moduleid={moduleId}", dto);
        }

        public async Task<int> SyncExpiringMembershipSegmentAsync(int moduleId)
        {
            return await PostJsonAsync<object, int>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/sync-expiring?moduleid={moduleId}", EntityNames.Module, moduleId), null);
        }

        public async Task<List<MSSA_MailingListNamedList>> GetNamedListsAsync(int moduleId)
        {
            return await GetJsonAsync<List<MSSA_MailingListNamedList>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/named-lists?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetNamedListMembersAsync(int namedListId, int moduleId)
        {
            return await GetJsonAsync<List<MSSA_MailingListSubscriber>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/named-lists/{namedListId}/members?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_MailingListNamedList> AddNamedListAsync(string name, int moduleId)
        {
            return await PostJsonAsync<MSSA_MailingListNamedList>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/named-lists?moduleid={moduleId}", EntityNames.Module, moduleId),
                new MSSA_MailingListNamedList { Name = name });
        }

        public async Task<MSSA_MailingListNamedList> RenameNamedListAsync(int namedListId, string name, int moduleId)
        {
            return await PutJsonAsync<MSSA_MailingListNamedList>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/named-lists/{namedListId}?moduleid={moduleId}", EntityNames.Module, moduleId),
                new MSSA_MailingListNamedList { NamedListId = namedListId, Name = name });
        }

        public async Task DeleteNamedListAsync(int namedListId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/named-lists/{namedListId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task SetNamedListMembersAsync(int namedListId, List<int> subscriberIds, int moduleId)
        {
            await PutJsonAsync<List<int>, List<int>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/named-lists/{namedListId}/members?moduleid={moduleId}", EntityNames.Module, moduleId), subscriberIds);
        }

        public async Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplatesAsync(int moduleId)
        {
            return await GetJsonAsync<List<MSSA_MailingListEmailTemplate>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/email-templates?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_MailingListEmailTemplate> AddEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId)
        {
            return await PostJsonAsync<MSSA_MailingListEmailTemplate>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/email-templates?moduleid={moduleId}", EntityNames.Module, moduleId), template);
        }

        public async Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplateAsync(MSSA_MailingListEmailTemplate template, int moduleId)
        {
            return await PutJsonAsync<MSSA_MailingListEmailTemplate>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/email-templates/{template.TemplateId}?moduleid={moduleId}", EntityNames.Module, moduleId), template);
        }

        public async Task DeleteEmailTemplateAsync(int templateId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/email-templates/{templateId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<int> GetCampaignRecipientCountAsync(CampaignAudienceDto dto, int moduleId)
        {
            return await PostJsonAsync<CampaignAudienceDto, int>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/campaign/recipient-count?moduleid={moduleId}", EntityNames.Module, moduleId), dto);
        }

        public async Task<int> SendCampaignAsync(SendCampaignDto dto, int moduleId)
        {
            return await PostJsonAsync<SendCampaignDto, int>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/campaign/send?moduleid={moduleId}", EntityNames.Module, moduleId), dto);
        }
    }
}

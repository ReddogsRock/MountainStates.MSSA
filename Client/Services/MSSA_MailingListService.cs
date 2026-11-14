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

        public async Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, int moduleId)
        {
            var encoded = WebUtility.UrlEncode(searchTerm ?? "");
            return await GetJsonAsync<List<MSSA_MailingListSubscriber>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/search?searchTerm={encoded}&moduleid={moduleId}", EntityNames.Module, moduleId));
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
    }
}

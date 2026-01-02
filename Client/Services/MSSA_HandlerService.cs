using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Services
{
    public class MSSA_HandlerService : ServiceBase, IMSSA_HandlerService, IService
    {
        public MSSA_HandlerService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_Handler");

        public async Task<List<MSSA_Handler>> GetHandlersAsync(int moduleId)
        {
            var handlers = await GetJsonAsync<List<MSSA_Handler>>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
            return handlers?.OrderBy(h => h.LastName).ThenBy(h => h.FirstName).ToList();
        }

        public async Task<MSSA_Handler> GetHandlerAsync(int handlerId, int moduleId)
        {
            return await GetJsonAsync<MSSA_Handler>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{handlerId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Handler> AddHandlerAsync(MSSA_Handler handler, int moduleId)
        {
            return await PostJsonAsync<MSSA_Handler>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId), handler);
        }

        public async Task<MSSA_Handler> UpdateHandlerAsync(MSSA_Handler handler, int moduleId)
        {
            return await PutJsonAsync<MSSA_Handler>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{handler.HandlerId}?moduleid={moduleId}", EntityNames.Module, moduleId), handler);
        }

        public async Task DeleteHandlerAsync(int handlerId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{handlerId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_Handler>> SearchHandlersAsync(
            string searchTerm,
            string stateCode,
            string handlerLevel,
            bool? hasActiveMembership,
            int moduleId)
        {
            var queryParams = new List<string> { $"moduleid={moduleId}" };

            if (!string.IsNullOrWhiteSpace(searchTerm))
                queryParams.Add($"searchTerm={System.Uri.EscapeDataString(searchTerm)}");
            if (!string.IsNullOrWhiteSpace(stateCode))
                queryParams.Add($"stateCode={stateCode}");
            if (!string.IsNullOrWhiteSpace(handlerLevel))
                queryParams.Add($"handlerLevel={System.Uri.EscapeDataString(handlerLevel)}");
            if (hasActiveMembership.HasValue)
                queryParams.Add($"hasActiveMembership={hasActiveMembership.Value}");

            var url = $"{ApiUrl}/search?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<MSSA_Handler>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        // Memberships
        public async Task<List<MSSA_HandlerMembership>> GetHandlerMembershipsAsync(int handlerId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_HandlerMembership");
            return await GetJsonAsync<List<MSSA_HandlerMembership>>(
                CreateAuthorizationPolicyUrl($"{url}/handler/{handlerId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_HandlerMembership> AddMembershipAsync(MSSA_HandlerMembership membership, int moduleId)
        {
            var url = CreateApiUrl("MSSA_HandlerMembership");
            return await PostJsonAsync<MSSA_HandlerMembership>(
                CreateAuthorizationPolicyUrl($"{url}?moduleid={moduleId}", EntityNames.Module, moduleId), membership);
        }

        public async Task<MSSA_HandlerMembership> UpdateMembershipAsync(MSSA_HandlerMembership membership, int moduleId)
        {
            var url = CreateApiUrl("MSSA_HandlerMembership");
            return await PutJsonAsync<MSSA_HandlerMembership>(
                CreateAuthorizationPolicyUrl($"{url}/{membership.MembershipId}?moduleid={moduleId}", EntityNames.Module, moduleId), membership);
        }

        public async Task DeleteMembershipAsync(int membershipId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_HandlerMembership");
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{url}/{membershipId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        // Entries
        public async Task<List<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_HandlerEntry");
            return await GetJsonAsync<List<MSSA_HandlerEntry>>(
                CreateAuthorizationPolicyUrl($"{url}/handler/{handlerId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Services
{
    public class MSSA_EventService : ServiceBase, IMSSA_EventService, IService
    {
        public MSSA_EventService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_Event");

        // Events
        public async Task<List<MSSA_Event>> GetEventsAsync(int moduleId)
        {
            var events = await GetJsonAsync<List<MSSA_Event>>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
            return events?.OrderByDescending(e => e.StartDate).ToList();
        }

        public async Task<MSSA_Event> GetEventAsync(int eventId, int moduleId)
        {
            return await GetJsonAsync<MSSA_Event>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{eventId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Event> AddEventAsync(MSSA_Event evt, int moduleId)
        {
            return await PostJsonAsync<MSSA_Event>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId), evt);
        }

        public async Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt, int moduleId)
        {
            return await PutJsonAsync<MSSA_Event>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{evt.EventId}?moduleid={moduleId}", EntityNames.Module, moduleId), evt);
        }

        public async Task DeleteEventAsync(int eventId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{eventId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_Event>> SearchEventsAsync(
            string searchTerm,
            string stateCode,
            int? year,
            bool? cattle,
            bool? sheep,
            bool? arena,
            bool? field,
            bool? onFoot,
            bool? horseback,
            bool? open,
            bool? nursery,
            bool? intermediate,
            bool? novice,
            bool? junior,
            int moduleId)
        {
            var queryParams = new List<string> { $"moduleid={moduleId}" };

            if (!string.IsNullOrWhiteSpace(searchTerm))
                queryParams.Add($"searchTerm={System.Uri.EscapeDataString(searchTerm)}");
            if (!string.IsNullOrWhiteSpace(stateCode))
                queryParams.Add($"stateCode={stateCode}");
            if (year.HasValue)
                queryParams.Add($"year={year.Value}");
            if (cattle.HasValue)
                queryParams.Add($"cattle={cattle.Value}");
            if (sheep.HasValue)
                queryParams.Add($"sheep={sheep.Value}");
            if (arena.HasValue)
                queryParams.Add($"arena={arena.Value}");
            if (field.HasValue)
                queryParams.Add($"field={field.Value}");
            if (onFoot.HasValue)
                queryParams.Add($"onFoot={onFoot.Value}");
            if (horseback.HasValue)
                queryParams.Add($"horseback={horseback.Value}");
            if (open.HasValue)
                queryParams.Add($"open={open.Value}");
            if (nursery.HasValue)
                queryParams.Add($"nursery={nursery.Value}");
            if (intermediate.HasValue)
                queryParams.Add($"intermediate={intermediate.Value}");
            if (novice.HasValue)
                queryParams.Add($"novice={novice.Value}");
            if (junior.HasValue)
                queryParams.Add($"junior={junior.Value}");

            var url = $"{ApiUrl}/search?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<MSSA_Event>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        // Trials
        public async Task<List<MSSA_Trial>> GetEventTrialsAsync(int eventId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_Trial");
            return await GetJsonAsync<List<MSSA_Trial>>(
                CreateAuthorizationPolicyUrl($"{url}/event/{eventId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Trial> GetTrialAsync(int trialId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_Trial");
            return await GetJsonAsync<MSSA_Trial>(
                CreateAuthorizationPolicyUrl($"{url}/{trialId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Trial> AddTrialAsync(MSSA_Trial trial, int moduleId)
        {
            var url = CreateApiUrl("MSSA_Trial");
            return await PostJsonAsync<MSSA_Trial>(
                CreateAuthorizationPolicyUrl($"{url}?moduleid={moduleId}", EntityNames.Module, moduleId), trial);
        }

        public async Task<MSSA_Trial> UpdateTrialAsync(MSSA_Trial trial, int moduleId)
        {
            var url = CreateApiUrl("MSSA_Trial");
            return await PutJsonAsync<MSSA_Trial>(
                CreateAuthorizationPolicyUrl($"{url}/{trial.TrialId}?moduleid={moduleId}", EntityNames.Module, moduleId), trial);
        }

        public async Task DeleteTrialAsync(int trialId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_Trial");
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{url}/{trialId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }
    }
}

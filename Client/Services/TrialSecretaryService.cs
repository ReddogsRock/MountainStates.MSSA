using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.TrialSecretary.Services
{
    public class TrialSecretaryService : ServiceBase, ITrialSecretaryService, IService
    {
        public TrialSecretaryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("TrialSecretary");

        public async Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId)
        {
            var events = await GetJsonAsync<List<RecentEventDto>>($"{ApiUrl}/recentevents?moduleid={moduleId}");
            return events ?? new List<RecentEventDto>();
        }

        public async Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId)
        {
            var url = $"{ApiUrl}/handlers/search?moduleid={moduleId}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                url += $"&searchTerm={System.Uri.EscapeDataString(searchTerm)}";

            var handlers = await GetJsonAsync<List<HandlerSearchDto>>(url);
            return handlers ?? new List<HandlerSearchDto>();
        }

        public async Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId, int moduleId)
        {
            return await GetJsonAsync<HandlerSearchDto>($"{ApiUrl}/handlers/{handlerId}?moduleid={moduleId}");
        }

        public async Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto handlerDto, int moduleId)
        {
            return await PostJsonAsync<CreateHandlerDto, MSSA_Handler>($"{ApiUrl}/handlers?moduleid={moduleId}", handlerDto);
        }

        public async Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm, int moduleId)
        {
            var url = $"{ApiUrl}/dogs/search?moduleid={moduleId}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                url += $"&searchTerm={System.Uri.EscapeDataString(searchTerm)}";

            var dogs = await GetJsonAsync<List<DogSearchDto>>(url);
            return dogs ?? new List<DogSearchDto>();
        }

        public async Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId)
        {
            return await GetJsonAsync<DogSearchDto>($"{ApiUrl}/dogs/{dogId}?moduleid={moduleId}");
        }

        public async Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto, int moduleId)
        {
            return await PostJsonAsync<CreateDogDto, MSSA_Dog>($"{ApiUrl}/dogs?moduleid={moduleId}", dogDto);
        }

        public async Task<List<int>> CreateEntriesAsync(CreateEntriesDto entriesDto, int moduleId)
        {
            var entryIds = await PostJsonAsync<CreateEntriesDto, List<int>>($"{ApiUrl}/entries?moduleid={moduleId}", entriesDto);
            return entryIds ?? new List<int>();
        }

        public async Task<EventEntriesSummaryDto> GetEventEntriesAsync(int eventId, int moduleId)
        {
            return await GetJsonAsync<EventEntriesSummaryDto>($"{ApiUrl}/events/{eventId}/entries?moduleid={moduleId}");
        }
    }
}

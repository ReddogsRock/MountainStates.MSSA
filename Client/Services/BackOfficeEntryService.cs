using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.TrialSecretary.Models;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Services
{
    public class BackOfficeEntryService : ServiceBase, IBackOfficeEntryService, IService
    {
        public BackOfficeEntryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("BackOfficeEntry");

        public async Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId)
        {
            var result = await GetJsonAsync<List<RecentEventDto>>($"{ApiUrl}/recentevents?moduleid={moduleId}");
            return result ?? new List<RecentEventDto>();
        }

        public async Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId)
        {
            var url = $"{ApiUrl}/handlers/search?moduleid={moduleId}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                url += $"&searchTerm={System.Uri.EscapeDataString(searchTerm)}";
            var result = await GetJsonAsync<List<HandlerSearchDto>>(url);
            return result ?? new List<HandlerSearchDto>();
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
            var result = await GetJsonAsync<List<DogSearchDto>>(url);
            return result ?? new List<DogSearchDto>();
        }

        public async Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId)
        {
            return await GetJsonAsync<DogSearchDto>($"{ApiUrl}/dogs/{dogId}?moduleid={moduleId}");
        }

        public async Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto, int moduleId)
        {
            return await PostJsonAsync<CreateDogDto, MSSA_Dog>($"{ApiUrl}/dogs?moduleid={moduleId}", dogDto);
        }

        public async Task<int> SaveResultEntryAsync(SaveResultEntryDto dto, int moduleId)
        {
            if (dto.EntryId == 0)
            {
                // Create new entry
                var result = await PostJsonAsync<SaveResultEntryDto, int>($"{ApiUrl}/entries?moduleid={moduleId}", dto);
                return result;
            }
            else
            {
                // Update existing entry — POST to a dedicated update endpoint
                var result = await PostJsonAsync<SaveResultEntryDto, int>($"{ApiUrl}/entries/update?moduleid={moduleId}", dto);
                return result;
            }
        }

        public async Task<SaveResultEntryDto> GetResultEntryAsync(int entryId, int moduleId)
        {
            return await GetJsonAsync<SaveResultEntryDto>($"{ApiUrl}/entries/{entryId}?moduleid={moduleId}");
        }

        public async Task<List<ResultEntryListItem>> GetTrialClassEntriesAsync(int trialId, int classId, int moduleId)
        {
            var result = await GetJsonAsync<List<ResultEntryListItem>>(
                $"{ApiUrl}/trials/{trialId}/classes/{classId}/entries?moduleid={moduleId}");
            return result ?? new List<ResultEntryListItem>();
        }

        public async Task<FinalizeResultDto> FinalizeTrialClassAsync(int trialId, int classId, int moduleId)
        {
            return await PostJsonAsync<object, FinalizeResultDto>(
                $"{ApiUrl}/trials/{trialId}/classes/{classId}/finalize?moduleid={moduleId}", new { });
        }
    }
}

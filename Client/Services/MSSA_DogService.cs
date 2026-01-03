using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Services
{
    public class MSSA_DogService : ServiceBase, IMSSA_DogService, IService
    {
        public MSSA_DogService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_Dog");

        public async Task<List<MSSA_Dog>> GetDogsAsync(int moduleId)
        {
            var dogs = await GetJsonAsync<List<MSSA_Dog>>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
            return dogs?.OrderBy(d => d.Name).ToList();
        }

        public async Task<MSSA_Dog> GetDogAsync(int dogId, int moduleId)
        {
            return await GetJsonAsync<MSSA_Dog>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{dogId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Dog> AddDogAsync(MSSA_Dog dog, int moduleId)
        {
            return await PostJsonAsync<MSSA_Dog>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId), dog);
        }

        public async Task<MSSA_Dog> UpdateDogAsync(MSSA_Dog dog, int moduleId)
        {
            return await PutJsonAsync<MSSA_Dog>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{dog.DogId}?moduleid={moduleId}", EntityNames.Module, moduleId), dog);
        }

        public async Task DeleteDogAsync(int dogId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{dogId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_Dog>> SearchDogsAsync(
            string searchTerm,
            string breed,
            bool? ownerIsMember,
            bool? includeInactive,
            int moduleId)
        {
            var queryParams = new List<string> { $"moduleid={moduleId}" };

            if (!string.IsNullOrWhiteSpace(searchTerm))
                queryParams.Add($"searchTerm={System.Uri.EscapeDataString(searchTerm)}");
            if (!string.IsNullOrWhiteSpace(breed))
                queryParams.Add($"breed={System.Uri.EscapeDataString(breed)}");
            if (ownerIsMember.HasValue)
                queryParams.Add($"ownerIsMember={ownerIsMember.Value}");
            if (includeInactive.HasValue)
                queryParams.Add($"includeInactive={includeInactive.Value}");

            var url = $"{ApiUrl}/search?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<MSSA_Dog>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        // Futurity
        public async Task<List<MSSA_DogFuturityParticipation>> GetDogFuturityParticipationAsync(int dogId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_DogFuturity");
            return await GetJsonAsync<List<MSSA_DogFuturityParticipation>>(
                CreateAuthorizationPolicyUrl($"{url}/dog/{dogId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_DogFuturityParticipation> AddFuturityParticipationAsync(MSSA_DogFuturityParticipation participation, int moduleId)
        {
            var url = CreateApiUrl("MSSA_DogFuturity");
            return await PostJsonAsync<MSSA_DogFuturityParticipation>(
                CreateAuthorizationPolicyUrl($"{url}?moduleid={moduleId}", EntityNames.Module, moduleId), participation);
        }

        public async Task DeleteFuturityParticipationAsync(int participationId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_DogFuturity");
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{url}/{participationId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        // Entries
        public async Task<List<MSSA_DogEntry>> GetDogEntriesAsync(int dogId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_DogEntry");
            return await GetJsonAsync<List<MSSA_DogEntry>>(
                CreateAuthorizationPolicyUrl($"{url}/dog/{dogId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }
    }
}

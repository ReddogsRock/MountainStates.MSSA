using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Models;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Services
{
    public class MSSA_YearEndStandingsService : ServiceBase, IMSSA_YearEndStandingsService, IService
    {
        public MSSA_YearEndStandingsService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_YearEndStandings");

        public async Task<List<MSSA_YearEndStanding>> GetStandingsAsync(int? year, string level, string species, int moduleId)
        {
            var queryParams = new List<string> { $"level={System.Uri.EscapeDataString(level)}", $"species={species}", $"moduleid={moduleId}" };
            if (year.HasValue)
            {
                queryParams.Add($"year={year.Value}");
            }
            // year omitted entirely => Lifetime

            var url = $"{ApiUrl}?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<MSSA_YearEndStanding>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<List<YearEndStandingDetail>> GetStandingDetailsAsync(int dogId, int? handlerId, int? year, string level, string species, int moduleId)
        {
            var queryParams = new List<string> { $"dogId={dogId}", $"level={System.Uri.EscapeDataString(level)}", $"species={species}", $"moduleid={moduleId}" };
            if (handlerId.HasValue)
            {
                queryParams.Add($"handlerId={handlerId.Value}");
            }
            if (year.HasValue)
            {
                queryParams.Add($"year={year.Value}");
            }

            var url = $"{ApiUrl}/details?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<YearEndStandingDetail>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<List<int>> GetAvailableYearsAsync(int moduleId)
        {
            var url = $"{ApiUrl}/years?moduleid={moduleId}";
            return await GetJsonAsync<List<int>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<List<string>> GetLevelsAsync(int moduleId)
        {
            var url = $"{ApiUrl}/levels?moduleid={moduleId}";
            return await GetJsonAsync<List<string>>(CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }
    }
}

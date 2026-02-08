using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Finals.Services
{
    public class MSSA_FinalsService : ServiceBase, IMSSA_FinalsService, IService
    {
        public MSSA_FinalsService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_Finals");

        public async Task<List<MSSA_FinalsResult>> GetFinalsResultsAsync(int moduleId)
        {
            return await GetJsonAsync<List<MSSA_FinalsResult>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_FinalsResult>> SearchFinalsResultsAsync(
            int? year,
            string level,
            string stock,
            string round,
            int? place,
            int moduleId)
        {
            var queryParams = new List<string> { $"moduleid={moduleId}" };

            if (year.HasValue)
                queryParams.Add($"year={year.Value}");
            if (!string.IsNullOrWhiteSpace(level))
                queryParams.Add($"level={System.Uri.EscapeDataString(level)}");
            if (!string.IsNullOrWhiteSpace(stock))
                queryParams.Add($"stock={System.Uri.EscapeDataString(stock)}");
            if (!string.IsNullOrWhiteSpace(round))
                queryParams.Add($"round={System.Uri.EscapeDataString(round)}");
            if (place.HasValue)
                queryParams.Add($"place={place.Value}");

            var url = $"{ApiUrl}/search?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<MSSA_FinalsResult>>(
                CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<List<int>> GetAvailableYearsAsync(int moduleId)
        {
            return await GetJsonAsync<List<int>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/years?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<string>> GetAvailableLevelsAsync(int moduleId)
        {
            return await GetJsonAsync<List<string>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/levels?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<string>> GetAvailableStocksAsync(int moduleId)
        {
            return await GetJsonAsync<List<string>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/stocks?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<string>> GetAvailableRoundsAsync(int? year, string level, string stock, int moduleId)
        {
            var queryParams = new List<string> { $"moduleid={moduleId}" };

            if (year.HasValue)
                queryParams.Add($"year={year.Value}");
            if (!string.IsNullOrWhiteSpace(level))
                queryParams.Add($"level={System.Uri.EscapeDataString(level)}");
            if (!string.IsNullOrWhiteSpace(stock))
                queryParams.Add($"stock={System.Uri.EscapeDataString(stock)}");

            var url = $"{ApiUrl}/rounds?{string.Join("&", queryParams)}";
            return await GetJsonAsync<List<string>>(
                CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_UltimateBreakdown>> GetUltimateBreakdownAsync(int year, string level, int handlerId, int dogId, int moduleId)
        {
            var url = $"{ApiUrl}/ultimate/breakdown?year={year}&level={System.Uri.EscapeDataString(level)}&handlerId={handlerId}&dogId={dogId}&moduleid={moduleId}";
            return await GetJsonAsync<List<MSSA_UltimateBreakdown>>(
                CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId));
        }
    }
}

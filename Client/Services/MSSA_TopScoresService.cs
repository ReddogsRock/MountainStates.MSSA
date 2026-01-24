using MountainStates.MSSA.Module.MSSA_TopScores.Models;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_TopScores.Services
{
    public class MSSA_TopScoresService : ServiceBase, IMSSA_TopScoresService, IService
    {
        public MSSA_TopScoresService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_TopScores");

        public async Task<IEnumerable<TopScoreResult>> GetTopScoresAsync(int moduleId, TopScoreParameters parameters)
        {
            return await PostJsonAsync<TopScoreParameters, IEnumerable<TopScoreResult>>($"{ApiUrl}?entityid={moduleId}", parameters);
        }

        public async Task<IEnumerable<int>> GetAvailableYearsAsync(int moduleId)
        {
            return await GetJsonAsync<IEnumerable<int>>($"{ApiUrl}/years?entityid={moduleId}");
        }
    }
}
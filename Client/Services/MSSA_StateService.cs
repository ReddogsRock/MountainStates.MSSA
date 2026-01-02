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
    public class MSSA_StateService : ServiceBase, IMSSA_StateService, IService
    {
        public MSSA_StateService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_State");

        public async Task<List<MSSA_State>> GetStatesAsync(int moduleId)
        {
            return await GetJsonAsync<List<MSSA_State>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<MSSA_State>> GetUSStatesAsync(int moduleId)
        {
            var states = await GetStatesAsync(moduleId);
            return states?.Where(s => s.Country == "US").ToList();
        }

        public async Task<List<MSSA_State>> GetCanadianProvincesAsync(int moduleId)
        {
            var states = await GetStatesAsync(moduleId);
            return states?.Where(s => s.Country == "CA").ToList();
        }
    }
}

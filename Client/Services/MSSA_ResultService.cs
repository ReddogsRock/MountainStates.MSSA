using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Results.Models;

namespace MountainStates.MSSA.Module.MSSA_Results.Services
{
    public class MSSA_ResultService : ServiceBase, IMSSA_ResultService, IService
    {
        public MSSA_ResultService(System.Net.Http.HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_Result");

        public async Task<List<EventScoringSummary>> GetScoringEventsAsync(int moduleId)
        {
            return await GetJsonAsync<List<EventScoringSummary>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/events?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<EventScoringSummary>> GetPendingApprovalEventsAsync(int moduleId)
        {
            return await GetJsonAsync<List<EventScoringSummary>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/pending?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<ResultRunRow>> GetTrialRunRowsAsync(int trialId, int moduleId)
        {
            return await GetJsonAsync<List<ResultRunRow>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}/rows?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task SaveResultRowAsync(int trialId, SaveResultRowDto dto, int moduleId)
        {
            await PostJsonAsync<SaveResultRowDto, bool>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}/rows/save?moduleid={moduleId}", EntityNames.Module, moduleId), dto);
        }

        public async Task CalculatePlacingAndPointsAsync(int trialId, int moduleId)
        {
            await PostJsonAsync<object, bool>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}/calculate?moduleid={moduleId}", EntityNames.Module, moduleId), null);
        }

        public async Task<SubmitEventResultsDto> SubmitEventForApprovalAsync(int eventId, int moduleId)
        {
            return await PostJsonAsync<object, SubmitEventResultsDto>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/events/{eventId}/submit?moduleid={moduleId}", EntityNames.Module, moduleId), null);
        }

        public async Task<SubmitEventResultsDto> ApproveEventAsync(int eventId, int moduleId)
        {
            return await PostJsonAsync<object, SubmitEventResultsDto>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/events/{eventId}/approve?moduleid={moduleId}", EntityNames.Module, moduleId), null);
        }
    }
}

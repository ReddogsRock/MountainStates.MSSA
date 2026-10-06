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

        public async Task<int> GetScoredRunCountAsync(int eventId, int moduleId)
        {
            return await GetJsonAsync<int>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/event/{eventId}/scoredruncount?moduleid={moduleId}", EntityNames.Module, moduleId));
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

        public async Task CalculatePlacingAndPointsAsync(int trialId, int moduleId, int? classId = null)
        {
            var url = $"{ApiUrl}/trial/{trialId}/calculate?moduleid={moduleId}";
            if (classId.HasValue)
            {
                url += $"&classid={classId.Value}";
            }

            await PostJsonAsync<object, bool>(
                CreateAuthorizationPolicyUrl(url, EntityNames.Module, moduleId), null);
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

        public async Task<ScoreSheetImportResult> ImportScoreSheetAsync(int trialId, ImportScoreSheetDto dto, int moduleId)
        {
            return await PostJsonAsync<ImportScoreSheetDto, ScoreSheetImportResult>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}/scoresheet/import?moduleid={moduleId}", EntityNames.Module, moduleId), dto);
        }

        public async Task<ImportCompleteTrialResult> ImportCompleteTrialAsync(int trialId, ImportScoreSheetDto dto, int moduleId)
        {
            return await PostJsonAsync<ImportScoreSheetDto, ImportCompleteTrialResult>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}/completetrial/import?moduleid={moduleId}", EntityNames.Module, moduleId), dto);
        }

        // Fetched as bytes through the authenticated HttpClient rather than linked to
        // directly - a plain <a href> download is a bare browser navigation that relies
        // on cookies alone, which doesn't carry whatever this app's own role/ownership
        // check needs for a non-Admin user (a Trial Secretary could edit scores and
        // calculate placing on a trial - both going through this same HttpClient - but
        // got Forbidden on the scoring sheet link specifically). The caller triggers the
        // actual file save client-side (see mssaResultsDownloadFile).
        public async Task<byte[]> GetScoreSheetAsync(int trialId, int moduleId)
        {
            return await GetByteArrayAsync(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}/scoresheet?moduleid={moduleId}", EntityNames.Module, moduleId));
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Results.Models;

namespace MountainStates.MSSA.Module.MSSA_Results.Manager
{
    public interface IMSSA_ResultManager
    {
        Task<List<EventScoringSummary>> GetScoringEventsAsync(int? ownerUserId, int? scorekeeperUserId, int moduleId);
        Task<List<EventScoringSummary>> GetPendingApprovalEventsAsync(int moduleId);

        Task<int?> GetEventOwnerForTrialAsync(int trialId, int moduleId);
        Task<int?> GetEventOwnerAsync(int eventId, int moduleId);
        Task<int?> GetTrialScorekeeperUserIdAsync(int trialId, int moduleId);

        Task<List<ResultRunRow>> GetTrialRunRowsAsync(int trialId, int moduleId);
        Task SaveResultRowAsync(SaveResultRowDto dto, int moduleId, int userId);
        Task CalculatePlacingAndPointsAsync(int trialId, int moduleId, int userId);

        Task<SubmitEventResultsDto> SubmitEventForApprovalAsync(int eventId, int moduleId, int userId);
        Task<SubmitEventResultsDto> ApproveEventAsync(int eventId, int moduleId, int userId);

        Task<byte[]> GenerateScoreSheetAsync(int trialId, int moduleId);
        Task<ScoreSheetImportResult> ImportScoreSheetAsync(int trialId, byte[] fileBytes, int moduleId, int userId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Results.Models;

namespace MountainStates.MSSA.Module.MSSA_Results.Services
{
    public interface IMSSA_ResultService
    {
        Task<List<EventScoringSummary>> GetScoringEventsAsync(int moduleId);
        Task<List<EventScoringSummary>> GetPendingApprovalEventsAsync(int moduleId);

        Task<List<ResultRunRow>> GetTrialRunRowsAsync(int trialId, int moduleId);
        Task SaveResultRowAsync(int trialId, SaveResultRowDto dto, int moduleId);
        Task CalculatePlacingAndPointsAsync(int trialId, int moduleId);

        Task<SubmitEventResultsDto> SubmitEventForApprovalAsync(int eventId, int moduleId);
        Task<SubmitEventResultsDto> ApproveEventAsync(int eventId, int moduleId);
    }
}

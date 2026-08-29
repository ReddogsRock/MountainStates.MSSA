using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Results.Models;
using MountainStates.MSSA.Module.MSSA_Results.Repository;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Results.Manager
{
    public class MSSA_ResultManager : IMSSA_ResultManager, ITransientService
    {
        private readonly IMSSA_ResultRepository _repository;

        public MSSA_ResultManager(IMSSA_ResultRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EventScoringSummary>> GetScoringEventsAsync(int? ownerUserId, int moduleId)
        {
            return await _repository.GetScoringEventsAsync(ownerUserId);
        }

        public async Task<List<EventScoringSummary>> GetPendingApprovalEventsAsync(int moduleId)
        {
            return await _repository.GetPendingApprovalEventsAsync();
        }

        public async Task<int?> GetEventOwnerForTrialAsync(int trialId, int moduleId)
        {
            return await _repository.GetEventOwnerForTrialAsync(trialId);
        }

        public async Task<int?> GetEventOwnerAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventOwnerAsync(eventId);
        }

        public async Task<List<ResultRunRow>> GetTrialRunRowsAsync(int trialId, int moduleId)
        {
            return await _repository.GetTrialRunRowsAsync(trialId);
        }

        public async Task SaveResultRowAsync(SaveResultRowDto dto, int moduleId, int userId)
        {
            await _repository.SaveResultRowAsync(dto, userId);
        }

        public async Task CalculatePlacingAndPointsAsync(int trialId, int moduleId, int userId)
        {
            await _repository.CalculatePlacingAndPointsAsync(trialId, userId);
        }

        public async Task<SubmitEventResultsDto> SubmitEventForApprovalAsync(int eventId, int moduleId, int userId)
        {
            return await _repository.SubmitEventForApprovalAsync(eventId, userId);
        }

        public async Task<SubmitEventResultsDto> ApproveEventAsync(int eventId, int moduleId, int userId)
        {
            return await _repository.ApproveEventAsync(eventId, userId);
        }
    }
}

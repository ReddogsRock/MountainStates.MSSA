using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Results.Models;

namespace MountainStates.MSSA.Module.MSSA_Results.Repository
{
    public interface IMSSA_ResultRepository
    {
        // Event dropdown / admin queue. ownerUserId is null for Admin/Scorekeeper (all
        // events); otherwise scoped to events that user created.
        Task<List<EventScoringSummary>> GetScoringEventsAsync(int? ownerUserId);
        Task<List<EventScoringSummary>> GetPendingApprovalEventsAsync();

        // Resolves the owner of the Event a Trial belongs to, for authorizing access to
        // the Results grid the same way MSSA_EntryRepository does for entries.
        Task<int?> GetEventOwnerForTrialAsync(int trialId);
        Task<int?> GetEventOwnerAsync(int eventId);

        Task<List<ResultRunRow>> GetTrialRunRowsAsync(int trialId);
        Task SaveResultRowAsync(SaveResultRowDto dto, int userId);

        // Computes Placing and TrialPoints for every class present in the trial. Ties
        // (identical score, run time, and tie-breaker time) share the average of the
        // placements they occupy, split only among the tied members - a tied non-member
        // still scores 0.
        Task CalculatePlacingAndPointsAsync(int trialId, int userId);

        Task<SubmitEventResultsDto> SubmitEventForApprovalAsync(int eventId, int userId);
        Task<SubmitEventResultsDto> ApproveEventAsync(int eventId, int userId);
    }
}

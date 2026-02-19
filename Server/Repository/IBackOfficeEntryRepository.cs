using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.TrialSecretary.Models;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Repository
{
    public interface IBackOfficeEntryRepository
    {
        // Events
        Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync();

        // Handlers
        Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm);
        Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId);
        Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto dto);

        // Dogs
        Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm);
        Task<DogSearchDto> GetDogByIdAsync(int dogId);
        Task<MSSA_Dog> CreateDogAsync(CreateDogDto dto);

        // Result entries
        Task<int> CreateResultEntryAsync(SaveResultEntryDto dto, int userId);
        Task UpdateResultEntryAsync(SaveResultEntryDto dto, int userId);
        Task<SaveResultEntryDto> GetResultEntryAsync(int entryId);
        Task<List<ResultEntryListItem>> GetTrialClassEntriesAsync(int trialId, int classId);

        // Finalize
        Task<FinalizeResultDto> FinalizeTrialClassAsync(int trialId, int classId, int userId);
    }
}

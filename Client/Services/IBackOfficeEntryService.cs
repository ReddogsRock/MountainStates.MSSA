using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.TrialSecretary.Models;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Services
{
    public interface IBackOfficeEntryService
    {
        // Events / Trials / Classes
        Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId);

        // Handler operations (shared with TrialSecretary pattern)
        Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId);
        Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId, int moduleId);
        Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto handlerDto, int moduleId);

        // Dog operations
        Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm, int moduleId);
        Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId);
        Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto, int moduleId);

        // Result entry
        Task<int> SaveResultEntryAsync(SaveResultEntryDto dto, int moduleId);
        Task<SaveResultEntryDto> GetResultEntryAsync(int entryId, int moduleId);
        Task<List<ResultEntryListItem>> GetTrialClassEntriesAsync(int trialId, int classId, int moduleId);

        // Finalize
        Task<FinalizeResultDto> FinalizeTrialClassAsync(int trialId, int classId, int moduleId);
    }
}

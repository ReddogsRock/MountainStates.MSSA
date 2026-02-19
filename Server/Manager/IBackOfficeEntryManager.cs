using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.TrialSecretary.Models;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Manager
{
    public interface IBackOfficeEntryManager
    {
        Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId);

        Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId);
        Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId, int moduleId);
        Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto dto, int moduleId);

        Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm, int moduleId);
        Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId);
        Task<MSSA_Dog> CreateDogAsync(CreateDogDto dto, int moduleId);

        Task<int> SaveResultEntryAsync(SaveResultEntryDto dto, int moduleId, int userId);
        Task<SaveResultEntryDto> GetResultEntryAsync(int entryId, int moduleId);
        Task<List<ResultEntryListItem>> GetTrialClassEntriesAsync(int trialId, int classId, int moduleId);
        Task<FinalizeResultDto> FinalizeTrialClassAsync(int trialId, int classId, int moduleId, int userId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.TrialSecretary.Manager
{
    public interface ITrialSecretaryManager
    {
        Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId);
        Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId);
        Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId, int moduleId);
        Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto handlerDto, int moduleId);
        Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm, int moduleId);
        Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId);
        Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto, int moduleId);
        Task<List<int>> CreateEntriesAsync(CreateEntriesDto entriesDto, int moduleId, int userId);
        Task<EventEntriesSummaryDto> GetEventEntriesAsync(int eventId, int moduleId);
    }
}

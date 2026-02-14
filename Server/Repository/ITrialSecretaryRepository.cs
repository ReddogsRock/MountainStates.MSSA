using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.TrialSecretary.Repository
{
    public interface ITrialSecretaryRepository
    {
        // Get recent events (within last 30 days) with trials
        Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync();
        
        // Handler operations
        Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm);
        Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId);
        Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto handlerDto);
        
        // Dog operations
        Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm);
        Task<DogSearchDto> GetDogByIdAsync(int dogId);
        Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto);
        
        // Entry creation
        Task<List<int>> CreateEntriesAsync(CreateEntriesDto entriesDto, int userId);
        
        // Get entries for an event
        Task<EventEntriesSummaryDto> GetEventEntriesAsync(int eventId);
    }
}

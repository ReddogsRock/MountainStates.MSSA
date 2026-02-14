using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.TrialSecretary.Repository;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.TrialSecretary.Manager
{
    public class TrialSecretaryManager : ITrialSecretaryManager, ITransientService
    {
        private readonly ITrialSecretaryRepository _repository;

        public TrialSecretaryManager(ITrialSecretaryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId)
        {
            return await _repository.GetRecentEventsWithTrialsAsync();
        }

        public async Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId)
        {
            return await _repository.SearchHandlersAsync(searchTerm);
        }

        public async Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId, int moduleId)
        {
            return await _repository.GetHandlerByIdAsync(handlerId);
        }

        public async Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto handlerDto, int moduleId)
        {
            return await _repository.CreateHandlerAsync(handlerDto);
        }

        public async Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm, int moduleId)
        {
            return await _repository.SearchDogsAsync(searchTerm);
        }

        public async Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId)
        {
            return await _repository.GetDogByIdAsync(dogId);
        }

        public async Task<MSSA_Dog> CreateDogAsync(CreateDogDto dogDto, int moduleId)
        {
            return await _repository.CreateDogAsync(dogDto);
        }

        public async Task<List<int>> CreateEntriesAsync(CreateEntriesDto entriesDto, int moduleId, int userId)
        {
            return await _repository.CreateEntriesAsync(entriesDto, userId);
        }

        public async Task<EventEntriesSummaryDto> GetEventEntriesAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventEntriesAsync(eventId);
        }
    }
}

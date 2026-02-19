using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.BackOfficeEntry.Repository;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using Oqtane.Modules;
using MountainStates.MSSA.Module.TrialSecretary.Models;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Manager
{
    public class BackOfficeEntryManager : IBackOfficeEntryManager, ITransientService
    {
        private readonly IBackOfficeEntryRepository _repository;

        public BackOfficeEntryManager(IBackOfficeEntryRepository repository)
        {
            _repository = repository;
        }

        public Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync(int moduleId)
            => _repository.GetRecentEventsWithTrialsAsync();

        public Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm, int moduleId)
            => _repository.SearchHandlersAsync(searchTerm);

        public Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId, int moduleId)
            => _repository.GetHandlerByIdAsync(handlerId);

        public Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto dto, int moduleId)
            => _repository.CreateHandlerAsync(dto);

        public Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm, int moduleId)
            => _repository.SearchDogsAsync(searchTerm);

        public Task<DogSearchDto> GetDogByIdAsync(int dogId, int moduleId)
            => _repository.GetDogByIdAsync(dogId);

        public Task<MSSA_Dog> CreateDogAsync(CreateDogDto dto, int moduleId)
            => _repository.CreateDogAsync(dto);

        public async Task<int> SaveResultEntryAsync(SaveResultEntryDto dto, int moduleId, int userId)
        {
            if (dto.EntryId == 0)
                return await _repository.CreateResultEntryAsync(dto, userId);

            await _repository.UpdateResultEntryAsync(dto, userId);
            return dto.EntryId;
        }

        public Task<SaveResultEntryDto> GetResultEntryAsync(int entryId, int moduleId)
            => _repository.GetResultEntryAsync(entryId);

        public Task<List<ResultEntryListItem>> GetTrialClassEntriesAsync(int trialId, int classId, int moduleId)
            => _repository.GetTrialClassEntriesAsync(trialId, classId);

        public Task<FinalizeResultDto> FinalizeTrialClassAsync(int trialId, int classId, int moduleId, int userId)
            => _repository.FinalizeTrialClassAsync(trialId, classId, userId);
    }
}

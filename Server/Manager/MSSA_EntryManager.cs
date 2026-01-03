using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Entries.Repository;
using MountainStates.MSSA.Module.MSSA_Entries.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Manager
{
    public class MSSA_EntryManager : IMSSA_EntryManager, ITransientService
    {
        private readonly IMSSA_EntryRepository _repository;

        public MSSA_EntryManager(IMSSA_EntryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MSSA_Entry>> GetEntriesAsync(int moduleId)
        {
            return await _repository.GetEntriesAsync(moduleId);
        }

        public async Task<IEnumerable<EntryListItem>> GetTrialEntriesAsync(int trialId, int moduleId)
        {
            return await _repository.GetTrialEntriesAsync(trialId);
        }

        public async Task<MSSA_Entry> GetEntryAsync(int entryId, int moduleId)
        {
            return await _repository.GetEntryAsync(entryId);
        }

        public async Task<MSSA_Entry> AddEntryAsync(MSSA_Entry entry, int moduleId)
        {
            return await _repository.AddEntryAsync(entry);
        }

        public async Task<MSSA_Entry> UpdateEntryAsync(MSSA_Entry entry, int moduleId)
        {
            return await _repository.UpdateEntryAsync(entry);
        }

        public async Task DeleteEntryAsync(int entryId, int moduleId)
        {
            await _repository.DeleteEntryAsync(entryId);
        }

        public async Task GenerateRunOrderAsync(int trialId, int classId, int moduleId)
        {
            await _repository.GenerateRunOrderAsync(trialId, classId);
        }

        public async Task<IEnumerable<MSSA_Class>> GetClassesAsync(int moduleId)
        {
            return await _repository.GetClassesAsync();
        }

        public async Task<MSSA_Class> GetClassAsync(int classId, int moduleId)
        {
            return await _repository.GetClassAsync(classId);
        }
    }
}

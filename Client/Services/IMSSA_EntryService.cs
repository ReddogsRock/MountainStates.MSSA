using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Services
{
    public interface IMSSA_EntryService
    {
        Task<List<MSSA_Entry>> GetEntriesAsync(int moduleId);
        Task<List<EntryListItem>> GetTrialEntriesAsync(int trialId, int moduleId);
        Task<MSSA_Entry> GetEntryAsync(int entryId, int moduleId);
        Task<MSSA_Entry> AddEntryAsync(MSSA_Entry entry, int moduleId);
        Task<MSSA_Entry> UpdateEntryAsync(MSSA_Entry entry, int moduleId);
        Task DeleteEntryAsync(int entryId, int moduleId);

        Task GenerateRunOrderAsync(int trialId, int classId, int moduleId);

        Task<List<MSSA_Class>> GetClassesAsync(int moduleId);
        Task<MSSA_Class> GetClassAsync(int classId, int moduleId);
    }
}

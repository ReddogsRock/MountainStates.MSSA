using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Repository
{
    public interface IMSSA_EntryRepository
    {
        // Entries
        Task<IEnumerable<MSSA_Entry>> GetEntriesAsync(int moduleId);
        Task<IEnumerable<EntryListItem>> GetTrialEntriesAsync(int trialId);
        Task<MSSA_Entry> GetEntryAsync(int entryId);
        Task<MSSA_Entry> AddEntryAsync(MSSA_Entry entry);
        Task<MSSA_Entry> UpdateEntryAsync(MSSA_Entry entry);
        Task DeleteEntryAsync(int entryId);

        // Run order
        Task GenerateRunOrderAsync(int trialId, int classId);

        // Classes
        Task<IEnumerable<MSSA_Class>> GetClassesAsync();
        Task<MSSA_Class> GetClassAsync(int classId);
    }
}

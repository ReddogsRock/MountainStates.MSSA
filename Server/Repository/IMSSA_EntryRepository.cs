using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Repository
{
    public interface IMSSA_EntryRepository
    {
        Task<IEnumerable<MSSA_Entry>> GetEntriesAsync(int moduleId);
        Task<IEnumerable<EntryListItem>> GetTrialEntriesAsync(int trialId);
        Task<MSSA_Entry> GetEntryAsync(int entryId);
        Task<MSSA_Entry> AddEntryAsync(MSSA_Entry entry);
        Task<MSSA_Entry> UpdateEntryAsync(MSSA_Entry entry);
        Task DeleteEntryAsync(int entryId);

        // Run order: propose (not persisted, all classes at once in fixed order) ->
        // review/edit client-side -> save (persists, returns the saved list back).
        Task<List<RunOrderEntry>> GetProposedRunOrderAsync(int trialId);
        Task<List<RunOrderEntry>> SaveRunOrderAsync(List<RunOrderEntry> assignments);

        // Scores import - matches rows to entries by EntryId. Returns only the rows
        // that were actually matched/updated; compare its count against what was sent
        // to see if any rows were skipped (e.g. stale/unknown EntryId).
        Task<List<ScoreImportRow>> ImportScoresAsync(List<ScoreImportRow> rows);

        // Classes
        Task<IEnumerable<MSSA_Class>> GetClassesAsync();
        Task<MSSA_Class> GetClassAsync(int classId);
    }
}

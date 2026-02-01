using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Repository
{
    public interface IMSSA_EventRepository
    {
        // Events
        Task<IEnumerable<MSSA_Event>> GetEventsAsync(int moduleId);
        Task<MSSA_Event> GetEventAsync(int eventId);
        Task<MSSA_Event> AddEventAsync(MSSA_Event evt);
        Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt);
        Task DeleteEventAsync(int eventId);

        // Search and filter
        Task<IEnumerable<MSSA_Event>> SearchEventsAsync(
            string searchTerm = null,
            string stateCode = null,
            int? year = null,
            bool? cattle = null,
            bool? sheep = null,
            bool? arena = null,
            bool? field = null,
            bool? onFoot = null,
            bool? horseback = null,
            bool? open = null,
            bool? nursery = null,
            bool? intermediate = null,
            bool? novice = null,
            bool? junior = null);

        // Trials
        Task<IEnumerable<MSSA_Trial>> GetEventTrialsAsync(int eventId);
        Task<MSSA_Trial> GetTrialAsync(int trialId);
        Task<MSSA_Trial> AddTrialAsync(MSSA_Trial trial);
        Task<MSSA_Trial> UpdateTrialAsync(MSSA_Trial trial);
        Task DeleteTrialAsync(int trialId);
        
        // Entries
        Task<List<EntryListItem>> GetTrialEntriesAsync(int trialId);
    }
}

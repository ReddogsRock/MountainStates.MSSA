using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Manager
{
    public interface IMSSA_EventManager
    {
        // Events
        Task<IEnumerable<MSSA_Event>> GetEventsAsync(int moduleId);
        Task<MSSA_Event> GetEventAsync(int eventId, int moduleId);
        Task<MSSA_Event> AddEventAsync(MSSA_Event evt, int moduleId);
        Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt, int moduleId);
        Task DeleteEventAsync(int eventId, int moduleId);

        Task<IEnumerable<MSSA_Event>> SearchEventsAsync(
            string searchTerm,
            string stateCode,
            int? year,
            bool? cattle,
            bool? sheep,
            bool? arena,
            bool? field,
            bool? onFoot,
            bool? horseback,
            bool? open,
            bool? nursery,
            bool? intermediate,
            bool? novice,
            bool? junior,
            int moduleId);

        // Trials
        Task<IEnumerable<MSSA_Trial>> GetEventTrialsAsync(int eventId, int moduleId);
        Task<MSSA_Trial> GetTrialAsync(int trialId, int moduleId);
        Task<MSSA_Trial> AddTrialAsync(MSSA_Trial trial, int moduleId);
        Task<MSSA_Trial> UpdateTrialAsync(MSSA_Trial trial, int moduleId);
        Task DeleteTrialAsync(int trialId, int moduleId);
    }
}

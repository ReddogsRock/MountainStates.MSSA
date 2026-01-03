using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Events.Repository;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Manager
{
    public class MSSA_EventManager : IMSSA_EventManager, ITransientService
    {
        private readonly IMSSA_EventRepository _repository;

        public MSSA_EventManager(IMSSA_EventRepository repository)
        {
            _repository = repository;
        }

        // Events
        public async Task<IEnumerable<MSSA_Event>> GetEventsAsync(int moduleId)
        {
            return await _repository.GetEventsAsync(moduleId);
        }

        public async Task<MSSA_Event> GetEventAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventAsync(eventId);
        }

        public async Task<MSSA_Event> AddEventAsync(MSSA_Event evt, int moduleId)
        {
            return await _repository.AddEventAsync(evt);
        }

        public async Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt, int moduleId)
        {
            return await _repository.UpdateEventAsync(evt);
        }

        public async Task DeleteEventAsync(int eventId, int moduleId)
        {
            await _repository.DeleteEventAsync(eventId);
        }

        public async Task<IEnumerable<MSSA_Event>> SearchEventsAsync(
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
            int moduleId)
        {
            return await _repository.SearchEventsAsync(
                searchTerm,
                stateCode,
                year,
                cattle,
                sheep,
                arena,
                field,
                onFoot,
                horseback,
                open,
                nursery,
                intermediate,
                novice,
                junior);
        }

        // Trials
        public async Task<IEnumerable<MSSA_Trial>> GetEventTrialsAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventTrialsAsync(eventId);
        }

        public async Task<MSSA_Trial> GetTrialAsync(int trialId, int moduleId)
        {
            return await _repository.GetTrialAsync(trialId);
        }

        public async Task<MSSA_Trial> AddTrialAsync(MSSA_Trial trial, int moduleId)
        {
            return await _repository.AddTrialAsync(trial);
        }

        public async Task<MSSA_Trial> UpdateTrialAsync(MSSA_Trial trial, int moduleId)
        {
            return await _repository.UpdateTrialAsync(trial);
        }

        public async Task DeleteTrialAsync(int trialId, int moduleId)
        {
            await _repository.DeleteTrialAsync(trialId);
        }
    }
}
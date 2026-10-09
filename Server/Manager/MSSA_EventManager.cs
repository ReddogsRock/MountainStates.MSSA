using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Events.Repository;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;

namespace MountainStates.MSSA.Module.MSSA_Events.Manager
{
    public class MSSA_EventManager : IMSSA_EventManager, ITransientService
    {
        private readonly IMSSA_EventRepository _repository;
        private readonly IStripeService _stripeService;

        public MSSA_EventManager(IMSSA_EventRepository repository, IStripeService stripeService)
        {
            _repository = repository;
            _stripeService = stripeService;
        }

        // Events
        public async Task<IEnumerable<MSSA_Event>> GetEventsAsync(int moduleId)
        {
            return await _repository.GetEventsAsync(moduleId);
        }

        public async Task<string> CreateSanctioningFeeCheckoutSessionAsync(int eventId, int quantity, string successUrl, string cancelUrl, int moduleId)
        {
            var evt = await _repository.GetEventAsync(eventId);

            return await _stripeService.CreateSanctioningFeeCheckoutSessionAsync(
                eventId,
                evt?.EventName ?? "Unknown Event",
                quantity,
                evt?.PointYear ?? evt?.StartDate?.Year ?? DateTime.Now.Year,
                successUrl,
                cancelUrl);
        }

        public async Task<MSSA_Event> MarkSanctionFeePaidAsync(int eventId, string stripePaymentIntentId, decimal amount, int moduleId)
        {
            return await _repository.MarkSanctionFeePaidAsync(eventId, stripePaymentIntentId, amount);
        }

        public async Task<MSSA_Event> GetEventAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventAsync(eventId);
        }

        public async Task<List<int>> GetEventTeamMemberUserIdsAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventTeamMemberUserIdsAsync(eventId);
        }

        public async Task AddEventTeamMemberAsync(int eventId, int userId, int moduleId)
        {
            await _repository.AddEventTeamMemberAsync(eventId, userId);
        }

        public async Task RemoveEventTeamMemberAsync(int eventId, int userId, int moduleId)
        {
            await _repository.RemoveEventTeamMemberAsync(eventId, userId);
        }

        public async Task<bool> IsUserOnEventTeamAsync(int eventId, int userId, int moduleId)
        {
            return await _repository.IsUserOnEventTeamAsync(eventId, userId);
        }

        public async Task<bool> IsUserOnEventTeamForTrialAsync(int trialId, int userId, int moduleId)
        {
            return await _repository.IsUserOnEventTeamForTrialAsync(trialId, userId);
        }

        public async Task<List<int>> GetEventIdsForTeamMemberAsync(int userId, int moduleId)
        {
            return await _repository.GetEventIdsForTeamMemberAsync(userId);
        }

        public async Task<MSSA_Event> AddEventAsync(MSSA_Event evt, int moduleId)
        {
            return await _repository.AddEventAsync(evt);
        }

        public async Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt, int moduleId)
        {
            return await _repository.UpdateEventAsync(evt);
        }

        public async Task<MSSA_Event> ApproveEventAsync(int eventId, int approvedByUserId, int moduleId)
        {
            return await _repository.ApproveEventAsync(eventId, approvedByUserId);
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

        // Offerings
        public async Task<List<MSSA_EventClassOffering>> GetEventOfferingsAsync(int eventId, int moduleId)
        {
            return await _repository.GetEventOfferingsAsync(eventId);
        }

        public async Task<List<MSSA_EventClassOffering>> SaveEventOfferingsAsync(int eventId, List<MSSA_EventClassOffering> offerings, int moduleId)
        {
            return await _repository.SaveEventOfferingsAsync(eventId, offerings);
        }

        // Entries
        public async Task<List<EntryListItem>> GetTrialEntriesAsync(int trialId, int moduleId)
        {
            return await _repository.GetTrialEntriesAsync(trialId);
        }
    }
}

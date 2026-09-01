using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Dogs.Repository;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Manager
{
    public class MSSA_DogManager : IMSSA_DogManager, ITransientService
    {
        private readonly IMSSA_DogRepository _repository;
        private readonly IStripeService _stripeService;

        public MSSA_DogManager(IMSSA_DogRepository repository, IStripeService stripeService)
        {
            _repository = repository;
            _stripeService = stripeService;
        }

        public async Task<IEnumerable<MSSA_Dog>> GetDogsAsync(int moduleId)
        {
            return await _repository.GetDogsAsync(moduleId);
        }

        public async Task<MSSA_Dog> GetDogAsync(int dogId, int moduleId)
        {
            return await _repository.GetDogAsync(dogId);
        }

        public async Task<MSSA_Dog> AddDogAsync(MSSA_Dog dog, int moduleId)
        {
            return await _repository.AddDogAsync(dog);
        }

        public async Task<MSSA_Dog> UpdateDogAsync(MSSA_Dog dog, int moduleId)
        {
            return await _repository.UpdateDogAsync(dog);
        }

        public async Task DeleteDogAsync(int dogId, int moduleId)
        {
            await _repository.DeleteDogAsync(dogId);
        }

        public async Task<IEnumerable<MSSA_Dog>> SearchDogsAsync(
            string searchTerm,
            string breed,
            bool? ownerIsMember,
            bool? includeInactive,
            int moduleId)
        {
            return await _repository.SearchDogsAsync(
                searchTerm,
                breed,
                ownerIsMember,
                includeInactive);
        }

        // Futurity
        public async Task<IEnumerable<MSSA_DogFuturityParticipation>> GetDogFuturityParticipationAsync(int dogId, int moduleId)
        {
            return await _repository.GetDogFuturityParticipationAsync(dogId);
        }

        public async Task<MSSA_DogFuturityParticipation> AddFuturityParticipationAsync(MSSA_DogFuturityParticipation participation, int moduleId)
        {
            return await _repository.AddFuturityParticipationAsync(participation);
        }

        public async Task DeleteFuturityParticipationAsync(int participationId, int moduleId)
        {
            await _repository.DeleteFuturityParticipationAsync(participationId);
        }

        public async Task<MSSA_DogFuturityParticipation> SaveFuturityDocumentAsync(int participationId, string fileName, string filePath, int moduleId)
        {
            return await _repository.SaveFuturityDocumentAsync(participationId, fileName, filePath);
        }

        public async Task<MSSA_DogFuturityParticipation> GetFuturityParticipationAsync(int participationId, int moduleId)
        {
            return await _repository.GetFuturityParticipationAsync(participationId);
        }

        public async Task<MSSA_DogFuturityParticipation> MarkFuturityPaymentReceivedAsync(int participationId, string stripePaymentIntentId, decimal amount, int moduleId)
        {
            return await _repository.MarkFuturityPaymentReceivedAsync(participationId, stripePaymentIntentId, amount);
        }

        public async Task<string> CreateFuturityCheckoutSessionAsync(int participationId, string successUrl, string cancelUrl, int moduleId)
        {
            return await _stripeService.CreateFuturityCheckoutSessionAsync(participationId, successUrl, cancelUrl);
        }

        // Entries
        public async Task<IEnumerable<MSSA_DogEntry>> GetDogEntriesAsync(int dogId, int moduleId)
        {
            return await _repository.GetDogEntriesAsync(dogId);
        }
    }
}

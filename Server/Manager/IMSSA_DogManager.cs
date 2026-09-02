using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Manager
{
    public interface IMSSA_DogManager
    {
        Task<IEnumerable<MSSA_Dog>> GetDogsAsync(int moduleId);
        Task<MSSA_Dog> GetDogAsync(int dogId, int moduleId);
        Task<MSSA_Dog> AddDogAsync(MSSA_Dog dog, int moduleId);
        Task<MSSA_Dog> UpdateDogAsync(MSSA_Dog dog, int moduleId);
        Task<MSSA_Dog> UpdateDogStatusAsync(int dogId, bool isActive, bool isDeceased, int moduleId);
        Task<MSSA_Dog> TransferDogOwnershipAsync(int dogId, string newOwnerName, int moduleId);
        Task<MSSA_Dog> MergeDogsAsync(int keepDogId, int mergeDogId, int moduleId);
        Task DeleteDogAsync(int dogId, int moduleId);

        Task<IEnumerable<MSSA_Dog>> SearchDogsAsync(
            string searchTerm,
            string breed,
            bool? ownerIsMember,
            bool? includeInactive,
            int moduleId);

        // Futurity
        Task<IEnumerable<MSSA_DogFuturityParticipation>> GetDogFuturityParticipationAsync(int dogId, int moduleId);
        Task<MSSA_DogFuturityParticipation> GetFuturityParticipationAsync(int participationId, int moduleId);
        Task<MSSA_DogFuturityParticipation> AddFuturityParticipationAsync(MSSA_DogFuturityParticipation participation, int moduleId);
        Task DeleteFuturityParticipationAsync(int participationId, int moduleId);
        Task<MSSA_DogFuturityParticipation> SaveFuturityDocumentAsync(int participationId, string fileName, string filePath, int moduleId);
        Task<MSSA_DogFuturityParticipation> MarkFuturityPaymentReceivedAsync(int participationId, string stripePaymentIntentId, decimal amount, int moduleId);
        Task<string> CreateFuturityCheckoutSessionAsync(int participationId, string successUrl, string cancelUrl, int moduleId);

        // Entries
        Task<IEnumerable<MSSA_DogEntry>> GetDogEntriesAsync(int dogId, int moduleId);
    }
}

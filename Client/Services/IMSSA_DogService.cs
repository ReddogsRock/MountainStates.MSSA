using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Services
{
    public interface IMSSA_DogService
    {
        Task<List<MSSA_Dog>> GetDogsAsync(int moduleId);
        Task<MSSA_Dog> GetDogAsync(int dogId, int moduleId);
        Task<MSSA_Dog> AddDogAsync(MSSA_Dog dog, int moduleId);
        Task<MSSA_Dog> UpdateDogAsync(MSSA_Dog dog, int moduleId);
        Task DeleteDogAsync(int dogId, int moduleId);

        Task<List<MSSA_Dog>> SearchDogsAsync(
            string searchTerm,
            string breed,
            bool? ownerIsMember,
            bool? includeInactive,
            int moduleId);

        // Futurity
        Task<List<MSSA_DogFuturityParticipation>> GetDogFuturityParticipationAsync(int dogId, int moduleId);
        Task<MSSA_DogFuturityParticipation> AddFuturityParticipationAsync(MSSA_DogFuturityParticipation participation, int moduleId);
        Task DeleteFuturityParticipationAsync(int participationId, int moduleId);

        // Entries
        Task<List<MSSA_DogEntry>> GetDogEntriesAsync(int dogId, int moduleId);
    }
}

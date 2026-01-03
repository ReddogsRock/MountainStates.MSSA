using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Repository
{
    public interface IMSSA_DogRepository
    {
        Task<IEnumerable<MSSA_Dog>> GetDogsAsync(int moduleId);
        Task<MSSA_Dog> GetDogAsync(int dogId);
        Task<MSSA_Dog> AddDogAsync(MSSA_Dog dog);
        Task<MSSA_Dog> UpdateDogAsync(MSSA_Dog dog);
        Task DeleteDogAsync(int dogId);

        // Search and filter
        Task<IEnumerable<MSSA_Dog>> SearchDogsAsync(
            string searchTerm = null,
            string breed = null,
            bool? ownerIsMember = null,
            bool? includeInactive = null);

        // Futurity
        Task<IEnumerable<MSSA_DogFuturityParticipation>> GetDogFuturityParticipationAsync(int dogId);
        Task<MSSA_DogFuturityParticipation> AddFuturityParticipationAsync(MSSA_DogFuturityParticipation participation);
        Task DeleteFuturityParticipationAsync(int participationId);

        // Entries for detail view
        Task<IEnumerable<MSSA_DogEntry>> GetDogEntriesAsync(int dogId);
    }
}

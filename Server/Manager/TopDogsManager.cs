using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.TopDogs.Repository;

namespace MountainStates.MSSA.Module.TopDogs.Manager
{
    public class TopDogsManager : ITopDogsManager, ITransientService
    {
        private readonly ITopDogsRepository _repository;

        public TopDogsManager(ITopDogsRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Models.TopDog>> GetTopDogsAsync(int year, int level, string species, int quantity)
        {
            return _repository.GetTopDogsAsync(year, level, species, quantity);
        }
    }

    public interface ITopDogsManager
    {
        Task<IEnumerable<Models.TopDog>> GetTopDogsAsync(int year, int level, string species, int quantity);
    }
}
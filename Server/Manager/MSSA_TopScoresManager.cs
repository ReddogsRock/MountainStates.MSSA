using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_TopScores.Models;
using MountainStates.MSSA.Module.MSSA_TopScores.Repository;

namespace MountainStates.MSSA.Module.MSSA_TopScores.Manager
{
    public class MSSA_TopScoresManager : IMSSA_TopScoresManager, ITransientService
    {
        private readonly IMSSA_TopScoresRepository _repository;

        public MSSA_TopScoresManager(IMSSA_TopScoresRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<TopScoreResult>> GetTopScoresAsync(int moduleId, TopScoreParameters parameters)
        {
            return _repository.GetTopScoresAsync(parameters);
        }

        public Task<IEnumerable<int>> GetAvailableYearsAsync(int moduleId)
        {
            return _repository.GetAvailableYearsAsync();
        }
    }
}
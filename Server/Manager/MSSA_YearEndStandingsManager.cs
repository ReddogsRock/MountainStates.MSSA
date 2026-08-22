using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Models;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Repository;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Manager
{
    public class MSSA_YearEndStandingsManager : IMSSA_YearEndStandingsManager, ITransientService
    {
        private readonly IMSSA_YearEndStandingsRepository _repository;

        public MSSA_YearEndStandingsManager(IMSSA_YearEndStandingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MSSA_YearEndStanding>> GetStandingsAsync(int? year, string level, string species, int moduleId)
        {
            return await _repository.GetStandingsAsync(year, level, species);
        }

        public async Task<List<YearEndStandingDetail>> GetStandingDetailsAsync(int dogId, int? handlerId, int? year, string level, string species, int moduleId)
        {
            return await _repository.GetStandingDetailsAsync(dogId, handlerId, year, level, species);
        }

        public async Task<List<int>> GetAvailableYearsAsync(int moduleId)
        {
            return await _repository.GetAvailableYearsAsync();
        }

        public async Task<List<string>> GetLevelsAsync(int moduleId)
        {
            return await _repository.GetLevelsAsync();
        }
    }
}

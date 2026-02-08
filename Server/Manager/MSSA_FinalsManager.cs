using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Finals.Repository;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Finals.Manager
{
    public class MSSA_FinalsManager : IMSSA_FinalsManager, ITransientService
    {
        private readonly IMSSA_FinalsRepository _repository;

        public MSSA_FinalsManager(IMSSA_FinalsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MSSA_FinalsResult>> GetFinalsResultsAsync(int moduleId)
        {
            return await _repository.GetFinalsResultsAsync(moduleId);
        }

        public async Task<IEnumerable<MSSA_FinalsResult>> SearchFinalsResultsAsync(
            int? year,
            string level,
            string stock,
            string round,
            int? place,
            int moduleId)
        {
            return await _repository.SearchFinalsResultsAsync(year, level, stock, round, place);
        }

        public async Task<IEnumerable<int>> GetAvailableYearsAsync(int moduleId)
        {
            return await _repository.GetAvailableYearsAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableLevelsAsync(int moduleId)
        {
            return await _repository.GetAvailableLevelsAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableStocksAsync(int moduleId)
        {
            return await _repository.GetAvailableStocksAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableRoundsAsync(int? year, string level, string stock, int moduleId)
        {
            return await _repository.GetAvailableRoundsAsync(year, level, stock);
        }

        public async Task<IEnumerable<MSSA_UltimateBreakdown>> GetUltimateBreakdownAsync(int year, string level, int handlerId, int dogId, int moduleId)
        {
            return await _repository.GetUltimateBreakdownAsync(year, level, handlerId, dogId);
        }
    }
}

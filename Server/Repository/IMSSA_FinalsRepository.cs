using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Finals.Repository
{
    public interface IMSSA_FinalsRepository
    {
        Task<IEnumerable<MSSA_FinalsResult>> GetFinalsResultsAsync(int moduleId);

        Task<IEnumerable<MSSA_FinalsResult>> SearchFinalsResultsAsync(
            int? year = null,
            string level = null,
            string stock = null,
            string round = null,
            int? place = null);

        Task<IEnumerable<int>> GetAvailableYearsAsync();
        Task<IEnumerable<string>> GetAvailableLevelsAsync();
        Task<IEnumerable<string>> GetAvailableStocksAsync();
        Task<IEnumerable<string>> GetAvailableRoundsAsync(int? year = null, string level = null, string stock = null);

        // For Ultimate breakdown
        Task<IEnumerable<MSSA_UltimateBreakdown>> GetUltimateBreakdownAsync(int year, string level, int handlerId, int dogId);
    }
}

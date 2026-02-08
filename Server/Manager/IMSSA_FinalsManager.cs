using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Finals.Manager
{
    public interface IMSSA_FinalsManager
    {
        Task<IEnumerable<MSSA_FinalsResult>> GetFinalsResultsAsync(int moduleId);

        Task<IEnumerable<MSSA_FinalsResult>> SearchFinalsResultsAsync(
            int? year,
            string level,
            string stock,
            string round,
            int? place,
            int moduleId);

        Task<IEnumerable<int>> GetAvailableYearsAsync(int moduleId);
        Task<IEnumerable<string>> GetAvailableLevelsAsync(int moduleId);
        Task<IEnumerable<string>> GetAvailableStocksAsync(int moduleId);
        Task<IEnumerable<string>> GetAvailableRoundsAsync(int? year, string level, string stock, int moduleId);

        Task<IEnumerable<MSSA_UltimateBreakdown>> GetUltimateBreakdownAsync(int year, string level, int handlerId, int dogId, int moduleId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Finals.Services
{
    public interface IMSSA_FinalsService
    {
        Task<List<MSSA_FinalsResult>> GetFinalsResultsAsync(int moduleId);

        Task<List<MSSA_FinalsResult>> SearchFinalsResultsAsync(
            int? year,
            string level,
            string stock,
            string round,
            int? place,
            int moduleId);

        Task<List<int>> GetAvailableYearsAsync(int moduleId);
        Task<List<string>> GetAvailableLevelsAsync(int moduleId);
        Task<List<string>> GetAvailableStocksAsync(int moduleId);
        Task<List<string>> GetAvailableRoundsAsync(int? year, string level, string stock, int moduleId);

        Task<List<MSSA_UltimateBreakdown>> GetUltimateBreakdownAsync(int year, string level, int handlerId, int dogId, int moduleId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Models;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Manager
{
    public interface IMSSA_YearEndStandingsManager
    {
        Task<List<MSSA_YearEndStanding>> GetStandingsAsync(int? year, string level, string species, int moduleId);
        Task<List<YearEndStandingDetail>> GetStandingDetailsAsync(int dogId, int? handlerId, int? year, string level, string species, int moduleId);
        Task<List<int>> GetAvailableYearsAsync(int moduleId);
        Task<List<string>> GetLevelsAsync(int moduleId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_TopScores.Models;

namespace MountainStates.MSSA.Module.MSSA_TopScores.Services
{
    public interface IMSSA_TopScoresService
    {
        Task<IEnumerable<TopScoreResult>> GetTopScoresAsync(int moduleId, TopScoreParameters parameters);
        Task<IEnumerable<int>> GetAvailableYearsAsync(int moduleId);
    }
}
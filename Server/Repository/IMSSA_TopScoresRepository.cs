using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_TopScores.Models;

namespace MountainStates.MSSA.Module.MSSA_TopScores.Repository
{
    public interface IMSSA_TopScoresRepository
    {
        Task<IEnumerable<TopScoreResult>> GetTopScoresAsync(TopScoreParameters parameters);
        Task<IEnumerable<int>> GetAvailableYearsAsync();
    }
}

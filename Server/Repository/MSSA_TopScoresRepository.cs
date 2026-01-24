using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_TopScores.Models;
using Oqtane.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_TopScores.Repository
{
    public class MSSA_TopScoresRepository : IMSSA_TopScoresRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_TopScoresRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<TopScoreResult>> GetTopScoresAsync(TopScoreParameters parameters)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            // First, check if this class accumulates by dog only
            var classInfo = await db.MSSA_Classes
                .Where(c => c.ClassName == parameters.ClassName && c.IsActive)
                .FirstOrDefaultAsync();

            if (classInfo == null)
            {
                return new List<TopScoreResult>();
            }

            var isDogStandings = classInfo.PointsAccumulateByDogOnly;

            if (isDogStandings)
            {
                // Dog standings - group by Dog only, show owner name
                var results = await (from e in db.MSSA_Entries
                                     join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                     join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                     join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                     join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                     where ev.PointYear == parameters.Year
                                        && t.Stock == parameters.Stock
                                        && c.ClassName == parameters.ClassName
                                        && c.IsActive
                                        && e.TrialPoints.HasValue
                                     group e by new { d.DogId, d.Name, d.OwnerName } into g
                                     orderby g.Sum(x => x.TrialPoints.Value) descending
                                     select new TopScoreResult
                                     {
                                         DogId = g.Key.DogId,
                                         DogName = g.Key.Name,
                                         HandlerName = g.Key.OwnerName ?? "Unknown Owner",
                                         TotalPoints = g.Sum(x => x.TrialPoints.Value)
                                     })
                                    .Take(parameters.Quantity)
                                    .ToListAsync();

                // Add rank
                int rank = 1;
                foreach (var result in results)
                {
                    result.Rank = rank++;
                }

                return results;
            }
            else
            {
                // Dog/Handler combo standings (combines Horseback and On-foot)
                var results = await (from e in db.MSSA_Entries
                                     join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                     join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                     join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                     join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                                     join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                     where ev.PointYear == parameters.Year
                                        && t.Stock == parameters.Stock
                                        && c.ClassName == parameters.ClassName
                                        && c.IsActive
                                        && e.TrialPoints.HasValue
                                     group e by new { d.DogId, d.Name, h.HandlerId, h.FullName } into g
                                     orderby g.Sum(x => x.TrialPoints.Value) descending
                                     select new TopScoreResult
                                     {
                                         DogId = g.Key.DogId,
                                         DogName = g.Key.Name,
                                         HandlerId = g.Key.HandlerId,
                                         HandlerName = g.Key.FullName,
                                         TotalPoints = g.Sum(x => x.TrialPoints.Value)
                                     })
                                    .Take(parameters.Quantity)
                                    .ToListAsync();

                // Add rank
                int rank = 1;
                foreach (var result in results)
                {
                    result.Rank = rank++;
                }

                return results;
            }
        }

        public async Task<IEnumerable<int>> GetAvailableYearsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var years = await db.MSSA_Events
                .Where(e => e.PointYear.HasValue)
                .Select(e => e.PointYear.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            return years;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Finals.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using Oqtane.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Finals.Repository
{
    public class MSSA_FinalsRepository : IMSSA_FinalsRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_FinalsRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<MSSA_FinalsResult>> GetFinalsResultsAsync(int moduleId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            // Query the view
            return await db.vw_AllFinalsResults
                .OrderByDescending(r => r.Year)
                .ThenBy(r => r.Level)
                .ThenBy(r => r.Stock)
                .ThenBy(r => r.Place)
                .ToListAsync();
        }

        public async Task<IEnumerable<MSSA_FinalsResult>> SearchFinalsResultsAsync(
            int? year = null,
            string level = null,
            string stock = null,
            string round = null,
            int? place = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.vw_AllFinalsResults.AsQueryable();

            // Apply filters
            if (year.HasValue)
            {
                query = query.Where(r => r.Year == year.Value);
            }

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(r => r.Level == level);
            }

            if (!string.IsNullOrEmpty(stock))
            {
                query = query.Where(r => r.Stock == stock);

                // If Ultimate, ignore round filter
                if (stock != "Ultimate" && !string.IsNullOrEmpty(round))
                {
                    query = query.Where(r => r.Round == round);
                }
            }

            if (place.HasValue)
            {
                query = query.Where(r => r.Place == place.Value);
            }

            return await query
                .OrderBy(r => r.Place)
                .ThenByDescending(r => r.TotalPoints)
                .ThenBy(r => r.TotalTimeSeconds)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetAvailableYearsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.vw_AllFinalsResults
                .Select(r => r.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableLevelsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.vw_AllFinalsResults
                .Select(r => r.Level)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableStocksAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var stocks = await db.vw_AllFinalsResults
                .Select(r => r.Stock)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return stocks;
        }

        public async Task<IEnumerable<string>> GetAvailableRoundsAsync(int? year = null, string level = null, string stock = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.vw_AllFinalsResults.AsQueryable();

            if (year.HasValue)
            {
                query = query.Where(r => r.Year == year.Value);
            }

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(r => r.Level == level);
            }

            if (!string.IsNullOrEmpty(stock) && stock != "Ultimate")
            {
                query = query.Where(r => r.Stock == stock);
            }

            return await query
                .Select(r => r.Round)
                .Where(r => r != null)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        // Get the breakdown of rounds for an Ultimate result
        public async Task<IEnumerable<MSSA_UltimateBreakdown>> GetUltimateBreakdownAsync(int year, string level, int handlerId, int dogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var breakdown = await db.MSSA_FinalsData
                .Where(r => r.Year == year 
                    && r.Level == level 
                    && r.HandlerId == handlerId 
                    && r.DogId == dogId
                    && (r.Stock == "Cattle" || r.Stock == "Sheep"))
                .Select(r => new MSSA_UltimateBreakdown
                {
                    Year = r.Year.Value,
                    Level = r.Level,
                    HandlerId = r.HandlerId,
                    DogId = r.DogId,
                    HandlerName = r.HandlerName,
                    DogName = r.DogName,
                    Stock = r.Stock,
                    Round = r.Round,
                    Place = r.Place,
                    Pts = r.Pts,
                    TimeSeconds = r.TimeSeconds,
                    SourceFile = r.SourceFile
                })
                .OrderBy(r => r.Stock)
                .ThenBy(r => r.Round)
                .ToListAsync();

            return breakdown;
        }
    }
}

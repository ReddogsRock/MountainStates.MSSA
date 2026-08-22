using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Models;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Repository
{
    // Replicates the [YearEndStandings] stored procedure's logic in LINQ, rather than
    // calling the SP directly, to stay consistent with the rest of this app's EF Core
    // data-access pattern and to apply the same 2-digit/4-digit PointYear normalization
    // established for TopScores/Finals/Entries earlier.
    //
    // "Level" here is a ClassName (e.g. "Open", "Novice"), not a single ClassId - On-foot
    // and Horseback are combined under one level, so a level maps to whichever ClassIds
    // share that ClassName. "Futurity" is not a real class at all: it's Nursery entries
    // restricted to dogs nominated in MSSA_DogFuturityParticipation, kept as a special
    // case here rather than a row in MSSA_Classes (see conversation) so it can't leak
    // into Entries/Events/TopScores' class pickers, which have no meaning for it.
    public class MSSA_YearEndStandingsRepository : IMSSA_YearEndStandingsRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        private const string FuturityLevel = "Futurity";

        public MSSA_YearEndStandingsRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<MSSA_YearEndStanding>> GetStandingsAsync(int? year, string level, string species)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            bool isFuturity = string.Equals(level, FuturityLevel, StringComparison.OrdinalIgnoreCase);
            var lookupClassName = isFuturity ? "Nursery" : level;

            // Futurity is Nursery/Cattle only - overrides whatever species was passed in,
            // regardless of what the UI sends, so this can never accidentally include Sheep.
            if (isFuturity)
            {
                species = "Cattle";
            }

            var matchingClasses = await db.MSSA_Classes
                .Where(c => c.ClassName == lookupClassName)
                .ToListAsync();

            if (!matchingClasses.Any())
            {
                return new List<MSSA_YearEndStanding>();
            }

            var classIds = matchingClasses.Select(c => c.ClassId).ToList();
            var dogOnly = matchingClasses.First().PointsAccumulateByDogOnly;

            var baseQuery = from e in db.MSSA_Entries
                             join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                             join ev in db.MSSA_Events on t.EventId equals ev.EventId
                             where classIds.Contains(e.ClassId) && e.TrialPoints.HasValue
                             select new { e.DogId, e.HandlerId, e.TrialPoints, ev.PointYear, t.Stock };

            // "All" combines Cattle + Sheep - skip the Stock filter entirely rather
            // than trying to match a literal "All" value against the column.
            if (!string.IsNullOrEmpty(species) && !string.Equals(species, "All", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(x => x.Stock == species);
            }

            if (year.HasValue)
            {
                // Inline ternary so EF Core translates this to a SQL CASE expression
                // rather than requiring a call to a C# helper method it can't translate.
                baseQuery = baseQuery.Where(x =>
                    (x.PointYear.HasValue ? (x.PointYear.Value < 100 ? 2000 + x.PointYear.Value : x.PointYear.Value) : (int?)null) == year.Value);
            }
            // year == null => Lifetime: no year filter at all, matching every event.

            if (isFuturity)
            {
                // Restrict to dogs actually nominated for Futurity. For a specific year,
                // nominated for that year specifically (matches the same year the entries
                // are already scoped to above, so "nominated" and "scored" line up). For
                // Lifetime, nominated in any year - their whole Nursery history then counts,
                // not just the nominated year's runs.
                IQueryable<int> nominatedDogIds = year.HasValue
                    ? db.MSSA_DogFuturityParticipation.Where(f => f.Year == year.Value).Select(f => f.DogId)
                    : db.MSSA_DogFuturityParticipation.Select(f => f.DogId).Distinct();

                baseQuery = baseQuery.Where(x => nominatedDogIds.Contains(x.DogId));
            }

            List<MSSA_YearEndStanding> results;

            if (dogOnly)
            {
                var grouped = await baseQuery
                    .GroupBy(x => x.DogId)
                    .Select(g => new { DogId = g.Key, TotalPoints = g.Sum(x => x.TrialPoints.Value) })
                    .Where(g => g.TotalPoints > 0)
                    .ToListAsync();

                var dogIds = grouped.Select(g => g.DogId).ToList();
                var dogs = await db.MSSA_Dogs
                    .Where(d => dogIds.Contains(d.DogId))
                    .ToDictionaryAsync(d => d.DogId, d => d);

                results = grouped
                    .OrderByDescending(g => g.TotalPoints)
                    .Select((g, idx) => new MSSA_YearEndStanding
                    {
                        Rank = idx + 1,
                        DogId = g.DogId,
                        DogName = dogs.TryGetValue(g.DogId, out var dog) ? dog.Name : "Unknown",
                        // Dog-only classes have no single Handler (points can come from
                        // runs with different handlers) - show the dog's Owner instead.
                        HandlerId = null,
                        HandlerName = dogs.TryGetValue(g.DogId, out var d2) ? d2.OwnerName : null,
                        TotalPoints = g.TotalPoints
                    })
                    .ToList();
            }
            else
            {
                var grouped = await baseQuery
                    .GroupBy(x => new { x.DogId, x.HandlerId })
                    .Select(g => new { g.Key.DogId, g.Key.HandlerId, TotalPoints = g.Sum(x => x.TrialPoints.Value) })
                    .Where(g => g.TotalPoints > 0)
                    .ToListAsync();

                var dogIds = grouped.Select(g => g.DogId).Distinct().ToList();
                var handlerIds = grouped.Select(g => g.HandlerId).Distinct().ToList();

                var dogNames = await db.MSSA_Dogs
                    .Where(d => dogIds.Contains(d.DogId))
                    .ToDictionaryAsync(d => d.DogId, d => d.Name);
                var handlerNames = await db.MSSA_Handlers
                    .Where(h => handlerIds.Contains(h.HandlerId))
                    .ToDictionaryAsync(h => h.HandlerId, h => h.FullName);

                results = grouped
                    .OrderByDescending(g => g.TotalPoints)
                    .Select((g, idx) => new MSSA_YearEndStanding
                    {
                        Rank = idx + 1,
                        DogId = g.DogId,
                        DogName = dogNames.TryGetValue(g.DogId, out var dn) ? dn : "Unknown",
                        HandlerId = g.HandlerId,
                        HandlerName = handlerNames.TryGetValue(g.HandlerId, out var hn) ? hn : "Unknown",
                        TotalPoints = g.TotalPoints
                    })
                    .ToList();
            }

            return results;
        }

        public async Task<List<YearEndStandingDetail>> GetStandingDetailsAsync(int dogId, int? handlerId, int? year, string level, string species)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            bool isFuturity = string.Equals(level, FuturityLevel, StringComparison.OrdinalIgnoreCase);
            var lookupClassName = isFuturity ? "Nursery" : level;

            if (isFuturity)
            {
                species = "Cattle";
            }

            var classIds = await db.MSSA_Classes
                .Where(c => c.ClassName == lookupClassName)
                .Select(c => c.ClassId)
                .ToListAsync();

            var query = from e in db.MSSA_Entries
                        join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                        join ev in db.MSSA_Events on t.EventId equals ev.EventId
                        join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                        where classIds.Contains(e.ClassId) && e.TrialPoints.HasValue && e.DogId == dogId
                        select new { e, t, ev, h };

            if (!string.IsNullOrEmpty(species) && !string.Equals(species, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.t.Stock == species);
            }

            if (handlerId.HasValue)
            {
                query = query.Where(x => x.e.HandlerId == handlerId.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(x =>
                    (x.ev.PointYear.HasValue ? (x.ev.PointYear.Value < 100 ? 2000 + x.ev.PointYear.Value : x.ev.PointYear.Value) : (int?)null) == year.Value);
            }

            return await query
                .OrderByDescending(x => x.ev.PointYear)
                .ThenBy(x => x.t.TrialDate)
                .Select(x => new YearEndStandingDetail
                {
                    EventName = x.ev.EventName,
                    TrialName = x.t.TrialName,
                    TrialDate = x.t.TrialDate,
                    HandlerName = x.h.FullName,
                    Stock = x.t.Stock,
                    Points = x.e.TrialPoints.Value,
                    PointYear = x.ev.PointYear
                })
                .ToListAsync();
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var rawYears = await db.MSSA_Events
                .Where(e => e.PointYear.HasValue)
                .Select(e => e.PointYear.Value)
                .Distinct()
                .ToListAsync();

            return rawYears
                .Select(y => y < 100 ? 2000 + y : y)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
        }

        public async Task<List<string>> GetLevelsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var names = await db.MSSA_Classes
                .Where(c => c.IsActive)
                .Select(c => c.ClassName)
                .Distinct()
                .ToListAsync();

            return names
                .Where(n => !IsProNovice(n))
                .OrderBy(n => n)
                .ToList();
        }

        private static bool IsProNovice(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                return false;
            }
            var normalized = className.Replace("-", " ").Trim();
            return string.Equals(normalized, "Pro Novice", StringComparison.OrdinalIgnoreCase);
        }
    }
}

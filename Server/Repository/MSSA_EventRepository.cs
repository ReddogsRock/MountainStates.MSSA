using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Events.Repository
{
    public class MSSA_EventRepository : IMSSA_EventRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_EventRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // Events
        public async Task<IEnumerable<MSSA_Event>> GetEventsAsync(int moduleId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var events = await db.MSSA_Events
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            // Populate state names and trial counts
            var stateCodes = events.Select(e => e.StateCode).Distinct().ToList();
            var states = await db.MSSA_States
                .Where(s => stateCodes.Contains(s.StateCode))
                .ToDictionaryAsync(s => s.StateCode, s => s.StateName);

            var trialCounts = await db.MSSA_Trials
                .GroupBy(t => t.EventId)
                .Select(g => new { EventId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EventId, x => x.Count);

            foreach (var evt in events)
            {
                if (!string.IsNullOrEmpty(evt.StateCode) && states.ContainsKey(evt.StateCode))
                {
                    evt.StateName = states[evt.StateCode];
                }
                evt.TrialCount = trialCounts.ContainsKey(evt.EventId) ? trialCounts[evt.EventId] : 0;
            }

            return events;
        }

        public async Task<MSSA_Event> GetEventAsync(int eventId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var evt = await db.MSSA_Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (evt != null)
            {
                // Populate state name
                if (!string.IsNullOrEmpty(evt.StateCode))
                {
                    var state = await db.MSSA_States
                        .FirstOrDefaultAsync(s => s.StateCode == evt.StateCode);
                    evt.StateName = state?.StateName;
                }

                // Get trial count
                evt.TrialCount = await db.MSSA_Trials
                    .CountAsync(t => t.EventId == eventId);
            }

            return evt;
        }

        public async Task<MSSA_Event> AddEventAsync(MSSA_Event evt)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            evt.CreatedDate = DateTime.UtcNow;
            evt.ModifiedDate = DateTime.UtcNow;

            db.MSSA_Events.Add(evt);
            await db.SaveChangesAsync();

            return evt;
        }

        public async Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            evt.ModifiedDate = DateTime.UtcNow;

            db.Entry(evt).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return evt;
        }

        public async Task DeleteEventAsync(int eventId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var evt = await db.MSSA_Events.FindAsync(eventId);
            if (evt != null)
            {
                // Soft delete
                evt.IsActive = false;
                evt.ModifiedDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MSSA_Event>> SearchEventsAsync(
            string searchTerm = null,
            string stateCode = null,
            int? year = null,
            bool? cattle = null,
            bool? sheep = null,
            bool? arena = null,
            bool? field = null,
            bool? onFoot = null,
            bool? horseback = null,
            bool? open = null,
            bool? nursery = null,
            bool? intermediate = null,
            bool? novice = null,
            bool? junior = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Events.Where(e => e.IsActive);

            // Apply search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e =>
                    e.EventName.ToLower().Contains(searchTerm) ||
                    e.EventIdentifier.ToLower().Contains(searchTerm) ||
                    (e.City != null && e.City.ToLower().Contains(searchTerm)));
            }

            // Filter by state
            if (!string.IsNullOrWhiteSpace(stateCode))
            {
                query = query.Where(e => e.StateCode == stateCode);
            }

            // Filter by year
            if (year.HasValue)
            {
                query = query.Where(e => e.PointYear == year.Value ||
                    (e.StartDate.HasValue && e.StartDate.Value.Year == year.Value));
            }

            // Filter by planning flags
            if (cattle.HasValue && cattle.Value)
                query = query.Where(e => e.Cattle);
            if (sheep.HasValue && sheep.Value)
                query = query.Where(e => e.Sheep);
            if (arena.HasValue && arena.Value)
                query = query.Where(e => e.Arena);
            if (field.HasValue && field.Value)
                query = query.Where(e => e.Field);
            if (onFoot.HasValue && onFoot.Value)
                query = query.Where(e => e.OnFoot);
            if (horseback.HasValue && horseback.Value)
                query = query.Where(e => e.Horseback);
            if (open.HasValue && open.Value)
                query = query.Where(e => e.Open);
            if (nursery.HasValue && nursery.Value)
                query = query.Where(e => e.Nursery);
            if (intermediate.HasValue && intermediate.Value)
                query = query.Where(e => e.Intermediate);
            if (novice.HasValue && novice.Value)
                query = query.Where(e => e.Novice);
            if (junior.HasValue && junior.Value)
                query = query.Where(e => e.Junior);

            var events = await query
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            // Populate state names and trial counts
            var stateCodes = events.Select(e => e.StateCode).Distinct().ToList();
            var states = await db.MSSA_States
                .Where(s => stateCodes.Contains(s.StateCode))
                .ToDictionaryAsync(s => s.StateCode, s => s.StateName);

            var trialCounts = await db.MSSA_Trials
                .Where(t => events.Select(e => e.EventId).Contains(t.EventId))
                .GroupBy(t => t.EventId)
                .Select(g => new { EventId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EventId, x => x.Count);

            foreach (var evt in events)
            {
                if (!string.IsNullOrEmpty(evt.StateCode) && states.ContainsKey(evt.StateCode))
                {
                    evt.StateName = states[evt.StateCode];
                }
                evt.TrialCount = trialCounts.ContainsKey(evt.EventId) ? trialCounts[evt.EventId] : 0;
            }

            return events;
        }

        // Trials
        public async Task<IEnumerable<MSSA_Trial>> GetEventTrialsAsync(int eventId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Trials
                .Where(t => t.EventId == eventId)
                .OrderBy(t => t.TrialDate)
                .ThenBy(t => t.TrialName)
                .ToListAsync();
        }

        public async Task<MSSA_Trial> GetTrialAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Trials
                .FirstOrDefaultAsync(t => t.TrialId == trialId);
        }

        public async Task<MSSA_Trial> AddTrialAsync(MSSA_Trial trial)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            trial.CreatedDate = DateTime.UtcNow;
            trial.ModifiedDate = DateTime.UtcNow;

            db.MSSA_Trials.Add(trial);
            await db.SaveChangesAsync();

            return trial;
        }

        public async Task<MSSA_Trial> UpdateTrialAsync(MSSA_Trial trial)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            trial.ModifiedDate = DateTime.UtcNow;

            db.Entry(trial).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return trial;
        }

        public async Task DeleteTrialAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var trial = await db.MSSA_Trials.FindAsync(trialId);
            if (trial != null)
            {
                db.MSSA_Trials.Remove(trial);
                await db.SaveChangesAsync();
            }
        }

        // Entries
        public async Task<List<EntryListItem>> GetTrialEntriesAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await (from e in db.MSSA_Entries
                                 join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                                 join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                 join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                 where e.TrialId == trialId
                                 select new EntryListItem
                                 {
                                     EntryId = e.EntryId,
                                     TrialId = e.TrialId,
                                     HandlerName = h.FullName,
                                     DogName = d.Name,
                                     ClassName = c.ClassName,
                                     SubClassName = c.SubClassName,
                                     RunOrder = e.RunOrder,
                                     Placing = e.Placing,
                                     RunTime = e.RunTime,
                                     TieBreakerTime = e.TieBreakerTime,
                                     SumOfObstacles = (e.ObstacleScore1 ?? 0) + (e.ObstacleScore2 ?? 0) +
                                                     (e.ObstacleScore3 ?? 0) + (e.ObstacleScore4 ?? 0) +
                                                     (e.ObstacleScore5 ?? 0) + (e.ObstacleScore6 ?? 0) +
                                                     (e.ObstacleScore7 ?? 0) + (e.ObstacleScore8 ?? 0) +
                                                     (e.ObstacleScore9 ?? 0),
                                     TrialPoints = e.TrialPoints
                                 })
                                .ToListAsync();

            return entries;
        }
    }
}

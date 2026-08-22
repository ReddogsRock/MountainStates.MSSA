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

            var eventIds = events.Select(e => e.EventId).ToList();
            var offeringsByEvent = await LoadOfferingsForEventsAsync(db, eventIds);

            foreach (var evt in events)
            {
                if (!string.IsNullOrEmpty(evt.StateCode) && states.ContainsKey(evt.StateCode))
                {
                    evt.StateName = states[evt.StateCode];
                }
                evt.TrialCount = trialCounts.ContainsKey(evt.EventId) ? trialCounts[evt.EventId] : 0;
                evt.Offerings = offeringsByEvent.TryGetValue(evt.EventId, out var offerings) ? offerings : new List<MSSA_EventClassOffering>();
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

                // Get planned run offerings
                evt.Offerings = await GetEventOfferingsAsync(eventId);
            }

            return evt;
        }

        public async Task<MSSA_Event> AddEventAsync(MSSA_Event evt)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            evt.CreatedDate = DateTime.UtcNow;
            evt.ModifiedDate = DateTime.UtcNow;

            var offerings = evt.Offerings;
            evt.Offerings = new List<MSSA_EventClassOffering>(); // not mapped - keep EF from touching it

            db.MSSA_Events.Add(evt);
            await db.SaveChangesAsync();

            if (offerings != null && offerings.Any())
            {
                foreach (var offering in offerings)
                {
                    offering.OfferingId = 0;
                    offering.EventId = evt.EventId;
                    db.MSSA_EventClassOfferings.Add(offering);
                }
                await db.SaveChangesAsync();

                await EnsureTrialsForOfferingsAsync(db, evt, offerings);
            }

            evt.Offerings = await LoadOfferingsAsync(db, evt.EventId);

            return evt;
        }

        public async Task<MSSA_Event> UpdateEventAsync(MSSA_Event evt)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            evt.ModifiedDate = DateTime.UtcNow;

            var offerings = evt.Offerings;
            evt.Offerings = new List<MSSA_EventClassOffering>(); // not mapped - keep EF from touching it

            db.Entry(evt).State = EntityState.Modified;
            await db.SaveChangesAsync();

            if (offerings != null)
            {
                var existing = await db.MSSA_EventClassOfferings.Where(o => o.EventId == evt.EventId).ToListAsync();
                db.MSSA_EventClassOfferings.RemoveRange(existing);

                foreach (var offering in offerings)
                {
                    offering.OfferingId = 0;
                    offering.EventId = evt.EventId;
                    db.MSSA_EventClassOfferings.Add(offering);
                }
                await db.SaveChangesAsync();

                if (offerings.Any())
                {
                    await EnsureTrialsForOfferingsAsync(db, evt, offerings);
                }
            }

            evt.Offerings = await LoadOfferingsAsync(db, evt.EventId);

            return evt;
        }

        // Auto-creates Trials to match the event's planning, grouped by Stock+Venue so
        // each Trial comes pre-filled - trial hosts shouldn't have to know to set
        // Stock/Venue by hand on every Trial. Within each Stock+Venue group, the target
        // count is the highest PlannedRuns among that group's offerings (not a sum, since
        // one Trial session can host runs for multiple classes that share the same
        // Stock+Venue).
        //
        // Safe to call on both create and edit: only ADDS trials to catch up to a higher
        // planned-run count. Never removes or renumbers existing trials, so nothing
        // already scheduled/scored gets disturbed if a count goes down or stays the same.
        private static async Task EnsureTrialsForOfferingsAsync(MSSADbContext db, MSSA_Event evt, List<MSSA_EventClassOffering> offerings)
        {
            var baseDate = evt.StartDate ?? DateTime.Today;

            var groups = offerings.GroupBy(o => new { o.Stock, o.Venue });
            foreach (var group in groups)
            {
                var targetCount = group.Max(o => o.PlannedRuns);
                var stock = group.Key.Stock;
                var venue = group.Key.Venue;

                var existingCount = await db.MSSA_Trials
                    .CountAsync(t => t.EventId == evt.EventId && t.Stock == stock && t.Venue == venue);

                for (int i = existingCount + 1; i <= targetCount; i++)
                {
                    var suffix = $"-{stock}-{venue}-T{i}";
                    var prefixBudget = Math.Max(0, 50 - suffix.Length);
                    var prefix = evt.EventIdentifier.Length > prefixBudget
                        ? evt.EventIdentifier.Substring(0, prefixBudget)
                        : evt.EventIdentifier;

                    db.MSSA_Trials.Add(new MSSA_Trial
                    {
                        EventId = evt.EventId,
                        TrialIdentifier = $"{prefix}{suffix}",
                        TrialName = $"{stock} {venue} - Trial {i}",
                        TrialDate = baseDate,
                        Stock = stock,
                        Venue = venue,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    });
                }
            }

            await db.SaveChangesAsync();
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

            // Filter by offerings (replaces the old boolean planning flags).
            // Stock and Venue are columns on the Offering row itself; Class filters
            // match against MSSA_Class.ClassName via the Offering's ClassId.
            if (cattle == true)
            {
                var ids = db.MSSA_EventClassOfferings.Where(o => o.Stock == "Cattle").Select(o => o.EventId);
                query = query.Where(e => ids.Contains(e.EventId));
            }
            if (sheep == true)
            {
                var ids = db.MSSA_EventClassOfferings.Where(o => o.Stock == "Sheep").Select(o => o.EventId);
                query = query.Where(e => ids.Contains(e.EventId));
            }
            if (arena == true)
            {
                var ids = db.MSSA_EventClassOfferings.Where(o => o.Venue == "Arena").Select(o => o.EventId);
                query = query.Where(e => ids.Contains(e.EventId));
            }
            if (field == true)
            {
                var ids = db.MSSA_EventClassOfferings.Where(o => o.Venue == "Field").Select(o => o.EventId);
                query = query.Where(e => ids.Contains(e.EventId));
            }
            if (onFoot == true)
            {
                var ids = from o in db.MSSA_EventClassOfferings
                          join c in db.MSSA_Classes on o.ClassId equals c.ClassId
                          where c.SubClassName == "On-foot"
                          select o.EventId;
                query = query.Where(e => ids.Contains(e.EventId));
            }
            if (horseback == true)
            {
                var ids = from o in db.MSSA_EventClassOfferings
                          join c in db.MSSA_Classes on o.ClassId equals c.ClassId
                          where c.SubClassName == "Horseback"
                          select o.EventId;
                query = query.Where(e => ids.Contains(e.EventId));
            }
            if (open == true) query = WhereOffersClass(db, query, "Open");
            if (nursery == true) query = WhereOffersClass(db, query, "Nursery");
            if (intermediate == true) query = WhereOffersClass(db, query, "Intermediate");
            if (novice == true) query = WhereOffersClass(db, query, "Novice");
            if (junior == true) query = WhereOffersClass(db, query, "JR Handler");

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

            var eventIds = events.Select(e => e.EventId).ToList();
            var offeringsByEvent = await LoadOfferingsForEventsAsync(db, eventIds);

            foreach (var evt in events)
            {
                if (!string.IsNullOrEmpty(evt.StateCode) && states.ContainsKey(evt.StateCode))
                {
                    evt.StateName = states[evt.StateCode];
                }
                evt.TrialCount = trialCounts.ContainsKey(evt.EventId) ? trialCounts[evt.EventId] : 0;
                evt.Offerings = offeringsByEvent.TryGetValue(evt.EventId, out var offerings) ? offerings : new List<MSSA_EventClassOffering>();
            }

            return events;
        }

        private static IQueryable<MSSA_Event> WhereOffersClass(MSSADbContext db, IQueryable<MSSA_Event> query, string className)
        {
            var ids = from o in db.MSSA_EventClassOfferings
                      join c in db.MSSA_Classes on o.ClassId equals c.ClassId
                      where c.ClassName == className
                      select o.EventId;
            return query.Where(e => ids.Contains(e.EventId));
        }

        // Offerings (planned runs per Class/Stock/Venue)
        public async Task<List<MSSA_EventClassOffering>> GetEventOfferingsAsync(int eventId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await LoadOfferingsAsync(db, eventId);
        }

        // Replaces the full set of offerings for an event in one operation - simpler
        // and safer than granular add/update/delete for a repeatable-rows form where
        // the whole list is edited together and saved with the parent Event.
        public async Task<List<MSSA_EventClassOffering>> SaveEventOfferingsAsync(int eventId, List<MSSA_EventClassOffering> offerings)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_EventClassOfferings.Where(o => o.EventId == eventId).ToListAsync();
            db.MSSA_EventClassOfferings.RemoveRange(existing);

            foreach (var offering in offerings)
            {
                offering.OfferingId = 0;
                offering.EventId = eventId;
                db.MSSA_EventClassOfferings.Add(offering);
            }

            await db.SaveChangesAsync();

            return await LoadOfferingsAsync(db, eventId);
        }

        private static async Task<List<MSSA_EventClassOffering>> LoadOfferingsAsync(MSSADbContext db, int eventId)
        {
            var offerings = await (from o in db.MSSA_EventClassOfferings
                                   join c in db.MSSA_Classes on o.ClassId equals c.ClassId
                                   where o.EventId == eventId
                                   orderby c.PrintOrder, o.Stock, o.Venue
                                   select new MSSA_EventClassOffering
                                   {
                                       OfferingId = o.OfferingId,
                                       EventId = o.EventId,
                                       ClassId = o.ClassId,
                                       Stock = o.Stock,
                                       Venue = o.Venue,
                                       PlannedRuns = o.PlannedRuns,
                                       ClassName = c.ClassName,
                                       SubClassName = c.SubClassName
                                   })
                                  .ToListAsync();

            return offerings;
        }

        private static async Task<Dictionary<int, List<MSSA_EventClassOffering>>> LoadOfferingsForEventsAsync(MSSADbContext db, List<int> eventIds)
        {
            var offerings = await (from o in db.MSSA_EventClassOfferings
                                   join c in db.MSSA_Classes on o.ClassId equals c.ClassId
                                   where eventIds.Contains(o.EventId)
                                   orderby c.PrintOrder, o.Stock, o.Venue
                                   select new MSSA_EventClassOffering
                                   {
                                       OfferingId = o.OfferingId,
                                       EventId = o.EventId,
                                       ClassId = o.ClassId,
                                       Stock = o.Stock,
                                       Venue = o.Venue,
                                       PlannedRuns = o.PlannedRuns,
                                       ClassName = c.ClassName,
                                       SubClassName = c.SubClassName
                                   })
                                  .ToListAsync();

            return offerings.GroupBy(o => o.EventId).ToDictionary(g => g.Key, g => g.ToList());
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
                                 join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                 join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                 where e.TrialId == trialId
                                 select new EntryListItem
                                 {
                                     EntryId = e.EntryId,
                                     TrialId = e.TrialId,
                                     DogId = e.DogId,
                                     HandlerId = e.HandlerId,
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
                                     TrialPoints = e.TrialPoints,
                                     Year = ev.PointYear ?? t.TrialDate.Year
                                 })
                                .ToListAsync();

            await ApplyFuturityMarkerAsync(db, entries);

            return entries;
        }

        // Appends "+" to DogName for any entry where the dog was enrolled in Futurity
        // for that entry's year - applies across all classes, not just Nursery (policy
        // change: previously gated to Nursery, now every entry by a nominated dog in
        // their nomination year gets marked).
        //
        // NOTE: Year here is sourced from MSSA_Events.PointYear (falling back to
        // TrialDate.Year), and PointYear is stored inconsistently - some rows 2-digit
        // (e.g. 24), others full 4-digit (e.g. 2024) - while
        // MSSA_DogFuturityParticipation.Year is always 4-digit. Normalize before comparing.
        private static async Task ApplyFuturityMarkerAsync(MSSADbContext db, List<EntryListItem> entries)
        {
            if (!entries.Any())
            {
                return;
            }

            var years = entries.Select(e => NormalizeYear(e.Year)).Distinct().ToList();
            var dogIds = entries.Select(e => e.DogId).Distinct().ToList();

            var futurityPairs = await db.MSSA_DogFuturityParticipation
                .Where(f => years.Contains(f.Year) && dogIds.Contains(f.DogId))
                .Select(f => new { f.DogId, f.Year })
                .ToListAsync();

            var futuritySet = new HashSet<(int DogId, int Year)>(futurityPairs.Select(f => (f.DogId, f.Year)));

            foreach (var entry in entries)
            {
                if (futuritySet.Contains((entry.DogId, NormalizeYear(entry.Year))))
                {
                    entry.DogName += "+";
                }
            }
        }

        // Converts a possibly-2-digit legacy year (e.g. 24) to full 4-digit form (2024).
        // Leaves already-4-digit years untouched.
        private static int NormalizeYear(int year) => year < 100 ? 2000 + year : year;
    }
}

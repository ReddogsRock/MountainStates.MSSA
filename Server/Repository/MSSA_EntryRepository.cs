using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Entries.Repository
{
    public class MSSA_EntryRepository : IMSSA_EntryRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_EntryRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<MSSA_Entry>> GetEntriesAsync(int moduleId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await (from e in db.MSSA_Entries
                                 join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                                 join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                 join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                 join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                 join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                 select new MSSA_Entry
                                 {
                                     EntryId = e.EntryId,
                                     TrialId = e.TrialId,
                                     HandlerId = e.HandlerId,
                                     DogId = e.DogId,
                                     ClassId = e.ClassId,
                                     RunOrder = e.RunOrder,
                                     Placing = e.Placing,
                                     RunTime = e.RunTime,
                                     TieBreakerTime = e.TieBreakerTime,
                                     ObstacleScore1 = e.ObstacleScore1,
                                     ObstacleScore2 = e.ObstacleScore2,
                                     ObstacleScore3 = e.ObstacleScore3,
                                     ObstacleScore4 = e.ObstacleScore4,
                                     ObstacleScore5 = e.ObstacleScore5,
                                     ObstacleScore6 = e.ObstacleScore6,
                                     ObstacleScore7 = e.ObstacleScore7,
                                     ObstacleScore8 = e.ObstacleScore8,
                                     ObstacleScore9 = e.ObstacleScore9,
                                     Penalty = e.Penalty,
                                     TrialPoints = e.TrialPoints,
                                     HandlerIsMSSAMember = e.HandlerIsMSSAMember,
                                     Comments = e.Comments,
                                     CreatedDate = e.CreatedDate,
                                     ModifiedDate = e.ModifiedDate,
                                     EnteredBy = e.EnteredBy,
                                     ModifiedBy = e.ModifiedBy,
                                     HandlerName = h.FullName,
                                     DogName = d.Name,
                                     ClassName = c.ClassName,
                                     SubClassName = c.SubClassName,
                                     Stock = t.Stock,
                                     EventName = ev.EventName,
                                     TrialDate = t.TrialDate,
                                     Year = ev.PointYear ?? t.TrialDate.Year,
                                     EventCreatedByUserId = ev.CreatedByUserId
                                 })
                                .OrderByDescending(e => e.TrialDate)
                                .ToListAsync();

            await ApplyFuturityMarkerAsync(db, entries);

            return entries;
        }

        public async Task<IEnumerable<EntryListItem>> GetTrialEntriesAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await (from e in db.MSSA_Entries
                                 join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                                 join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                 join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                 join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                 join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                 where e.TrialId == trialId
                                 orderby c.PrintOrder, e.RunOrder  // Move ordering here, before select
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
                                     TrialPoints = e.TrialPoints,
                                     RunTime = e.RunTime,
                                     TieBreakerTime = e.TieBreakerTime,
                                     ObstacleScore1 = e.ObstacleScore1,
                                     ObstacleScore2 = e.ObstacleScore2,
                                     ObstacleScore3 = e.ObstacleScore3,
                                     ObstacleScore4 = e.ObstacleScore4,
                                     ObstacleScore5 = e.ObstacleScore5,
                                     ObstacleScore6 = e.ObstacleScore6,
                                     ObstacleScore7 = e.ObstacleScore7,
                                     ObstacleScore8 = e.ObstacleScore8,
                                     ObstacleScore9 = e.ObstacleScore9,
                                     Penalty = e.Penalty,
                                     Comments = e.Comments,
                                     TotalScore = (e.ObstacleScore1 ?? 0) + (e.ObstacleScore2 ?? 0) +
                                                 (e.ObstacleScore3 ?? 0) + (e.ObstacleScore4 ?? 0) +
                                                 (e.ObstacleScore5 ?? 0) + (e.ObstacleScore6 ?? 0) +
                                                 (e.ObstacleScore7 ?? 0) + (e.ObstacleScore8 ?? 0) +
                                                 (e.ObstacleScore9 ?? 0) - (e.Penalty ?? 0),
                                     EventName = ev.EventName,
                                     TrialDate = t.TrialDate,
                                     Stock = t.Stock,
                                     Year = ev.PointYear ?? t.TrialDate.Year,
                                     EventCreatedByUserId = ev.CreatedByUserId
                                 })
                                .ToListAsync();

            await ApplyFuturityMarkerAsync(db, entries);

            return entries;
        }

        public async Task<MSSA_Entry> GetEntryAsync(int entryId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entry = await (from e in db.MSSA_Entries
                               join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                               join d in db.MSSA_Dogs on e.DogId equals d.DogId
                               join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                               join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                               join ev in db.MSSA_Events on t.EventId equals ev.EventId
                               where e.EntryId == entryId
                               select new MSSA_Entry
                               {
                                   EntryId = e.EntryId,
                                   TrialId = e.TrialId,
                                   HandlerId = e.HandlerId,
                                   DogId = e.DogId,
                                   ClassId = e.ClassId,
                                   RunOrder = e.RunOrder,
                                   Placing = e.Placing,
                                   RunTime = e.RunTime,
                                   TieBreakerTime = e.TieBreakerTime,
                                   ObstacleScore1 = e.ObstacleScore1,
                                   ObstacleScore2 = e.ObstacleScore2,
                                   ObstacleScore3 = e.ObstacleScore3,
                                   ObstacleScore4 = e.ObstacleScore4,
                                   ObstacleScore5 = e.ObstacleScore5,
                                   ObstacleScore6 = e.ObstacleScore6,
                                   ObstacleScore7 = e.ObstacleScore7,
                                   ObstacleScore8 = e.ObstacleScore8,
                                   ObstacleScore9 = e.ObstacleScore9,
                                   Penalty = e.Penalty,
                                   TrialPoints = e.TrialPoints,
                                   HandlerIsMSSAMember = e.HandlerIsMSSAMember,
                                   Comments = e.Comments,
                                   CreatedDate = e.CreatedDate,
                                   ModifiedDate = e.ModifiedDate,
                                   EnteredBy = e.EnteredBy,
                                   ModifiedBy = e.ModifiedBy,
                                   HandlerName = h.FullName,
                                   DogName = d.Name,
                                   ClassName = c.ClassName,
                                   SubClassName = c.SubClassName,
                                   Stock = t.Stock,
                                   EventName = ev.EventName,
                                   TrialDate = t.TrialDate,
                                   Year = ev.PointYear ?? t.TrialDate.Year,
                                   EventCreatedByUserId = ev.CreatedByUserId
                               })
                              .FirstOrDefaultAsync();

            if (entry != null)
            {
                await ApplyFuturityMarkerAsync(db, new List<MSSA_Entry> { entry });
            }

            return entry;
        }

        public async Task<MSSA_Entry> AddEntryAsync(MSSA_Entry entry)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            entry.CreatedDate = DateTime.UtcNow;
            entry.ModifiedDate = DateTime.UtcNow;

            db.MSSA_Entries.Add(entry);
            await db.SaveChangesAsync();

            return entry;
        }

        public async Task<MSSA_Entry> UpdateEntryAsync(MSSA_Entry entry)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            entry.ModifiedDate = DateTime.UtcNow;

            db.Entry(entry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return entry;
        }

        public async Task DeleteEntryAsync(int entryId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entry = await db.MSSA_Entries.FindAsync(entryId);
            if (entry != null)
            {
                db.MSSA_Entries.Remove(entry);
                await db.SaveChangesAsync();
            }
        }

        // Resolves the owner of the Event a Trial belongs to, for authorizing entry
        // creation before an Entry row exists to carry EventCreatedByUserId itself.
        public async Task<int?> GetEventOwnerForTrialAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await (from t in db.MSSA_Trials
                          join ev in db.MSSA_Events on t.EventId equals ev.EventId
                          where t.TrialId == trialId
                          select ev.CreatedByUserId)
                         .FirstOrDefaultAsync();
        }

        // Fixed class run order: Open runs first, then Nursery, Intermediate, Novice,
        // Junior - dogs are shuffled randomly within a class, but classes themselves
        // always run in this sequence. Anything not in this list (legacy/unusual
        // classes) is appended after, alphabetically, so nothing silently vanishes.
        private static readonly string[] ClassRunOrder = { "Open", "Nursery", "Intermediate", "Novice", "Junior" };

        // Builds a proposed run order for every entry in the trial (all classes at
        // once), NOT persisted - the caller reviews/edits this list and calls
        // SaveRunOrderAsync to actually commit it. Re-running this discards any
        // previous proposal; it always starts fresh from every entry in the trial,
        // including ones that already have a RunOrder from an earlier generation.
        public async Task<List<RunOrderEntry>> GetProposedRunOrderAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await (from e in db.MSSA_Entries
                                 join h in db.MSSA_Handlers on e.HandlerId equals h.HandlerId
                                 join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                 join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                 where e.TrialId == trialId
                                 select new RunOrderEntry
                                 {
                                     EntryId = e.EntryId,
                                     TrialId = e.TrialId,
                                     ClassId = e.ClassId,
                                     ClassName = c.ClassName,
                                     SubClassName = c.SubClassName,
                                     DogName = d.Name,
                                     HandlerName = h.FullName
                                 })
                                .ToListAsync();

            var random = new Random();
            var ordered = new List<RunOrderEntry>();

            var classNames = entries.Select(e => e.ClassName).Distinct().ToList();
            var orderedClassNames = ClassRunOrder
                .Where(n => classNames.Contains(n))
                .Concat(classNames.Where(n => !ClassRunOrder.Contains(n)).OrderBy(n => n));

            int runOrder = 1;
            foreach (var className in orderedClassNames)
            {
                var shuffled = entries
                    .Where(e => e.ClassName == className)
                    .OrderBy(x => random.Next())
                    .ToList();

                foreach (var entry in shuffled)
                {
                    entry.RunOrder = runOrder++;
                    ordered.Add(entry);
                }
            }

            return ordered;
        }

        // Persists a (possibly user-edited) run order proposal. Only EntryId and
        // RunOrder are used - the rest of RunOrderEntry is display-only. Returns the
        // same list back (matches ServiceBase.PostJsonAsync<T>'s same-type-in-and-out
        // constraint, same pattern as SaveEventOfferingsAsync).
        public async Task<List<RunOrderEntry>> SaveRunOrderAsync(List<RunOrderEntry> assignments)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entryIds = assignments.Select(a => a.EntryId).ToList();
            var entries = await db.MSSA_Entries
                .Where(e => entryIds.Contains(e.EntryId))
                .ToDictionaryAsync(e => e.EntryId);

            foreach (var assignment in assignments)
            {
                if (entries.TryGetValue(assignment.EntryId, out var entry))
                {
                    entry.RunOrder = assignment.RunOrder;
                    entry.ModifiedDate = DateTime.UtcNow;
                }
            }

            await db.SaveChangesAsync();

            return assignments;
        }

        // Applies re-uploaded scores to the matching entries by EntryId. Rows whose
        // EntryId doesn't match any entry are silently skipped (e.g. a row someone
        // added by hand, or an entry that was deleted since export) rather than failing
        // the whole import. Returns only the rows that were actually matched/updated
        // (matches ServiceBase.PostJsonAsync<T>'s same-type-in-and-out constraint) - the
        // caller compares this list's count against what it submitted to see if
        // anything was skipped.
        public async Task<List<ScoreImportRow>> ImportScoresAsync(List<ScoreImportRow> rows)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entryIds = rows.Select(r => r.EntryId).ToList();
            var entries = await db.MSSA_Entries
                .Where(e => entryIds.Contains(e.EntryId))
                .ToDictionaryAsync(e => e.EntryId);

            var updatedRows = new List<ScoreImportRow>();
            foreach (var row in rows)
            {
                if (!entries.TryGetValue(row.EntryId, out var entry))
                {
                    continue;
                }

                entry.ObstacleScore1 = row.ObstacleScore1;
                entry.ObstacleScore2 = row.ObstacleScore2;
                entry.ObstacleScore3 = row.ObstacleScore3;
                entry.ObstacleScore4 = row.ObstacleScore4;
                entry.ObstacleScore5 = row.ObstacleScore5;
                entry.ObstacleScore6 = row.ObstacleScore6;
                entry.ObstacleScore7 = row.ObstacleScore7;
                entry.ObstacleScore8 = row.ObstacleScore8;
                entry.ObstacleScore9 = row.ObstacleScore9;
                entry.Penalty = row.Penalty;
                entry.Placing = row.Placing;
                entry.TrialPoints = row.TrialPoints;
                entry.Comments = row.Comments;

                var runTime = ParseMinutesSeconds(row.RunTime);
                if (runTime.HasValue)
                {
                    entry.RunTime = runTime.Value;
                }

                var tieBreakerTime = ParseMinutesSeconds(row.TieBreakerTime);
                if (tieBreakerTime.HasValue)
                {
                    entry.TieBreakerTime = tieBreakerTime.Value;
                }

                entry.ModifiedDate = DateTime.UtcNow;
                updatedRows.Add(row);
            }

            await db.SaveChangesAsync();
            return updatedRows;
        }

        // Parses run/tie-breaker times. Accepts ':' or '.' as the minutes/seconds
        // separator - both mean the same thing (MM:SS / MM.SS), '.' is just much
        // easier to type on a numeric keypad while entering scores at the trial than
        // switching to type a colon. A value with no separator at all is treated as a
        // bare number of minutes (e.g. "10" -> 10:00).
        //
        // Examples: "10:35" -> 10:35, "10.35" -> 10:35, ".35" -> 0:35, "10" -> 10:00.
        private static TimeSpan? ParseMinutesSeconds(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            char separator = trimmed.Contains(':') ? ':' : (trimmed.Contains('.') ? '.' : '\0');

            if (separator != '\0')
            {
                var parts = trimmed.Split(separator);
                if (parts.Length == 2)
                {
                    var minutesPart = string.IsNullOrEmpty(parts[0]) ? "0" : parts[0];
                    if (int.TryParse(minutesPart, out var minutes) && int.TryParse(parts[1], out var seconds))
                    {
                        return new TimeSpan(0, 0, minutes, seconds);
                    }
                }
                return null;
            }

            // No separator at all - bare number of minutes.
            if (int.TryParse(trimmed, out var minutesOnly))
            {
                return TimeSpan.FromMinutes(minutesOnly);
            }

            return null;
        }

        // Classes
        public async Task<IEnumerable<MSSA_Class>> GetClassesAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.PrintOrder)
                .ToListAsync();
        }

        public async Task<MSSA_Class> GetClassAsync(int classId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Classes
                .FirstOrDefaultAsync(c => c.ClassId == classId);
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
        private static async Task ApplyFuturityMarkerAsync(MSSADbContext db, List<MSSA_Entry> entries)
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

        // Same as above, for the trial-scoped list projection (EntryListItem).
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
using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Repository
{
    public class BackOfficeEntryRepository : IBackOfficeEntryRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public BackOfficeEntryRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // ─────────────────────────────────────────────────────
        //  Events  (60-day window for back-office scenario)
        // ─────────────────────────────────────────────────────

        public async Task<List<RecentEventDto>> GetRecentEventsWithTrialsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var cutoffDate = DateTime.Today.AddDays(-60);

            var events = await db.MSSA_Events
                .Where(e => e.IsActive && e.StartDate >= cutoffDate)
                .OrderByDescending(e => e.StartDate)
                .Select(e => new RecentEventDto
                {
                    EventId = e.EventId,
                    EventIdentifier = e.EventIdentifier,
                    EventName = e.EventName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    City = e.City,
                    StateCode = e.StateCode
                })
                .ToListAsync();

            var eventIds = events.Select(e => e.EventId).ToList();

            var trials = await db.MSSA_Trials
                .Where(t => eventIds.Contains(t.EventId))
                .OrderBy(t => t.TrialDate)
                .ToListAsync();

            var allClasses = await db.MSSA_Classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.PrintOrder ?? int.MaxValue)
                .ToListAsync();

            foreach (var evt in events)
            {
                var fullEvent = await db.MSSA_Events.FirstOrDefaultAsync(e => e.EventId == evt.EventId);

                var filteredClasses = allClasses
                    .Where(c => IsClassAllowedForEvent(c, fullEvent))
                    .Select(c => new ClassOptionDto
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName,
                        SubClassName = c.SubClassName
                    })
                    .ToList();

                evt.Trials = trials
                    .Where(t => t.EventId == evt.EventId)
                    .Select(t => new TrialSummaryDto
                    {
                        TrialId = t.TrialId,
                        TrialIdentifier = t.TrialIdentifier,
                        TrialName = t.TrialName,
                        TrialDate = t.TrialDate,
                        Stock = t.Stock,
                        AvailableClasses = filteredClasses
                    })
                    .ToList();
            }

            return events;
        }

        private bool IsClassAllowedForEvent(MSSA_Class classItem, MSSA_Event evt)
        {
            if (evt == null) return true;
            var fullName = $"{classItem.ClassName} {classItem.SubClassName}".ToLower();

            bool hasHorseback = fullName.Contains("horseback") || fullName.Contains("horse back");
            bool hasOnFoot    = fullName.Contains("on foot")   || fullName.Contains("on-foot") || fullName.Contains("onfoot");

            if (hasHorseback && !evt.Horseback) return false;
            if (hasOnFoot    && !evt.OnFoot)    return false;
            if (fullName.Contains("open")         && !evt.Open)         return false;
            if (fullName.Contains("nursery")      && !evt.Nursery)      return false;
            if (fullName.Contains("intermediate") && !evt.Intermediate) return false;
            if (fullName.Contains("novice")       && !evt.Novice)       return false;
            if (fullName.Contains("junior")       && !evt.Junior)       return false;
            if (fullName.Contains("arena")        && !evt.Arena)        return false;
            if (fullName.Contains("field")        && !evt.Field)        return false;

            return true;
        }

        // ─────────────────────────────────────────────────────
        //  Handlers  (identical to TrialSecretary)
        // ─────────────────────────────────────────────────────

        public async Task<List<HandlerSearchDto>> SearchHandlersAsync(string searchTerm)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Handlers.Where(h => h.IsActive);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(h =>
                    h.FirstName.ToLower().Contains(term) ||
                    h.LastName.ToLower().Contains(term)  ||
                    h.FullName.ToLower().Contains(term));
            }

            var handlers = await query
                .OrderBy(h => h.LastName).ThenBy(h => h.FirstName)
                .Take(50).ToListAsync();

            var handlerIds = handlers.Select(h => h.HandlerId).ToList();
            var currentYear = DateTime.Today.Year;
            var activeMembers = await db.MSSA_HandlerMemberships
                .Where(m => handlerIds.Contains(m.HandlerId) &&
                            m.StartYear <= currentYear && m.EndYear >= currentYear &&
                            m.IsActive && m.DateReceived.HasValue)
                .Select(m => m.HandlerId)
                .ToListAsync();

            return handlers.Select(h => new HandlerSearchDto
            {
                HandlerId = h.HandlerId, FullName = h.FullName,
                City = h.City, StateCode = h.StateCode,
                Email = h.Email, Phone = h.Phone,
                HasActiveMembership = activeMembers.Contains(h.HandlerId)
            }).ToList();
        }

        public async Task<HandlerSearchDto> GetHandlerByIdAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var h = await db.MSSA_Handlers.FirstOrDefaultAsync(x => x.HandlerId == handlerId && x.IsActive);
            if (h == null) return null;

            var currentYear = DateTime.Today.Year;
            var isMember = await db.MSSA_HandlerMemberships
                .AnyAsync(m => m.HandlerId == handlerId &&
                               m.StartYear <= currentYear && m.EndYear >= currentYear &&
                               m.IsActive && m.DateReceived.HasValue);

            return new HandlerSearchDto
            {
                HandlerId = h.HandlerId, FullName = h.FullName,
                City = h.City, StateCode = h.StateCode,
                Email = h.Email, Phone = h.Phone,
                HasActiveMembership = isMember
            };
        }

        public async Task<MSSA_Handler> CreateHandlerAsync(CreateHandlerDto dto)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var h = new MSSA_Handler
            {
                FirstName = dto.FirstName, LastName = dto.LastName,
                Email = dto.Email, Phone = dto.Phone,
                City = dto.City, StateCode = dto.StateCode,
                CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow,
                IsActive = true
            };
            db.MSSA_Handlers.Add(h);
            await db.SaveChangesAsync();
            return h;
        }

        // ─────────────────────────────────────────────────────
        //  Dogs  (identical to TrialSecretary)
        // ─────────────────────────────────────────────────────

        public async Task<List<DogSearchDto>> SearchDogsAsync(string searchTerm)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var query = db.MSSA_Dogs.Where(d => d.IsActive && !d.IsDeceased);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(term));
            }
            return await query.OrderBy(d => d.Name).Take(50)
                .Select(d => new DogSearchDto
                {
                    DogId = d.DogId, Name = d.Name, Breed = d.Breed, OwnerName = d.OwnerName,
                    Age = d.DateOfBirth.HasValue ? DateTime.Today.Year - d.DateOfBirth.Value.Year : (int?)null
                }).ToListAsync();
        }

        public async Task<DogSearchDto> GetDogByIdAsync(int dogId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var d = await db.MSSA_Dogs.FirstOrDefaultAsync(x => x.DogId == dogId && x.IsActive && !x.IsDeceased);
            if (d == null) return null;
            return new DogSearchDto
            {
                DogId = d.DogId, Name = d.Name, Breed = d.Breed, OwnerName = d.OwnerName,
                Age = d.DateOfBirth.HasValue ? DateTime.Today.Year - d.DateOfBirth.Value.Year : (int?)null
            };
        }

        public async Task<MSSA_Dog> CreateDogAsync(CreateDogDto dto)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var d = new MSSA_Dog
            {
                Name = dto.Name, Breed = dto.Breed, OwnerName = dto.OwnerName,
                CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow,
                IsActive = true, IsDeceased = false, IsSold = false
            };
            db.MSSA_Dogs.Add(d);
            await db.SaveChangesAsync();
            return d;
        }

        // ─────────────────────────────────────────────────────
        //  Result Entries
        // ─────────────────────────────────────────────────────

        public async Task<int> CreateResultEntryAsync(SaveResultEntryDto dto, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var currentYear = DateTime.Today.Year;
            var isMember = await db.MSSA_HandlerMemberships
                .AnyAsync(m => m.HandlerId == dto.HandlerId &&
                               m.StartYear <= currentYear && m.EndYear >= currentYear &&
                               m.IsActive && m.DateReceived.HasValue);

            var entry = new MSSA_Entry
            {
                TrialId  = dto.TrialId,
                HandlerId = dto.HandlerId,
                DogId    = dto.DogId,
                ClassId  = dto.ClassId,
                HandlerIsMSSAMember = isMember,

                RunTime         = ParseTimeString(dto.RunTimeStr),
                TieBreakerTime  = ParseTimeString(dto.TieBreakerTimeStr),
                ObstacleScore1  = dto.ObstacleScore1,
                ObstacleScore2  = dto.ObstacleScore2,
                ObstacleScore3  = dto.ObstacleScore3,
                ObstacleScore4  = dto.ObstacleScore4,
                ObstacleScore5  = dto.ObstacleScore5,
                ObstacleScore6  = dto.ObstacleScore6,
                ObstacleScore7  = dto.ObstacleScore7,
                ObstacleScore8  = dto.ObstacleScore8,
                ObstacleScore9  = dto.ObstacleScore9,
                Penalty         = dto.Penalty,
                Comments        = dto.Comments,

                CreatedDate  = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                EnteredBy    = userId,
                ModifiedBy   = userId
            };

            db.MSSA_Entries.Add(entry);
            await db.SaveChangesAsync();
            return entry.EntryId;
        }

        public async Task UpdateResultEntryAsync(SaveResultEntryDto dto, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entry = await db.MSSA_Entries.FindAsync(dto.EntryId);
            if (entry == null)
                throw new InvalidOperationException($"Entry {dto.EntryId} not found.");

            entry.RunTime        = ParseTimeString(dto.RunTimeStr);
            entry.TieBreakerTime = ParseTimeString(dto.TieBreakerTimeStr);
            entry.ObstacleScore1 = dto.ObstacleScore1;
            entry.ObstacleScore2 = dto.ObstacleScore2;
            entry.ObstacleScore3 = dto.ObstacleScore3;
            entry.ObstacleScore4 = dto.ObstacleScore4;
            entry.ObstacleScore5 = dto.ObstacleScore5;
            entry.ObstacleScore6 = dto.ObstacleScore6;
            entry.ObstacleScore7 = dto.ObstacleScore7;
            entry.ObstacleScore8 = dto.ObstacleScore8;
            entry.ObstacleScore9 = dto.ObstacleScore9;
            entry.Penalty        = dto.Penalty;
            entry.Comments       = dto.Comments;
            entry.ModifiedDate   = DateTime.UtcNow;
            entry.ModifiedBy     = userId;

            await db.SaveChangesAsync();
        }

        public async Task<SaveResultEntryDto> GetResultEntryAsync(int entryId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
            var e = await db.MSSA_Entries.FindAsync(entryId);
            if (e == null) return null;

            return new SaveResultEntryDto
            {
                EntryId         = e.EntryId,
                TrialId         = e.TrialId,
                ClassId         = e.ClassId,
                HandlerId       = e.HandlerId,
                DogId           = e.DogId,
                RunTimeStr         = FormatTimeSpan(e.RunTime),
                TieBreakerTimeStr  = FormatTimeSpan(e.TieBreakerTime),
                ObstacleScore1  = e.ObstacleScore1,
                ObstacleScore2  = e.ObstacleScore2,
                ObstacleScore3  = e.ObstacleScore3,
                ObstacleScore4  = e.ObstacleScore4,
                ObstacleScore5  = e.ObstacleScore5,
                ObstacleScore6  = e.ObstacleScore6,
                ObstacleScore7  = e.ObstacleScore7,
                ObstacleScore8  = e.ObstacleScore8,
                ObstacleScore9  = e.ObstacleScore9,
                Penalty         = e.Penalty,
                Comments        = e.Comments
            };
        }

        public async Task<List<ResultEntryListItem>> GetTrialClassEntriesAsync(int trialId, int classId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await db.MSSA_Entries
                .Where(e => e.TrialId == trialId && e.ClassId == classId)
                .ToListAsync();

            if (!entries.Any()) return new List<ResultEntryListItem>();

            var handlerIds = entries.Select(e => e.HandlerId).Distinct().ToList();
            var handlers   = await db.MSSA_Handlers
                .Where(h => handlerIds.Contains(h.HandlerId))
                .ToDictionaryAsync(h => h.HandlerId, h => h.FullName);

            var dogIds = entries.Select(e => e.DogId).Distinct().ToList();
            var dogs   = await db.MSSA_Dogs
                .Where(d => dogIds.Contains(d.DogId))
                .ToDictionaryAsync(d => d.DogId, d => d.Name);

            // Futurity "+" marker: only applies to Nursery-class scores, and only for
            // dogs enrolled in Futurity for the competition year this trial falls in.
            //
            // NOTE: MSSA_Events.PointYear is stored inconsistently - some rows 2-digit
            // (e.g. 24), others full 4-digit (e.g. 2024) - while
            // MSSA_DogFuturityParticipation.Year is always 4-digit. Normalize before comparing.
            var classInfo = await db.MSSA_Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
            var isNursery = string.Equals(classInfo?.ClassName, "Nursery", StringComparison.OrdinalIgnoreCase);

            var futurityDogIds = new HashSet<int>();
            if (isNursery)
            {
                var trial = await db.MSSA_Trials.FirstOrDefaultAsync(t => t.TrialId == trialId);
                var evt = trial != null
                    ? await db.MSSA_Events.FirstOrDefaultAsync(e => e.EventId == trial.EventId)
                    : null;
                var year = evt?.PointYear ?? trial?.TrialDate.Year ?? DateTime.Today.Year;
                year = NormalizeYear(year);

                var enrolled = await db.MSSA_DogFuturityParticipation
                    .Where(f => f.Year == year && dogIds.Contains(f.DogId))
                    .Select(f => f.DogId)
                    .ToListAsync();

                futurityDogIds = enrolled.ToHashSet();
            }

            return entries.Select(e => new ResultEntryListItem
            {
                EntryId              = e.EntryId,
                HandlerName          = handlers.TryGetValue(e.HandlerId, out var hn) ? hn : "Unknown",
                DogName              = (dogs.TryGetValue(e.DogId, out var dn) ? dn : "Unknown")
                                       + (futurityDogIds.Contains(e.DogId) ? "+" : ""),
                HandlerIsMSSAMember  = e.HandlerIsMSSAMember,
                RunTimeDisplay       = FormatTimeSpanDisplay(e.RunTime),
                TieBreakerTimeDisplay = FormatTimeSpanDisplay(e.TieBreakerTime),
                ObstacleScore1       = e.ObstacleScore1,
                ObstacleScore2       = e.ObstacleScore2,
                ObstacleScore3       = e.ObstacleScore3,
                ObstacleScore4       = e.ObstacleScore4,
                ObstacleScore5       = e.ObstacleScore5,
                ObstacleScore6       = e.ObstacleScore6,
                ObstacleScore7       = e.ObstacleScore7,
                ObstacleScore8       = e.ObstacleScore8,
                ObstacleScore9       = e.ObstacleScore9,
                Penalty              = e.Penalty,
                Comments             = e.Comments,
                Placing              = e.Placing,
                TrialPoints          = e.TrialPoints
            }).ToList();
        }

        // ─────────────────────────────────────────────────────
        //  Finalize — Calculate Placing and TrialPoints
        // ─────────────────────────────────────────────────────

        public async Task<FinalizeResultDto> FinalizeTrialClassAsync(int trialId, int classId, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await db.MSSA_Entries
                .Where(e => e.TrialId == trialId && e.ClassId == classId)
                .ToListAsync();

            if (!entries.Any())
                return new FinalizeResultDto { EntriesFinalized = 0, Message = "No entries found to finalize." };

            // ── Sort for placing: TotalScore DESC, RunTime ASC, TieBreakerTime ASC ──
            // TotalScore is computed here the same way MSSA_Entry.TotalScore does it
            var scored = entries.Select(e => new
            {
                Entry = e,
                Total = (e.ObstacleScore1 ?? 0) + (e.ObstacleScore2 ?? 0) + (e.ObstacleScore3 ?? 0)
                      + (e.ObstacleScore4 ?? 0) + (e.ObstacleScore5 ?? 0) + (e.ObstacleScore6 ?? 0)
                      + (e.ObstacleScore7 ?? 0) + (e.ObstacleScore8 ?? 0) + (e.ObstacleScore9 ?? 0)
                      - (e.Penalty ?? 0)
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Entry.RunTime ?? TimeSpan.MaxValue)
            .ThenBy(x => x.Entry.TieBreakerTime ?? TimeSpan.MaxValue)
            .ToList();

            int totalEntries = scored.Count;

            // Base points for 1st place = number of entries * 30
            // Each subsequent place decreases by 100, minimum 0
            // Non-MSSA members always receive 0 trial points
            int basePoints = totalEntries * 30;

            for (int i = 0; i < scored.Count; i++)
            {
                var row   = scored[i];
                int place = i + 1;
                row.Entry.Placing = place;

                if (row.Entry.HandlerIsMSSAMember)
                {
                    int points = basePoints - ((place - 1) * 100);
                    row.Entry.TrialPoints = Math.Max(points, 0);
                }
                else
                {
                    row.Entry.TrialPoints = 0;
                }

                row.Entry.ModifiedDate = DateTime.UtcNow;
                row.Entry.ModifiedBy   = userId;
            }

            await db.SaveChangesAsync();

            return new FinalizeResultDto
            {
                EntriesFinalized = totalEntries,
                Message = $"Finalized {totalEntries} entries. Placings and trial points have been calculated."
            };
        }
        /// <summary>
        /// Parses digit-only time strings the same way as the Excel module.
        /// "345" → 3:45, "1234" → 12:34, "12" → 0:12
        /// Also accepts "M:SS" or "MM:SS" colon format for flexibility.
        /// Returns null if empty or unparseable.
        /// </summary>
        private static TimeSpan? ParseTimeString(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();

            // Extract digits only (matches Excel module approach)
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits)) return null;

            try
            {
                if (digits.Length <= 2)
                {
                    // Just seconds: "45" → 0:45
                    return new TimeSpan(0, 0, int.Parse(digits));
                }
                else if (digits.Length == 3)
                {
                    // M:SS: "345" → 3:45
                    return new TimeSpan(0, int.Parse(digits.Substring(0, 1)), int.Parse(digits.Substring(1, 2)));
                }
                else if (digits.Length == 4)
                {
                    // MM:SS: "1234" → 12:34
                    return new TimeSpan(0, int.Parse(digits.Substring(0, 2)), int.Parse(digits.Substring(2, 2)));
                }
                else
                {
                    // MMM:SS or longer
                    return new TimeSpan(0, int.Parse(digits.Substring(0, digits.Length - 2)), int.Parse(digits.Substring(digits.Length - 2, 2)));
                }
            }
            catch { return null; }
        }

        /// <summary>Formats for round-trip back into the edit form as digit string (e.g. "345" for 3:45).</summary>
        private static string FormatTimeSpan(TimeSpan? ts)
        {
            if (!ts.HasValue) return "";
            int totalMinutes = (int)ts.Value.TotalMinutes;
            int seconds = ts.Value.Seconds;
            return $"{totalMinutes}{seconds:D2}";
        }

        /// <summary>Formats for display in the results grid as "M:SS" or "-".</summary>
        private static string FormatTimeSpanDisplay(TimeSpan? ts)
        {
            if (!ts.HasValue) return "-";
            int totalMinutes = (int)ts.Value.TotalMinutes;
            int seconds = ts.Value.Seconds;
            return $"{totalMinutes}:{seconds:D2}";
        }

        // Converts a possibly-2-digit legacy year (e.g. 24) to full 4-digit form (2024).
        // Leaves already-4-digit years untouched. MSSA_Events.PointYear is stored
        // inconsistently across the two conventions.
        private static int NormalizeYear(int year) => year < 100 ? 2000 + year : year;
    }
}

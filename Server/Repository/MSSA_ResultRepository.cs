using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_Results.Enums;
using MountainStates.MSSA.Module.MSSA_Results.Models;
using MountainStates.MSSA.Module.MSSA_Results.Utilities;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Results.Repository
{
    public class MSSA_ResultRepository : IMSSA_ResultRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_ResultRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<EventScoringSummary>> GetScoringEventsAsync(int? ownerUserId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Events.Where(e => e.IsActive && e.ResultsApprovalStatus != EventResultsStatus.Approved);
            if (ownerUserId.HasValue)
            {
                query = query.Where(e => e.CreatedByUserId == ownerUserId.Value);
            }

            var events = await query.OrderByDescending(e => e.StartDate).ToListAsync();
            return await BuildScoringSummariesAsync(db, events);
        }

        public async Task<List<EventScoringSummary>> GetPendingApprovalEventsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var events = await db.MSSA_Events
                .Where(e => e.IsActive && e.ResultsApprovalStatus == EventResultsStatus.PendingApproval)
                .OrderBy(e => e.ResultsSubmittedDate)
                .ToListAsync();

            return await BuildScoringSummariesAsync(db, events);
        }

        private static async Task<List<EventScoringSummary>> BuildScoringSummariesAsync(MSSADbContext db, List<MSSA_Event> events)
        {
            var summaries = new List<EventScoringSummary>();
            if (!events.Any())
            {
                return summaries;
            }

            var eventIds = events.Select(e => e.EventId).ToList();

            var trialsByEvent = await db.MSSA_Trials
                .Where(t => eventIds.Contains(t.EventId))
                .Select(t => new { t.EventId, t.TrialId })
                .ToListAsync();

            var allTrialIds = trialsByEvent.Select(t => t.TrialId).ToList();

            var entries = await db.MSSA_Entries
                .Where(e => allTrialIds.Contains(e.TrialId))
                .ToListAsync();

            var entriesByTrial = entries.ToLookup(e => e.TrialId);

            foreach (var evt in events)
            {
                var trialIds = trialsByEvent.Where(t => t.EventId == evt.EventId).Select(t => t.TrialId).ToList();
                var eventEntries = trialIds.SelectMany(tid => entriesByTrial[tid]).ToList();

                summaries.Add(new EventScoringSummary
                {
                    EventId = evt.EventId,
                    EventName = evt.EventName,
                    DateRange = evt.DateRange,
                    ResultsApprovalStatus = evt.ResultsApprovalStatus,
                    TotalRuns = eventEntries.Count,
                    ScoredRuns = eventEntries.Count(IsScored),
                    ResultsSubmittedDate = evt.ResultsSubmittedDate,
                    ResultsSubmittedByUserId = evt.ResultsSubmittedByUserId,
                    ResultsApprovedDate = evt.ResultsApprovedDate
                });
            }

            return summaries;
        }

        public async Task<int?> GetEventOwnerForTrialAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await (from t in db.MSSA_Trials
                          join ev in db.MSSA_Events on t.EventId equals ev.EventId
                          where t.TrialId == trialId
                          select ev.CreatedByUserId)
                         .FirstOrDefaultAsync();
        }

        public async Task<int?> GetEventOwnerAsync(int eventId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Events
                .Where(e => e.EventId == eventId)
                .Select(e => e.CreatedByUserId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ResultRunRow>> GetTrialRunRowsAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await db.MSSA_Entries.Where(e => e.TrialId == trialId).ToListAsync();
            if (!entries.Any())
            {
                return new List<ResultRunRow>();
            }

            var handlerIds = entries.Select(e => e.HandlerId).Distinct().ToList();
            var handlers = await db.MSSA_Handlers
                .Where(h => handlerIds.Contains(h.HandlerId))
                .ToDictionaryAsync(h => h.HandlerId, h => h.FullName);

            var dogIds = entries.Select(e => e.DogId).Distinct().ToList();
            var dogs = await db.MSSA_Dogs
                .Where(d => dogIds.Contains(d.DogId))
                .ToDictionaryAsync(d => d.DogId, d => d.Name);

            var classIds = entries.Select(e => e.ClassId).Distinct().ToList();
            var classes = await db.MSSA_Classes
                .Where(c => classIds.Contains(c.ClassId))
                .ToDictionaryAsync(c => c.ClassId);

            return entries
                .OrderBy(e => e.RunOrder ?? int.MaxValue)
                .ThenBy(e => classes.TryGetValue(e.ClassId, out var ci) ? ci.PrintOrder ?? int.MaxValue : int.MaxValue)
                .ThenBy(e => handlers.TryGetValue(e.HandlerId, out var hn) ? hn : "")
                .Select(e => new ResultRunRow
                {
                    EntryId = e.EntryId,
                    TrialId = e.TrialId,
                    RunOrder = e.RunOrder,
                    ClassName = classes.TryGetValue(e.ClassId, out var c) ? c.ClassName : "",
                    SubClassName = classes.TryGetValue(e.ClassId, out var c2) ? c2.SubClassName : "",
                    HandlerName = handlers.TryGetValue(e.HandlerId, out var handlerName) ? handlerName : "Unknown",
                    DogName = dogs.TryGetValue(e.DogId, out var dogName) ? dogName : "Unknown",
                    RunTimeStr = TimeParsingHelper.Format(e.RunTime),
                    TieBreakerTimeStr = TimeParsingHelper.Format(e.TieBreakerTime),
                    TotalScore = EffectiveScore(e),
                    Placing = e.Placing,
                    TrialPoints = e.TrialPoints
                })
                .ToList();
        }

        public async Task SaveResultRowAsync(SaveResultRowDto dto, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entry = await db.MSSA_Entries.FindAsync(dto.EntryId);
            if (entry == null)
            {
                throw new InvalidOperationException($"Entry {dto.EntryId} not found.");
            }

            entry.RunTime = TimeParsingHelper.ParseMinutesSeconds(dto.RunTimeStr);
            entry.TieBreakerTime = TimeParsingHelper.ParseMinutesSeconds(dto.TieBreakerTimeStr);
            entry.EnteredTotalScore = dto.TotalScore;
            entry.ModifiedDate = DateTime.UtcNow;
            entry.ModifiedBy = userId;

            await db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────
        //  Placing & Points - tie-aware
        // ─────────────────────────────────────────────────────

        public async Task CalculatePlacingAndPointsAsync(int trialId, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await db.MSSA_Entries.Where(e => e.TrialId == trialId).ToListAsync();
            if (!entries.Any())
            {
                return;
            }

            foreach (var classGroup in entries.GroupBy(e => e.ClassId))
            {
                // Rank: score desc, then run time asc, then tie-breaker time asc. Missing
                // values sort last within their class.
                var ranked = classGroup
                    .OrderByDescending(e => EffectiveScore(e) ?? decimal.MinValue)
                    .ThenBy(e => e.RunTime ?? TimeSpan.MaxValue)
                    .ThenBy(e => e.TieBreakerTime ?? TimeSpan.MaxValue)
                    .ToList();

                int totalEntries = ranked.Count;
                int basePoints = totalEntries * 30;

                int rank = 1;
                int i = 0;
                while (i < ranked.Count)
                {
                    // Extend the group while consecutive entries are a genuine tie (equal
                    // score, run time, and tie-breaker time - not just both blank).
                    int j = i;
                    while (j + 1 < ranked.Count && IsRealTie(ranked[j], ranked[j + 1]))
                    {
                        j++;
                    }
                    int groupSize = j - i + 1;
                    var tiedGroup = ranked.GetRange(i, groupSize);

                    // What each occupied placement in this range would individually be
                    // worth, ignoring membership - the "pool" tied members split.
                    var slotValues = new List<int>();
                    for (int r = rank; r < rank + groupSize; r++)
                    {
                        slotValues.Add(Math.Max(basePoints - (r - 1) * 100, 0));
                    }

                    var memberCount = tiedGroup.Count(e => e.HandlerIsMSSAMember);
                    int? sharedPoints = (groupSize > 1 && memberCount > 0)
                        ? (int)Math.Round(slotValues.Sum() / (decimal)memberCount, MidpointRounding.AwayFromZero)
                        : (int?)null;

                    foreach (var entry in tiedGroup)
                    {
                        entry.Placing = rank;

                        if (!entry.HandlerIsMSSAMember)
                        {
                            entry.TrialPoints = 0;
                        }
                        else
                        {
                            entry.TrialPoints = groupSize == 1 ? slotValues[0] : sharedPoints;
                        }

                        entry.ModifiedDate = DateTime.UtcNow;
                        entry.ModifiedBy = userId;
                    }

                    rank += groupSize;
                    i = j + 1;
                }
            }

            await db.SaveChangesAsync();
        }

        private static bool IsRealTie(MSSA_Entry a, MSSA_Entry b)
        {
            var scoreA = EffectiveScore(a);
            var scoreB = EffectiveScore(b);

            return scoreA.HasValue && scoreB.HasValue && scoreA == scoreB
                && a.RunTime.HasValue && b.RunTime.HasValue && a.RunTime == b.RunTime
                && a.TieBreakerTime == b.TieBreakerTime;
        }

        // A run's total score: the directly-entered value if present, otherwise the
        // 9-obstacle sum minus penalty (same rule as MSSA_Entry.TotalScore, duplicated
        // here since this runs against already-materialized entities, not a translated
        // SQL projection).
        private static decimal? EffectiveScore(MSSA_Entry e)
        {
            if (e.EnteredTotalScore.HasValue)
            {
                return e.EnteredTotalScore;
            }

            decimal sum = 0;
            bool any = false;
            void Add(decimal? v) { if (v.HasValue) { sum += v.Value; any = true; } }

            Add(e.ObstacleScore1); Add(e.ObstacleScore2); Add(e.ObstacleScore3);
            Add(e.ObstacleScore4); Add(e.ObstacleScore5); Add(e.ObstacleScore6);
            Add(e.ObstacleScore7); Add(e.ObstacleScore8); Add(e.ObstacleScore9);

            if (!any)
            {
                return null;
            }

            if (e.Penalty.HasValue)
            {
                sum -= e.Penalty.Value;
            }

            return sum;
        }

        private static bool IsScored(MSSA_Entry e) => e.RunTime.HasValue && EffectiveScore(e).HasValue;

        // ─────────────────────────────────────────────────────
        //  Submit / Approve
        // ─────────────────────────────────────────────────────

        public async Task<SubmitEventResultsDto> SubmitEventForApprovalAsync(int eventId, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var evt = await db.MSSA_Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null)
            {
                return new SubmitEventResultsDto { Success = false, Reasons = { "Event not found." } };
            }

            var trialIds = await db.MSSA_Trials.Where(t => t.EventId == eventId).Select(t => t.TrialId).ToListAsync();
            var entries = await db.MSSA_Entries.Where(e => trialIds.Contains(e.TrialId)).ToListAsync();

            var reasons = new List<string>();
            if (!entries.Any())
            {
                reasons.Add("This event has no entries yet.");
            }
            else
            {
                int unscored = entries.Count(e => !IsScored(e));
                if (unscored > 0)
                {
                    reasons.Add($"{unscored} run(s) still need a Time and Total Points entered.");
                }
            }

            if (reasons.Any())
            {
                return new SubmitEventResultsDto
                {
                    Success = false,
                    ResultsApprovalStatus = evt.ResultsApprovalStatus,
                    Reasons = reasons
                };
            }

            evt.ResultsApprovalStatus = EventResultsStatus.PendingApproval;
            evt.ResultsSubmittedDate = DateTime.UtcNow;
            evt.ResultsSubmittedByUserId = userId;
            evt.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return new SubmitEventResultsDto { Success = true, ResultsApprovalStatus = evt.ResultsApprovalStatus };
        }

        public async Task<SubmitEventResultsDto> ApproveEventAsync(int eventId, int userId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var evt = await db.MSSA_Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null)
            {
                return new SubmitEventResultsDto { Success = false, Reasons = { "Event not found." } };
            }

            evt.ResultsApprovalStatus = EventResultsStatus.Approved;
            evt.ResultsApprovedDate = DateTime.UtcNow;
            evt.ResultsApprovedByUserId = userId;
            evt.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return new SubmitEventResultsDto { Success = true, ResultsApprovalStatus = evt.ResultsApprovalStatus };
        }
    }
}

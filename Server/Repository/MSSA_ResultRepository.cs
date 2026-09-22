using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_Results.Enums;
using MountainStates.MSSA.Module.MSSA_Results.Models;
using MountainStates.MSSA.Module.MSSA_Results.Utilities;
using OfficeOpenXml;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Results.Repository
{
    public class MSSA_ResultRepository : IMSSA_ResultRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        // EPPlus's license must be set before any ExcelPackage is used, but this
        // repository is transient - a new instance per request - so the constructor
        // runs far more than once per process. Guard so the license call itself only
        // ever runs once, regardless of how many instances get created.
        private static bool _excelLicenseSet;
        private static readonly object _excelLicenseLock = new();

        public MSSA_ResultRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            EnsureExcelLicenseSet();
        }

        private static void EnsureExcelLicenseSet()
        {
            if (_excelLicenseSet)
            {
                return;
            }

            lock (_excelLicenseLock)
            {
                if (_excelLicenseSet)
                {
                    return;
                }

                ExcelPackage.License.SetNonCommercialOrganization("Mountain States Stockdog Association");
                _excelLicenseSet = true;
            }
        }

        public async Task<List<EventScoringSummary>> GetScoringEventsAsync(int? ownerUserId, int? scorekeeperUserId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Events.Where(e => e.IsActive && e.ResultsApprovalStatus != EventResultsStatus.Approved);

            if (ownerUserId.HasValue)
            {
                query = query.Where(e => e.CreatedByUserId == ownerUserId.Value);
            }
            else if (scorekeeperUserId.HasValue)
            {
                // A Scorekeeper doesn't own the Event - they're assigned per Trial, so an
                // event qualifies if any of its trials are assigned to them.
                var eventIdsWithScorekeeper = db.MSSA_Trials
                    .Where(t => t.ScorekeeperUserId == scorekeeperUserId.Value)
                    .Select(t => t.EventId);
                query = query.Where(e => eventIdsWithScorekeeper.Contains(e.EventId));
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

        public async Task<int?> GetTrialScorekeeperUserIdAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_Trials
                .Where(t => t.TrialId == trialId)
                .Select(t => t.ScorekeeperUserId)
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

            // MSSA_Entry.Year is [NotMapped] - it isn't loaded by the query above, so it
            // has to be derived here the same way MSSA_EntryRepository does: from the
            // trial's Event.PointYear, falling back to the Trial's date.
            var trialYear = await db.MSSA_Trials
                .Where(t => t.TrialId == trialId)
                .Join(db.MSSA_Events, t => t.EventId, ev => ev.EventId, (t, ev) => ev.PointYear ?? t.TrialDate.Year)
                .FirstOrDefaultAsync();
            var normalizedYear = NormalizeYear(trialYear);

            var futurityPairs = await db.MSSA_DogFuturityParticipation
                .Where(f => f.Year == normalizedYear && dogIds.Contains(f.DogId))
                .Select(f => f.DogId)
                .ToListAsync();
            var futuritySet = new HashSet<int>(futurityPairs);

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
                    DogName = (dogs.TryGetValue(e.DogId, out var dogName) ? dogName : "Unknown")
                        + (futuritySet.Contains(e.DogId) ? "+" : ""),
                    RunTimeStr = TimeParsingHelper.Format(e.RunTime),
                    TieBreakerTimeStr = TimeParsingHelper.Format(e.TieBreakerTime),
                    TotalScore = EffectiveScore(e),
                    Placing = e.Placing,
                    TrialPoints = e.TrialPoints
                })
                .ToList();
        }

        // Converts a possibly-2-digit legacy year (e.g. 24) to full 4-digit form (2024).
        // Leaves already-4-digit years untouched. Matches MSSA_EntryRepository's rule.
        private static int NormalizeYear(int year) => year < 100 ? 2000 + year : year;

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

        // ─────────────────────────────────────────────────────
        //  Scoring sheet (Excel) - generate and import
        // ─────────────────────────────────────────────────────

        // Column order here is just for a human filling it in by hand - the importer
        // reads by header name, in any order, and ignores anything extra.
        private static readonly string[] ScoreSheetHeaders =
        {
            "RunOrder", "ClassName", "SubClassName", "ClassId",
            "HandlerId", "HandlerName", "DogId", "DogName", "EntryId",
            "Time", "TieTime", "TotalPoints"
        };

        public async Task<byte[]> GenerateScoreSheetAsync(int trialId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var entries = await db.MSSA_Entries.Where(e => e.TrialId == trialId).ToListAsync();

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

            var ordered = entries
                .OrderBy(e => e.RunOrder ?? int.MaxValue)
                .ThenBy(e => classes.TryGetValue(e.ClassId, out var ci) ? ci.PrintOrder ?? int.MaxValue : int.MaxValue)
                .ToList();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Scoring Sheet");

            for (int col = 0; col < ScoreSheetHeaders.Length; col++)
            {
                ws.Cells[1, col + 1].Value = ScoreSheetHeaders[col];
            }

            using (var headerRange = ws.Cells[1, 1, 1, ScoreSheetHeaders.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;
            foreach (var e in ordered)
            {
                var c = classes.TryGetValue(e.ClassId, out var classInfo) ? classInfo : null;

                ws.Cells[row, 1].Value = e.RunOrder;
                ws.Cells[row, 2].Value = c?.ClassName ?? "";
                ws.Cells[row, 3].Value = c?.SubClassName ?? "";
                ws.Cells[row, 4].Value = e.ClassId;
                ws.Cells[row, 5].Value = e.HandlerId;
                ws.Cells[row, 6].Value = handlers.TryGetValue(e.HandlerId, out var hn) ? hn : "";
                ws.Cells[row, 7].Value = e.DogId;
                ws.Cells[row, 8].Value = dogs.TryGetValue(e.DogId, out var dn) ? dn : "";
                ws.Cells[row, 9].Value = e.EntryId;
                ws.Cells[row, 10].Value = TimeParsingHelper.Format(e.RunTime);
                ws.Cells[row, 11].Value = TimeParsingHelper.Format(e.TieBreakerTime);
                if (e.EnteredTotalScore.HasValue)
                {
                    ws.Cells[row, 12].Value = e.EnteredTotalScore.Value;
                }
                row++;
            }

            if (ws.Dimension != null)
            {
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        public async Task<ScoreSheetImportResult> ImportScoreSheetAsync(int trialId, byte[] fileBytes, int userId)
        {
            var result = new ScoreSheetImportResult();

            using var db = await _dbContextFactory.CreateDbContextAsync();
            var entries = await db.MSSA_Entries.Where(e => e.TrialId == trialId).ToListAsync();

            using var stream = new MemoryStream(fileBytes);
            using var package = new ExcelPackage(stream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();

            if (ws?.Dimension == null)
            {
                result.Warnings.Add("The file appears to be empty.");
                return result;
            }

            int lastCol = ws.Dimension.End.Column;
            int lastRow = ws.Dimension.End.Row;

            var headerIndex = new Dictionary<string, int>();
            for (int col = 1; col <= lastCol; col++)
            {
                var header = ws.Cells[1, col].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    headerIndex[NormalizeHeader(header)] = col;
                }
            }

            int? ColOf(string name) => headerIndex.TryGetValue(NormalizeHeader(name), out var c) ? c : (int?)null;

            var entryIdCol = ColOf("EntryId");
            var handlerIdCol = ColOf("HandlerId");
            var dogIdCol = ColOf("DogId");
            var classIdCol = ColOf("ClassId");
            var timeCol = ColOf("Time") ?? ColOf("RunTime");
            var tieTimeCol = ColOf("TieTime") ?? ColOf("TieBreakerTime");
            var totalPointsCol = ColOf("TotalPoints") ?? ColOf("TotalScore") ?? ColOf("Score");

            bool hasEntryIdColumn = entryIdCol.HasValue;
            bool hasTripleColumns = handlerIdCol.HasValue && dogIdCol.HasValue && classIdCol.HasValue;

            if (!hasEntryIdColumn && !hasTripleColumns)
            {
                result.Warnings.Add("The file must include either an EntryId column, or HandlerId, DogId, and ClassId columns.");
                return result;
            }

            var byEntryId = entries.ToDictionary(e => e.EntryId);
            var byTriple = entries
                .GroupBy(e => (e.HandlerId, e.DogId, e.ClassId))
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int row = 2; row <= lastRow; row++)
            {
                bool rowHasData = false;
                for (int col = 1; col <= lastCol; col++)
                {
                    if (ws.Cells[row, col].Value != null)
                    {
                        rowHasData = true;
                        break;
                    }
                }
                if (!rowHasData)
                {
                    continue;
                }

                result.RowsProcessed++;

                MSSA_Entry entry = null;

                if (hasEntryIdColumn)
                {
                    var entryId = ParseInt(ws.Cells[row, entryIdCol.Value].Value);
                    if (entryId.HasValue)
                    {
                        byEntryId.TryGetValue(entryId.Value, out entry);
                    }
                }

                if (entry == null && hasTripleColumns)
                {
                    var handlerId = ParseInt(ws.Cells[row, handlerIdCol.Value].Value);
                    var dogId = ParseInt(ws.Cells[row, dogIdCol.Value].Value);
                    var classId = ParseInt(ws.Cells[row, classIdCol.Value].Value);

                    if (handlerId.HasValue && dogId.HasValue && classId.HasValue)
                    {
                        if (byTriple.TryGetValue((handlerId.Value, dogId.Value, classId.Value), out var matches))
                        {
                            if (matches.Count == 1)
                            {
                                entry = matches[0];
                            }
                            else
                            {
                                result.Warnings.Add($"Row {row}: multiple entries match Handler {handlerId}, Dog {dogId}, Class {classId} - skipped.");
                                result.RowsSkipped++;
                                continue;
                            }
                        }
                    }
                }

                if (entry == null)
                {
                    result.Warnings.Add($"Row {row}: no matching entry found in this trial - skipped.");
                    result.RowsSkipped++;
                    continue;
                }

                if (timeCol.HasValue)
                {
                    entry.RunTime = ReadTimeCell(ws, row, timeCol.Value);
                }
                if (tieTimeCol.HasValue)
                {
                    entry.TieBreakerTime = ReadTimeCell(ws, row, tieTimeCol.Value);
                }
                if (totalPointsCol.HasValue)
                {
                    var raw = ws.Cells[row, totalPointsCol.Value].Value;
                    entry.EnteredTotalScore = raw != null && decimal.TryParse(raw.ToString(), out var score) ? score : (decimal?)null;
                }

                entry.ModifiedDate = DateTime.UtcNow;
                entry.ModifiedBy = userId;
                result.RowsUpdated++;
            }

            await db.SaveChangesAsync();

            return result;
        }

        public async Task<ImportCompleteTrialResult> ImportCompleteTrialAsync(int trialId, byte[] fileBytes, int userId)
        {
            var result = new ImportCompleteTrialResult();

            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existingKeys = (await db.MSSA_Entries
                .Where(e => e.TrialId == trialId)
                .Select(e => new { e.HandlerId, e.DogId, e.ClassId })
                .ToListAsync())
                .Select(e => (e.HandlerId, e.DogId, e.ClassId))
                .ToHashSet();

            var handlersById = await db.MSSA_Handlers.ToDictionaryAsync(h => h.HandlerId);
            var handlersByName = (await db.MSSA_Handlers.ToListAsync())
                .GroupBy(h => NormalizeName(h.FullName))
                .ToDictionary(g => g.Key, g => g.ToList());

            var dogsById = await db.MSSA_Dogs.ToDictionaryAsync(d => d.DogId);
            var dogsByName = (await db.MSSA_Dogs.ToListAsync())
                .GroupBy(d => NormalizeName(d.Name))
                .ToDictionary(g => g.Key, g => g.ToList());

            var classesById = await db.MSSA_Classes.ToDictionaryAsync(c => c.ClassId);
            // No Horseback signal in this file format, same as everywhere else this
            // assumption is made (see TimeParsingHelper, the Access migration scripts) -
            // matches On-foot only. A Horseback trial still needs entries added by hand.
            var classesByName = (await db.MSSA_Classes.ToListAsync())
                .Where(c => string.Equals(c.SubClassName, "On-foot", StringComparison.OrdinalIgnoreCase))
                .GroupBy(c => NormalizeName(c.ClassName))
                .ToDictionary(g => g.Key, g => g.First());

            using var stream = new MemoryStream(fileBytes);
            using var package = new ExcelPackage(stream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();

            if (ws?.Dimension == null)
            {
                result.Warnings.Add("The file appears to be empty.");
                return result;
            }

            int lastCol = ws.Dimension.End.Column;
            int lastRow = ws.Dimension.End.Row;

            var headerIndex = new Dictionary<string, int>();
            for (int col = 1; col <= lastCol; col++)
            {
                var header = ws.Cells[1, col].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    headerIndex[NormalizeHeader(header)] = col;
                }
            }

            int? ColOf(string name) => headerIndex.TryGetValue(NormalizeHeader(name), out var c) ? c : (int?)null;

            // "ClassId" is accepted as a header even though real spreadsheets (a Trial
            // Secretary's own template) often put the class NAME under it rather than
            // a numeric ID - both are handled row by row below.
            var classCol = ColOf("ClassId") ?? ColOf("ClassName") ?? ColOf("Class");
            var handlerIdCol = ColOf("HandlerId");
            var handlerNameCol = ColOf("HandlerName") ?? ColOf("Handler");
            var dogIdCol = ColOf("DogId");
            var dogNameCol = ColOf("DogName") ?? ColOf("Dog");
            var timeCol = ColOf("Time") ?? ColOf("RunTime");
            var tieTimeCol = ColOf("TieTime") ?? ColOf("TieBreakerTime");
            var totalPointsCol = ColOf("TotalPoints") ?? ColOf("TotalScore") ?? ColOf("Score");

            if (!classCol.HasValue || (!handlerIdCol.HasValue && !handlerNameCol.HasValue) || (!dogIdCol.HasValue && !dogNameCol.HasValue))
            {
                result.Warnings.Add("The file must include a Class column, a Handler (name or ID) column, and a Dog (name or ID) column.");
                return result;
            }

            var newEntries = new List<MSSA_Entry>();
            var unmatchedRows = new List<UnmatchedRow>();

            for (int row = 2; row <= lastRow; row++)
            {
                bool rowHasData = false;
                for (int col = 1; col <= lastCol; col++)
                {
                    if (ws.Cells[row, col].Value != null)
                    {
                        rowHasData = true;
                        break;
                    }
                }
                if (!rowHasData)
                {
                    continue;
                }

                result.RowsProcessed++;

                // Captured once per row, regardless of outcome - reused for both the
                // error-rows workbook and the plain-text warnings, and kept in the same
                // shape as the input columns so an error row can be fixed and the same
                // sheet re-uploaded as-is.
                var rowData = new UnmatchedRow
                {
                    ClassCell = classCol.HasValue ? ws.Cells[row, classCol.Value].Value?.ToString() : null,
                    HandlerIdCell = handlerIdCol.HasValue ? ws.Cells[row, handlerIdCol.Value].Value?.ToString() : null,
                    HandlerNameCell = handlerNameCol.HasValue ? ws.Cells[row, handlerNameCol.Value].Value?.ToString() : null,
                    DogIdCell = dogIdCol.HasValue ? ws.Cells[row, dogIdCol.Value].Value?.ToString() : null,
                    DogNameCell = dogNameCol.HasValue ? ws.Cells[row, dogNameCol.Value].Value?.ToString() : null,
                    TieTimeCell = tieTimeCol.HasValue ? ws.Cells[row, tieTimeCol.Value].Text : null,
                    TotalPointsCell = totalPointsCol.HasValue ? ws.Cells[row, totalPointsCol.Value].Value?.ToString() : null,
                    TimeCell = timeCol.HasValue ? ws.Cells[row, timeCol.Value].Text : null
                };

                var classCell = rowData.ClassCell;
                MSSA_Class matchedClass = null;
                if (int.TryParse(classCell, out var classIdNum))
                {
                    classesById.TryGetValue(classIdNum, out matchedClass);
                }
                if (matchedClass == null && !string.IsNullOrWhiteSpace(classCell))
                {
                    classesByName.TryGetValue(NormalizeName(classCell), out matchedClass);
                }
                if (matchedClass == null)
                {
                    rowData.Error = $"Class \"{classCell}\" not recognized.";
                    unmatchedRows.Add(rowData);
                    result.Warnings.Add($"Row {row}: {rowData.Error} - skipped.");
                    result.RowsSkippedUnmatched++;
                    continue;
                }

                var handler = ResolveByIdOrName(row, handlerIdCol, handlerNameCol, ws, handlersById, handlersByName, h => h.FullName);
                if (handler == null)
                {
                    rowData.Error = $"Handler (ID \"{rowData.HandlerIdCell}\", name \"{rowData.HandlerNameCell}\") not found, or the ID and name point at different people.";
                    unmatchedRows.Add(rowData);
                    result.Warnings.Add($"Row {row}: {rowData.Error} Add this handler, then re-upload to pick up just this row.");
                    result.RowsSkippedUnmatched++;
                    continue;
                }

                var dog = ResolveByIdOrName(row, dogIdCol, dogNameCol, ws, dogsById, dogsByName, d => d.Name);
                if (dog == null)
                {
                    rowData.Error = $"Dog (ID \"{rowData.DogIdCell}\", name \"{rowData.DogNameCell}\") not found, or the ID and name point at different dogs.";
                    unmatchedRows.Add(rowData);
                    result.Warnings.Add($"Row {row}: {rowData.Error} Add this dog, then re-upload to pick up just this row.");
                    result.RowsSkippedUnmatched++;
                    continue;
                }

                var key = (handler.HandlerId, dog.DogId, matchedClass.ClassId);
                if (!existingKeys.Add(key))
                {
                    result.RowsSkippedExisting++;
                    continue;
                }

                newEntries.Add(new MSSA_Entry
                {
                    TrialId = trialId,
                    HandlerId = handler.HandlerId,
                    DogId = dog.DogId,
                    ClassId = matchedClass.ClassId,
                    RunTime = timeCol.HasValue ? ReadTimeCell(ws, row, timeCol.Value) : null,
                    TieBreakerTime = tieTimeCol.HasValue ? ReadTimeCell(ws, row, tieTimeCol.Value) : null,
                    EnteredTotalScore = totalPointsCol.HasValue && decimal.TryParse(ws.Cells[row, totalPointsCol.Value].Value?.ToString(), out var score) ? score : (decimal?)null,
                    HandlerIsMSSAMember = false, // same default as adding an Entry by hand - not inferable from the file
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    EnteredBy = userId,
                    ModifiedBy = userId
                });
                result.RowsCreated++;
            }

            db.MSSA_Entries.AddRange(newEntries);
            await db.SaveChangesAsync();

            if (unmatchedRows.Any())
            {
                result.ErrorsFile = BuildUnmatchedRowsWorkbook(unmatchedRows);
            }

            return result;
        }

        // Carries one skipped row's original cell values (same shape as the input
        // columns, so the file this becomes can be fixed and re-uploaded as-is) plus
        // why it was skipped.
        private class UnmatchedRow
        {
            public string ClassCell;
            public string HandlerIdCell;
            public string HandlerNameCell;
            public string DogIdCell;
            public string DogNameCell;
            public string TieTimeCell;
            public string TotalPointsCell;
            public string TimeCell;
            public string Error;
        }

        private static byte[] BuildUnmatchedRowsWorkbook(List<UnmatchedRow> rows)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Unmatched Rows");

            string[] headers = { "ClassId", "HandlerName", "HandlerId", "DogName", "DogId", "TieTime", "TotalPoints", "Time", "Error" };
            for (int col = 0; col < headers.Length; col++)
            {
                ws.Cells[1, col + 1].Value = headers[col];
            }
            using (var headerRange = ws.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;
            foreach (var r in rows)
            {
                ws.Cells[row, 1].Value = r.ClassCell;
                ws.Cells[row, 2].Value = r.HandlerNameCell;
                ws.Cells[row, 3].Value = r.HandlerIdCell;
                ws.Cells[row, 4].Value = r.DogNameCell;
                ws.Cells[row, 5].Value = r.DogIdCell;
                ws.Cells[row, 6].Value = r.TieTimeCell;
                ws.Cells[row, 7].Value = r.TotalPointsCell;
                ws.Cells[row, 8].Value = r.TimeCell;
                ws.Cells[row, 9].Value = r.Error;
                row++;
            }

            if (ws.Dimension != null)
            {
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        // Matches by ID first, but only trusts it outright when there's no name to
        // check it against, or the name matches the ID'd record - otherwise (a typo'd
        // but still-valid ID, e.g. a transposed digit landing on someone else
        // entirely) falls through to matching by name instead of silently attaching
        // the row to the wrong person. An ambiguous name match (two records sharing a
        // name) is treated as unmatched rather than guessing which one was meant.
        private static T ResolveByIdOrName<T>(
            int row, int? idCol, int? nameCol, ExcelWorksheet ws,
            Dictionary<int, T> byId, Dictionary<string, List<T>> byName, Func<T, string> nameSelector)
            where T : class
        {
            var rowName = nameCol.HasValue ? ws.Cells[row, nameCol.Value].Value?.ToString() : null;

            if (idCol.HasValue)
            {
                var idText = ws.Cells[row, idCol.Value].Value?.ToString();
                if (int.TryParse(idText, out var id) && byId.TryGetValue(id, out var byIdMatch))
                {
                    if (string.IsNullOrWhiteSpace(rowName) || NormalizeName(nameSelector(byIdMatch)) == NormalizeName(rowName))
                    {
                        return byIdMatch;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(rowName) && byName.TryGetValue(NormalizeName(rowName), out var nameMatches) && nameMatches.Count == 1)
            {
                return nameMatches[0];
            }

            return null;
        }

        // Confirmed against a real Trial Secretary's spreadsheet (EPPlus 8.6.1):
        //   - A cell formatted as a clock time (e.g. "h:mm") comes back as a genuine
        //     DateTime, e.g. "1:52 AM" for an entered "1:52" - Excel read that as
        //     1 HOUR 52 MINUTES, not 1 minute 52 seconds. Every run time in this sport
        //     is well under an hour, so the intent is always minutes:seconds -
        //     reinterpret hour-as-minutes, minute-as-seconds, exactly the same
        //     short-time-entry ambiguity already corrected for the original Access
        //     migration (see 03_InsertFromOriginal_v2.sql).
        //   - A cell with no time formatting at all (plain "0.21") comes back as a
        //     bare double - this is this app's own "M.SS" shorthand (TimeParsingHelper),
        //     not a fraction of a day. Treating it as an Excel date serial (an earlier
        //     version of this method did) produced multi-hour nonsense and, for at
        //     least one real row, a value SQL Server's time column couldn't even hold.
        //     Round-trips through the same string parser as plain text instead.
        //   - Plain text ("1:52", "0.21") - unchanged, TimeParsingHelper as before.
        private static TimeSpan? ReadTimeCell(ExcelWorksheet ws, int row, int col)
        {
            var raw = ws.Cells[row, col].Value;
            if (raw is DateTime dt)
            {
                return new TimeSpan(0, 0, dt.Hour, dt.Minute, 0);
            }
            if (raw is double d)
            {
                return TimeParsingHelper.ParseMinutesSeconds(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            return TimeParsingHelper.ParseMinutesSeconds(raw?.ToString());
        }

        private static string NormalizeName(string name) =>
            string.IsNullOrWhiteSpace(name) ? "" : name.Trim().ToLowerInvariant();

        // Header matching ignores case, spaces, and punctuation - "Tie Time", "TieTime",
        // and "tie_time" all match the same column.
        private static string NormalizeHeader(string header)
        {
            return new string(header.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        private static int? ParseInt(object value)
        {
            return value != null && int.TryParse(value.ToString(), out var i) ? i : (int?)null;
        }
    }
}

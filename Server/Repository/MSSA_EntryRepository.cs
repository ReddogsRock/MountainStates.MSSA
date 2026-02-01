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
                                     TrialDate = t.TrialDate
                                 })
                                .OrderByDescending(e => e.TrialDate)
                                .ToListAsync();

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
                                     HandlerName = h.FullName,
                                     DogName = d.Name,
                                     ClassName = c.ClassName,
                                     SubClassName = c.SubClassName,
                                     RunOrder = e.RunOrder,
                                     Placing = e.Placing,
                                     TrialPoints = e.TrialPoints,
                                     TotalScore = (e.ObstacleScore1 ?? 0) + (e.ObstacleScore2 ?? 0) +
                                                 (e.ObstacleScore3 ?? 0) + (e.ObstacleScore4 ?? 0) +
                                                 (e.ObstacleScore5 ?? 0) + (e.ObstacleScore6 ?? 0) +
                                                 (e.ObstacleScore7 ?? 0) + (e.ObstacleScore8 ?? 0) +
                                                 (e.ObstacleScore9 ?? 0) - (e.Penalty ?? 0),
                                     EventName = ev.EventName,
                                     TrialDate = t.TrialDate,
                                     Stock = t.Stock
                                 })
                                .ToListAsync();

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
                                   TrialDate = t.TrialDate
                               })
                              .FirstOrDefaultAsync();

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

        public async Task GenerateRunOrderAsync(int trialId, int classId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            // Get all entries for this trial and class that don't have a run order
            var entries = await db.MSSA_Entries
                .Where(e => e.TrialId == trialId && e.ClassId == classId && !e.RunOrder.HasValue)
                .ToListAsync();

            if (entries.Any())
            {
                // Get the highest existing run order for this trial/class
                var maxRunOrder = await db.MSSA_Entries
                    .Where(e => e.TrialId == trialId && e.ClassId == classId && e.RunOrder.HasValue)
                    .Select(e => e.RunOrder.Value)
                    .DefaultIfEmpty(0)
                    .MaxAsync();

                // Randomize and assign run orders
                var random = new Random();
                var shuffled = entries.OrderBy(x => random.Next()).ToList();

                for (int i = 0; i < shuffled.Count; i++)
                {
                    shuffled[i].RunOrder = maxRunOrder + i + 1;
                    shuffled[i].ModifiedDate = DateTime.UtcNow;
                }

                await db.SaveChangesAsync();
            }
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public class MSSA_HandlerRepository : IMSSA_HandlerRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_HandlerRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<MSSA_Handler>> GetHandlersAsync(int moduleId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var handlers = await db.MSSA_Handlers
                .Where(h => h.IsActive)
                .OrderBy(h => h.LastName)
                .ThenBy(h => h.FirstName)
                .ToListAsync();

            // Populate state names
            var stateCodes = handlers.Select(h => h.StateCode).Distinct().ToList();
            var states = await db.MSSA_States
                .Where(s => stateCodes.Contains(s.StateCode))
                .ToDictionaryAsync(s => s.StateCode, s => s.StateName);

            foreach (var handler in handlers)
            {
                if (!string.IsNullOrEmpty(handler.StateCode) && states.ContainsKey(handler.StateCode))
                {
                    handler.StateName = states[handler.StateCode];
                }

                // Check for active membership
                handler.HasActiveMembership = await HasActiveMembershipAsync(db, handler.HandlerId);
            }

            return handlers;
        }

        public async Task<MSSA_Handler> GetHandlerAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var handler = await db.MSSA_Handlers
                .FirstOrDefaultAsync(h => h.HandlerId == handlerId);

            if (handler != null)
            {
                // Populate state name
                if (!string.IsNullOrEmpty(handler.StateCode))
                {
                    var state = await db.MSSA_States
                        .FirstOrDefaultAsync(s => s.StateCode == handler.StateCode);
                    handler.StateName = state?.StateName;
                }

                // Populate family member name
                if (handler.FamilyMemberHandlerId.HasValue)
                {
                    var familyMember = await db.MSSA_Handlers
                        .FirstOrDefaultAsync(h => h.HandlerId == handler.FamilyMemberHandlerId.Value);
                    handler.FamilyMemberName = familyMember?.FullName;
                }

                handler.HasActiveMembership = await HasActiveMembershipAsync(db, handlerId);
            }

            return handler;
        }

        public async Task<MSSA_Handler> AddHandlerAsync(MSSA_Handler handler)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            handler.CreatedDate = DateTime.UtcNow;
            handler.ModifiedDate = DateTime.UtcNow;

            db.MSSA_Handlers.Add(handler);
            await db.SaveChangesAsync();

            return handler;
        }

        public async Task<MSSA_Handler> UpdateHandlerAsync(MSSA_Handler handler)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            handler.ModifiedDate = DateTime.UtcNow;

            db.Entry(handler).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return handler;
        }

        public async Task DeleteHandlerAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var handler = await db.MSSA_Handlers.FindAsync(handlerId);
            if (handler != null)
            {
                // Soft delete
                handler.IsActive = false;
                handler.ModifiedDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MSSA_Handler>> SearchHandlersAsync(
            string searchTerm = null,
            string stateCode = null,
            string handlerLevel = null,
            bool? hasActiveMembership = null)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_Handlers.Where(h => h.IsActive);

            // Apply search term (name search)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(h =>
                    h.FirstName.ToLower().Contains(searchTerm) ||
                    h.LastName.ToLower().Contains(searchTerm) ||
                    (h.FirstName + " " + h.LastName).ToLower().Contains(searchTerm));
            }

            // Filter by state
            if (!string.IsNullOrWhiteSpace(stateCode))
            {
                query = query.Where(h => h.StateCode == stateCode);
            }

            // Filter by handler level
            if (!string.IsNullOrWhiteSpace(handlerLevel))
            {
                query = query.Where(h => h.HandlerLevel == handlerLevel);
            }

            var handlers = await query
                .OrderBy(h => h.LastName)
                .ThenBy(h => h.FirstName)
                .ToListAsync();

            // Filter by membership status if specified
            if (hasActiveMembership.HasValue)
            {
                var currentYear = DateTime.Now.Year;
                var handlerIds = await db.MSSA_HandlerMemberships
                    .Where(m => m.IsActive && m.StartYear <= currentYear && m.EndYear >= currentYear)
                    .Select(m => m.HandlerId)
                    .Distinct()
                    .ToListAsync();

                handlers = hasActiveMembership.Value
                    ? handlers.Where(h => handlerIds.Contains(h.HandlerId)).ToList()
                    : handlers.Where(h => !handlerIds.Contains(h.HandlerId)).ToList();
            }

            // Populate state names
            var stateCodes = handlers.Select(h => h.StateCode).Distinct().ToList();
            var states = await db.MSSA_States
                .Where(s => stateCodes.Contains(s.StateCode))
                .ToDictionaryAsync(s => s.StateCode, s => s.StateName);

            foreach (var handler in handlers)
            {
                if (!string.IsNullOrEmpty(handler.StateCode) && states.ContainsKey(handler.StateCode))
                {
                    handler.StateName = states[handler.StateCode];
                }
                handler.HasActiveMembership = await HasActiveMembershipAsync(db, handler.HandlerId);
            }

            return handlers;
        }

        // Memberships
        public async Task<IEnumerable<MSSA_HandlerMembership>> GetHandlerMembershipsAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_HandlerMemberships
                .Where(m => m.HandlerId == handlerId && m.IsActive)
                .OrderByDescending(m => m.StartYear)
                .ToListAsync();
        }

        public async Task<MSSA_HandlerMembership> AddMembershipAsync(MSSA_HandlerMembership membership)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            db.MSSA_HandlerMemberships.Add(membership);
            await db.SaveChangesAsync();

            return membership;
        }

        public async Task<MSSA_HandlerMembership> UpdateMembershipAsync(MSSA_HandlerMembership membership)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            db.Entry(membership).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return membership;
        }

        public async Task DeleteMembershipAsync(int membershipId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var membership = await db.MSSA_HandlerMemberships.FindAsync(membershipId);
            if (membership != null)
            {
                membership.IsActive = false;
                await db.SaveChangesAsync();
            }
        }

        // Entries
        public async Task<IEnumerable<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();
/*
            var entries = await (from e in db.MSSA_Entries
                                 join t in db.MSSA_Trials on e.TrialId equals t.TrialId
                                 join ev in db.MSSA_Events on t.EventId equals ev.EventId
                                 join d in db.MSSA_Dogs on e.DogId equals d.DogId
                                 join c in db.MSSA_Classes on e.ClassId equals c.ClassId
                                 where e.HandlerId == handlerId
                                 select new MSSA_HandlerEntry
                                 {
                                     EntryId = e.EntryId,
                                     DogName = d.Name,
                                     ClassName = c.ClassName,
                                     SubClassName = c.SubClassName,
                                     Stock = t.Stock,
                                     Placing = e.Placing,
                                     TrialPoints = e.TrialPoints,
                                     TrialDate = t.TrialDate,
                                     EventName = ev.EventName,
                                     Year = ev.PointYear ?? t.TrialDate.Year
                                 })
                                .OrderByDescending(e => e.TrialDate)
                                .ToListAsync();

            return entries; */

            // Temporary: Return empty list until other tables are created
            return await Task.FromResult(new List<MSSA_HandlerEntry>());
        }

        // Helper method
        private async Task<bool> HasActiveMembershipAsync(MSSADbContext db, int handlerId)
        {
            var currentYear = DateTime.Now.Year;
            return await db.MSSA_HandlerMemberships
                .AnyAsync(m => m.HandlerId == handlerId &&
                              m.IsActive &&
                              m.StartYear <= currentYear &&
                              m.EndYear >= currentYear);
        }
    }
}
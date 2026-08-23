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

                handler.Memberships = await LoadHandlerMembershipsAsync(db, handlerId);
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
            string handlerLevel = null)
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
            }

            return handlers;
        }

        // Entries
        public async Task<IEnumerable<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

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

            return entries;
        }

        // Memberships

        // All membership periods this handler has ever been linked to (current and
        // historical), newest first, each with its full member list populated.
        public async Task<List<MSSA_Membership>> GetHandlerMembershipsAsync(int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await LoadHandlerMembershipsAsync(db, handlerId);
        }

        // Creates a new membership period and links it to every handler in
        // membership.MemberHandlerIds (the primary purchaser plus however many family
        // members - can be zero additional for Individual, any number for Family).
        // EndYear is computed from MembershipType + StartYear rather than trusting a
        // client-supplied value.
        public async Task<MSSA_Membership> AddMembershipAsync(MSSA_Membership membership)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var handlerIds = membership.MemberHandlerIds ?? new List<int>();
            var primaryHandlerId = membership.PrimaryHandlerId;

            membership.EndYear = ComputeEndYear(membership.MembershipType, membership.StartYear);
            membership.CreatedDate = DateTime.UtcNow;
            membership.ModifiedDate = DateTime.UtcNow;
            membership.MemberHandlerIds = new List<int>(); // not mapped - keep EF from touching it

            db.MSSA_Memberships.Add(membership);
            await db.SaveChangesAsync();

            foreach (var handlerId in handlerIds.Distinct())
            {
                db.MSSA_MembershipHandlers.Add(new MSSA_MembershipHandler
                {
                    MembershipId = membership.MembershipId,
                    HandlerId = handlerId,
                    IsPrimary = handlerId == primaryHandlerId
                });
            }
            await db.SaveChangesAsync();

            membership.Members = await LoadMembersAsync(db, membership.MembershipId);
            return membership;
        }

        // Updates the purchase details (type/years/amount/etc) of an existing
        // membership - not its member list, which is managed separately via
        // AddMemberToMembershipAsync/RemoveMemberFromMembershipAsync.
        public async Task<MSSA_Membership> UpdateMembershipAsync(MSSA_Membership membership)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            membership.EndYear = ComputeEndYear(membership.MembershipType, membership.StartYear);
            membership.ModifiedDate = DateTime.UtcNow;

            db.Entry(membership).State = EntityState.Modified;
            await db.SaveChangesAsync();

            membership.Members = await LoadMembersAsync(db, membership.MembershipId);
            return membership;
        }

        public async Task DeleteMembershipAsync(int membershipId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var links = await db.MSSA_MembershipHandlers
                .Where(mh => mh.MembershipId == membershipId)
                .ToListAsync();
            db.MSSA_MembershipHandlers.RemoveRange(links);

            var membership = await db.MSSA_Memberships.FindAsync(membershipId);
            if (membership != null)
            {
                db.MSSA_Memberships.Remove(membership);
            }

            await db.SaveChangesAsync();
        }

        // Adds one more family member to an existing membership - supports adding
        // children to a Family membership incrementally, any time, not just when the
        // membership was first purchased.
        public async Task<List<MembershipMemberInfo>> AddMemberToMembershipAsync(int membershipId, int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var alreadyLinked = await db.MSSA_MembershipHandlers
                .AnyAsync(mh => mh.MembershipId == membershipId && mh.HandlerId == handlerId);

            if (!alreadyLinked)
            {
                db.MSSA_MembershipHandlers.Add(new MSSA_MembershipHandler
                {
                    MembershipId = membershipId,
                    HandlerId = handlerId,
                    IsPrimary = false
                });
                await db.SaveChangesAsync();
            }

            return await LoadMembersAsync(db, membershipId);
        }

        public async Task<List<MembershipMemberInfo>> RemoveMemberFromMembershipAsync(int membershipId, int handlerId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var link = await db.MSSA_MembershipHandlers
                .FirstOrDefaultAsync(mh => mh.MembershipId == membershipId && mh.HandlerId == handlerId);

            if (link != null)
            {
                db.MSSA_MembershipHandlers.Remove(link);
                await db.SaveChangesAsync();
            }

            return await LoadMembersAsync(db, membershipId);
        }

        // Cross-handler membership search for the Membership admin module - unlike
        // GetHandlerMembershipsAsync, this isn't scoped to one handler. filter is one
        // of "ExpiringThisYear", "Expired", "PendingPayment", or null/"All" for no
        // filter. searchTerm matches against any covered handler's name.
        public async Task<List<MSSA_Membership>> SearchMembershipsAsync(string filter, string searchTerm)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var currentYear = DateTime.Today.Year;
            var query = db.MSSA_Memberships.AsQueryable();

            switch (filter)
            {
                case "ExpiringThisYear":
                    // Still active, but this is the last year it covers.
                    query = query.Where(m => m.EndYear == currentYear);
                    break;
                case "Expired":
                    // Coverage ended in a prior year and was never renewed since.
                    query = query.Where(m => m.EndYear != null && m.EndYear < currentYear);
                    break;
                case "PendingPayment":
                    // No DateReceived on file yet - the natural "awaiting payment" signal,
                    // since every other membership field already gets filled in at
                    // creation time (see AddMembershipAsync).
                    //
                    // Excludes the migration's Pass 2 historical placeholder rows
                    // (built from M2016-M2029 flags with no known payment details) -
                    // those aren't people who currently owe money, they're old years
                    // with no record captured. Matched by their exact construction
                    // signature from MigrateMemberships.sql: MembershipType='Unknown',
                    // Amount and PaidBy both null, and StartYear == EndYear (always a
                    // single placeholder year, never a real multi-year purchase).
                    query = query.Where(m => m.DateReceived == null &&
                        !(m.MembershipType == "Unknown" && m.Amount == null && m.PaidBy == null && m.StartYear == m.EndYear));
                    break;
                // null or "All": no filter.
            }

            var memberships = await query
                .OrderBy(m => m.EndYear ?? int.MaxValue) // Lifetime (null) sorts last
                .ThenBy(m => m.StartYear)
                .ToListAsync();

            foreach (var membership in memberships)
            {
                membership.Members = await LoadMembersAsync(db, membership.MembershipId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                memberships = memberships
                    .Where(m => m.Members.Any(x => x.HandlerName != null && x.HandlerName.ToLower().Contains(term)))
                    .ToList();
            }

            return memberships;
        }

        // AI/AF = 1 year, 3I/3F = 3 years, Lifetime = never expires (null EndYear).
        private static int? ComputeEndYear(string membershipType, int startYear)
        {
            return membershipType switch
            {
                "AI" or "AF" => startYear,
                "3I" or "3F" => startYear + 2,
                "Lifetime" => null,
                _ => startYear
            };
        }

        private static async Task<List<MSSA_Membership>> LoadHandlerMembershipsAsync(MSSADbContext db, int handlerId)
        {
            var membershipIds = await db.MSSA_MembershipHandlers
                .Where(mh => mh.HandlerId == handlerId)
                .Select(mh => mh.MembershipId)
                .ToListAsync();

            var memberships = await db.MSSA_Memberships
                .Where(m => membershipIds.Contains(m.MembershipId))
                .OrderByDescending(m => m.StartYear)
                .ToListAsync();

            foreach (var membership in memberships)
            {
                membership.Members = await LoadMembersAsync(db, membership.MembershipId);
            }

            return memberships;
        }

        private static async Task<List<MembershipMemberInfo>> LoadMembersAsync(MSSADbContext db, int membershipId)
        {
            return await (from mh in db.MSSA_MembershipHandlers
                          join h in db.MSSA_Handlers on mh.HandlerId equals h.HandlerId
                          where mh.MembershipId == membershipId
                          orderby mh.IsPrimary descending, h.LastName, h.FirstName
                          select new MembershipMemberInfo
                          {
                              HandlerId = h.HandlerId,
                              HandlerName = h.FullName,
                              Email = h.Email,
                              IsPrimary = mh.IsPrimary
                          })
                          .ToListAsync();
        }
    }
}
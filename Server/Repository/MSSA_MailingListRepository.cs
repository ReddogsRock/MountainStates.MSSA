using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public class MSSA_MailingListRepository : IMSSA_MailingListRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_MailingListRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetActiveSubscribersAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_MailingListSubscribers
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        public async Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm, bool expiringOnly)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_MailingListSubscribers.AsQueryable();

            if (expiringOnly)
            {
                query = query.Where(s => s.IsExpiringMembership);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(s =>
                    s.Email.Contains(term) ||
                    (s.FirstName != null && s.FirstName.Contains(term)) ||
                    (s.LastName != null && s.LastName.Contains(term)));
            }

            // The expiring segment is a bounded, moderate-size list (a few hundred at
            // most) - safe to show in full rather than capping like a general search
            // over the whole subscriber table would need to.
            var limit = expiringOnly ? 1000 : 100;

            return await query
                .OrderBy(s => s.Email)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<MSSA_MailingListSubscriber> AddSubscriberAsync(MSSA_MailingListSubscriber subscriber)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            subscriber.SubscriberId = 0;
            subscriber.CreatedDate = DateTime.UtcNow;
            subscriber.ModifiedDate = DateTime.UtcNow;

            db.MSSA_MailingListSubscribers.Add(subscriber);
            await db.SaveChangesAsync();

            return subscriber;
        }

        public async Task<MSSA_MailingListSubscriber> UpdateSubscriberAsync(MSSA_MailingListSubscriber subscriber)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_MailingListSubscribers.FindAsync(subscriber.SubscriberId);
            if (existing == null)
            {
                return null;
            }

            existing.Email = subscriber.Email;
            existing.FirstName = subscriber.FirstName;
            existing.LastName = subscriber.LastName;
            existing.IsHandler = subscriber.IsHandler;

            // UnsubscribedDate tracks the transition, not just the current flag - only
            // touch it when IsActive actually changes.
            if (existing.IsActive && !subscriber.IsActive)
            {
                existing.UnsubscribedDate = DateTime.UtcNow;
            }
            else if (!existing.IsActive && subscriber.IsActive)
            {
                existing.UnsubscribedDate = null;
            }
            existing.IsActive = subscriber.IsActive;

            existing.ModifiedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return existing;
        }

        public async Task SubscribeAsync(string email, string firstName, string lastName)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var trimmedEmail = email.Trim();
            var existing = await db.MSSA_MailingListSubscribers
                .FirstOrDefaultAsync(s => s.Email == trimmedEmail);

            if (existing == null)
            {
                db.MSSA_MailingListSubscribers.Add(new MSSA_MailingListSubscriber
                {
                    Email = trimmedEmail,
                    FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim(),
                    LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
                    IsHandler = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                });
            }
            else if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UnsubscribedDate = null;
                existing.ModifiedDate = DateTime.UtcNow;
            }
            // Already active - nothing to do, succeeds quietly either way.

            await db.SaveChangesAsync();
        }

        // Real membership types only - 'Unknown' rows have no confirmed status to act
        // on, and Lifetime never expires so it's deliberately excluded by not being in
        // this list.
        private static readonly string[] RealMembershipTypes = { "AI", "AF", "3I", "3F", "4I" };

        public async Task<int> SyncExpiringMembershipSegmentAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var currentYear = DateTime.UtcNow.Year;

            // Handlers whose latest real membership expires at the end of this year -
            // i.e. they haven't renewed past it yet.
            var expiringHandlerIds = await (
                from mh in db.MSSA_MembershipHandlers
                join m in db.MSSA_Memberships on mh.MembershipId equals m.MembershipId
                where mh.IsPrimary && RealMembershipTypes.Contains(m.MembershipType)
                group m.EndYear by mh.HandlerId into g
                where g.Max() == currentYear
                select g.Key
            ).ToListAsync();

            var expiringHandlers = await db.MSSA_Handlers
                .Where(h => expiringHandlerIds.Contains(h.HandlerId) && h.Email != null && h.Email != "")
                .Select(h => new { h.Email, h.FirstName, h.LastName })
                .ToListAsync();

            // Load everyone once - resets anyone previously flagged (so a renewal since
            // the last sync drops them out automatically), then flags this year's set.
            var allSubscribers = await db.MSSA_MailingListSubscribers.ToListAsync();
            foreach (var s in allSubscribers)
            {
                if (s.IsExpiringMembership)
                {
                    s.IsExpiringMembership = false;
                    s.ModifiedDate = DateTime.UtcNow;
                }
            }
            var byEmail = allSubscribers.ToDictionary(s => s.Email.Trim().ToLowerInvariant());

            var flaggedCount = 0;
            foreach (var h in expiringHandlers)
            {
                var key = h.Email.Trim().ToLowerInvariant();
                if (byEmail.TryGetValue(key, out var existing))
                {
                    existing.IsExpiringMembership = true;
                    existing.ModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    var newSubscriber = new MSSA_MailingListSubscriber
                    {
                        Email = h.Email.Trim(),
                        FirstName = h.FirstName,
                        LastName = h.LastName,
                        IsHandler = true,
                        IsActive = true,
                        IsExpiringMembership = true,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };
                    db.MSSA_MailingListSubscribers.Add(newSubscriber);
                    byEmail[key] = newSubscriber;
                }
                flaggedCount++;
            }

            await db.SaveChangesAsync();

            return flaggedCount;
        }

        // CSV export target - active subscribers only, so an opt-out is respected even
        // for this special-purpose segment.
        public async Task<List<MSSA_MailingListSubscriber>> GetExpiringMembershipSubscribersAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_MailingListSubscribers
                .Where(s => s.IsExpiringMembership && s.IsActive)
                .OrderBy(s => s.LastName)
                .ToListAsync();
        }

        public async Task DeleteSubscriberAsync(int subscriberId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_MailingListSubscribers.FindAsync(subscriberId);
            if (existing != null)
            {
                db.MSSA_MailingListSubscribers.Remove(existing);
                await db.SaveChangesAsync();
            }
        }
    }
}

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

        public async Task<List<MSSA_MailingListSubscriber>> SearchSubscribersAsync(string searchTerm)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.MSSA_MailingListSubscribers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(s =>
                    s.Email.Contains(term) ||
                    (s.FirstName != null && s.FirstName.Contains(term)) ||
                    (s.LastName != null && s.LastName.Contains(term)));
            }

            return await query
                .OrderBy(s => s.Email)
                .Take(100)
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

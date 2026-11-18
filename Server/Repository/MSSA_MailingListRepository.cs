using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
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

        // CSV export target - active subscribers only (respects an opt-out even for
        // this special-purpose segment), joined back to Handlers by email to pull
        // HandlerId for building a personalized renewal link.
        public async Task<List<MSSA_ExpiringMembershipExportRow>> GetExpiringMembershipExportRowsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var subscribers = await db.MSSA_MailingListSubscribers
                .Where(s => s.IsExpiringMembership && s.IsActive)
                .OrderBy(s => s.LastName)
                .ToListAsync();

            var handlerIdsByEmail = await GetHandlerIdsByEmailAsync();

            return subscribers.Select(s => new MSSA_ExpiringMembershipExportRow
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                HandlerId = handlerIdsByEmail.TryGetValue(s.Email.Trim().ToLowerInvariant(), out var id) ? id : (int?)null
            }).ToList();
        }

        // Email -> HandlerId, lowercased/trimmed key. Grouped rather than a straight
        // ToDictionary - several households share one email across multiple Handler
        // rows, which would otherwise throw on the first duplicate key. Any one of
        // them is a fine link target since they share the inbox anyway. Shared by the
        // CSV export above and the {RenewalLink} merge in MailgunService.SendCampaignAsync.
        public async Task<Dictionary<string, int>> GetHandlerIdsByEmailAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return (await db.MSSA_Handlers
                .Where(h => h.Email != null && h.Email != "")
                .Select(h => new { h.Email, h.HandlerId })
                .ToListAsync())
                .GroupBy(h => h.Email.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First().HandlerId);
        }

        public async Task<List<MSSA_MailingListNamedList>> GetNamedListsAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var lists = await db.MSSA_MailingListNamedLists
                .OrderBy(l => l.Name)
                .ToListAsync();

            var counts = await db.MSSA_MailingListNamedListMembers
                .GroupBy(m => m.NamedListId)
                .Select(g => new { NamedListId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.NamedListId, g => g.Count);

            foreach (var list in lists)
            {
                list.MemberCount = counts.TryGetValue(list.NamedListId, out var count) ? count : 0;
            }

            return lists;
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetNamedListMembersAsync(int namedListId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var subscriberIds = await db.MSSA_MailingListNamedListMembers
                .Where(m => m.NamedListId == namedListId)
                .Select(m => m.SubscriberId)
                .ToListAsync();

            return await db.MSSA_MailingListSubscribers
                .Where(s => subscriberIds.Contains(s.SubscriberId))
                .OrderBy(s => s.Email)
                .ToListAsync();
        }

        public async Task<MSSA_MailingListNamedList> AddNamedListAsync(string name)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var list = new MSSA_MailingListNamedList
            {
                Name = name.Trim(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            db.MSSA_MailingListNamedLists.Add(list);
            await db.SaveChangesAsync();

            return list;
        }

        public async Task<MSSA_MailingListNamedList> RenameNamedListAsync(int namedListId, string name)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_MailingListNamedLists.FindAsync(namedListId);
            if (existing == null)
            {
                return null;
            }

            existing.Name = name.Trim();
            existing.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return existing;
        }

        public async Task DeleteNamedListAsync(int namedListId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var members = await db.MSSA_MailingListNamedListMembers
                .Where(m => m.NamedListId == namedListId)
                .ToListAsync();
            db.MSSA_MailingListNamedListMembers.RemoveRange(members);

            var existing = await db.MSSA_MailingListNamedLists.FindAsync(namedListId);
            if (existing != null)
            {
                db.MSSA_MailingListNamedLists.Remove(existing);
            }

            await db.SaveChangesAsync();
        }

        public async Task SetNamedListMembersAsync(int namedListId, List<int> subscriberIds)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_MailingListNamedListMembers
                .Where(m => m.NamedListId == namedListId)
                .ToListAsync();
            db.MSSA_MailingListNamedListMembers.RemoveRange(existing);

            var now = DateTime.UtcNow;
            foreach (var subscriberId in subscriberIds.Distinct())
            {
                db.MSSA_MailingListNamedListMembers.Add(new MSSA_MailingListNamedListMember
                {
                    NamedListId = namedListId,
                    SubscriberId = subscriberId,
                    CreatedDate = now
                });
            }

            var list = await db.MSSA_MailingListNamedLists.FindAsync(namedListId);
            if (list != null)
            {
                list.ModifiedDate = now;
            }

            await db.SaveChangesAsync();
        }

        public async Task<List<MSSA_MailingListSubscriber>> GetCampaignRecipientsAsync(string audience, int? namedListId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            if (audience == MailingListAudience.Expiring)
            {
                return await db.MSSA_MailingListSubscribers
                    .Where(s => s.IsActive && s.IsExpiringMembership)
                    .ToListAsync();
            }

            if (audience == MailingListAudience.NamedList)
            {
                // No list id to scope to - fail safe to nobody rather than accidentally
                // falling through to "everyone".
                if (!namedListId.HasValue)
                {
                    return new List<MSSA_MailingListSubscriber>();
                }

                var subscriberIds = await db.MSSA_MailingListNamedListMembers
                    .Where(m => m.NamedListId == namedListId.Value)
                    .Select(m => m.SubscriberId)
                    .ToListAsync();

                return await db.MSSA_MailingListSubscribers
                    .Where(s => s.IsActive && subscriberIds.Contains(s.SubscriberId))
                    .ToListAsync();
            }

            // MailingListAudience.All
            return await db.MSSA_MailingListSubscribers
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        public async Task<List<MSSA_MailingListEmailTemplate>> GetEmailTemplatesAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_MailingListEmailTemplates
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<MSSA_MailingListEmailTemplate> GetEmailTemplateAsync(int templateId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_MailingListEmailTemplates.FindAsync(templateId);
        }

        public async Task<MSSA_MailingListEmailTemplate> AddEmailTemplateAsync(MSSA_MailingListEmailTemplate template)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            template.TemplateId = 0;
            template.CreatedDate = DateTime.UtcNow;
            template.ModifiedDate = DateTime.UtcNow;

            db.MSSA_MailingListEmailTemplates.Add(template);
            await db.SaveChangesAsync();

            return template;
        }

        public async Task<MSSA_MailingListEmailTemplate> UpdateEmailTemplateAsync(MSSA_MailingListEmailTemplate template)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_MailingListEmailTemplates.FindAsync(template.TemplateId);
            if (existing == null)
            {
                return null;
            }

            existing.Name = template.Name;
            existing.Subject = template.Subject;
            existing.BodyHtml = template.BodyHtml;
            existing.ModifiedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return existing;
        }

        public async Task DeleteEmailTemplateAsync(int templateId)
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            var existing = await db.MSSA_MailingListEmailTemplates.FindAsync(templateId);
            if (existing != null)
            {
                db.MSSA_MailingListEmailTemplates.Remove(existing);
                await db.SaveChangesAsync();
            }
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

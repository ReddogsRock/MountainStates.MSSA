using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using Oqtane.Modules;

namespace MountainStates.MSSA.Server.Startup
{
    public interface IMSSA_AdminNotificationService
    {
        // Queues an email to every user holding the Administrators role for the site -
        // actual sending is handled by Oqtane's own Notification Job (same SMTP settings
        // already configured in Host Settings), not by any code here. Fire-and-forget by
        // design: never throws - a failed notification should never fail the action that
        // triggered it (an event/dog/payment that genuinely saved shouldn't come back as
        // an error just because the "let Admin know" step had a problem).
        void NotifyAdmins(int siteId, string subject, string body);

        // For the admin-facing "Notification Log" report - every notification this
        // service has ever queued (not Oqtane's own system emails, like registration or
        // site-error notifications, which share the same table but never carry this
        // subject tag), most recent first.
        List<Notification> GetRecentNotifications(int siteId, int count);
    }

    public class MSSA_AdminNotificationService : IMSSA_AdminNotificationService, ITransientService
    {
        // Every subject this service sends is tagged with this prefix so the Notification
        // Log report can tell them apart from Oqtane's own system notifications sharing
        // the same table (registration emails, site error alerts, etc.).
        private const string SubjectTag = "[MSSA] ";

        private readonly IUserRoleRepository _userRoleRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IDbContextFactory<TenantDBContext> _tenantDbContextFactory;
        private readonly ILogger<MSSA_AdminNotificationService> _logger;

        public MSSA_AdminNotificationService(
            IUserRoleRepository userRoleRepository,
            INotificationRepository notificationRepository,
            IDbContextFactory<TenantDBContext> tenantDbContextFactory,
            ILogger<MSSA_AdminNotificationService> logger)
        {
            _userRoleRepository = userRoleRepository;
            _notificationRepository = notificationRepository;
            _tenantDbContextFactory = tenantDbContextFactory;
            _logger = logger;
        }

        public void NotifyAdmins(int siteId, string subject, string body)
        {
            try
            {
                var admins = _userRoleRepository.GetUserRoles(siteId)
                    .Where(ur => ur.Role.Name == RoleNames.Admin)
                    .Select(ur => ur.User)
                    .GroupBy(u => u.UserId).Select(g => g.First()); // a user can hold more than one role

                foreach (var admin in admins)
                {
                    _notificationRepository.AddNotification(new Notification(siteId, admin, SubjectTag + subject, body));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing admin notification: {Subject}", subject);
            }
        }

        public List<Notification> GetRecentNotifications(int siteId, int count)
        {
            using var db = _tenantDbContextFactory.CreateDbContext();

            return db.Notification
                .Where(n => n.SiteId == siteId && n.Subject.StartsWith(SubjectTag) && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedOn)
                .Take(count)
                .ToList();
        }
    }
}

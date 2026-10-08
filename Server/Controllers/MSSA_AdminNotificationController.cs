using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;
using MountainStates.MSSA.Server.Startup;

namespace MountainStates.MSSA.Module.MSSA_Events.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_AdminNotificationController : ModuleControllerBase
    {
        private readonly IMSSA_AdminNotificationService _notificationService;

        public MSSA_AdminNotificationController(IMSSA_AdminNotificationService notificationService, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _notificationService = notificationService;
        }

        // GET: api/MSSA_AdminNotification?count=200&moduleid=x
        // Admin-only - this is the "who got emailed, about what, and did it actually
        // send" log, not something a Trial Secretary needs to see.
        [HttpGet]
        [Authorize(Policy = PolicyNames.EditModule)]
        public List<Notification> Get(int moduleId, int count = 200)
        {
            if (!User.IsInRole(RoleNames.Admin))
            {
                HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }

            return _notificationService.GetRecentNotifications(HttpContext.GetAlias().SiteId, count);
        }
    }
}

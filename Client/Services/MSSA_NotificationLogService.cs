using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;

namespace MountainStates.MSSA.Module.MSSA_Events.Services
{
    public class MSSA_NotificationLogService : ServiceBase, IMSSA_NotificationLogService, IService
    {
        public MSSA_NotificationLogService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_AdminNotification");

        public async Task<List<Notification>> GetRecentNotificationsAsync(int count, int moduleId)
        {
            return await GetJsonAsync<List<Notification>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}?count={count}&moduleid={moduleId}", EntityNames.Module, moduleId));
        }
    }
}

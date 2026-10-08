using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Services
{
    public interface IMSSA_NotificationLogService
    {
        Task<List<Notification>> GetRecentNotificationsAsync(int count, int moduleId);
    }
}

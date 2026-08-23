using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_MembershipAdmin
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Membership Admin",
            Description = "Cross-handler membership monitoring: expiring, expired, and pending-payment views, plus marking a membership paid",
            Version = "1.0.0",
            // No dedicated server/manager/repository - this module reuses the existing
            // MSSA_Handlers module's manager/repository/service chain directly, since
            // MSSA_Membership already lives there.
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Handlers.Manager.MSSA_HandlerManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_MembershipAdmin.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_MembershipAdmin"
        };
    }
}

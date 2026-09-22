using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_ViewResults
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA View Results",
            Description = "Public, read-only trial results browsing for competitors",
            Version = "1.0.0",
            // No dedicated server/manager/repository - this module reuses the existing
            // MSSA_Event and MSSA_Result manager/service chains directly.
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Events.Manager.MSSA_EventManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_ViewResults.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_ViewResults"
        };
    }
}

using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Events
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Events",
            Description = "Manage herding trial events and trials for MSSA",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Events.Manager.MSSA_EventManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Events.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Events"
        };
    }
}

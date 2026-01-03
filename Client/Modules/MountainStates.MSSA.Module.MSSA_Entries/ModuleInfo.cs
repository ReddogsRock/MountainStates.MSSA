using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Entries
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Entries",
            Description = "Manage competition entries for MSSA herding trials",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Entries.Manager.MSSA_EntryManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Entries.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Entries"
        };
    }
}

using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Finals
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Finals",
            Description = "Display and search MSSA Finals results with filtering",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Finals.Manager.MSSA_FinalsManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Finals.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Finals"
        };
    }
}

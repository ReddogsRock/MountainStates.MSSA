using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Dogs
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Dogs",
            Description = "Manage competition dogs for MSSA herding trials",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Dogs.Manager.MSSA_DogManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Dogs.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Dogs"
        };
    }
}

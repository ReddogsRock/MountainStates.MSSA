using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Results
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Results",
            Description = "Score entry and results approval for MSSA herding trials",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Results.Manager.MSSA_ResultManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Results.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Results"
        };
    }
}

using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.TrialSecretary
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Trial Secretary",
            Description = "Enter dog handler teams into herding trials",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.TrialSecretary.Manager.TrialSecretaryManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.TrialSecretary.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.TrialSecretary"
        };
    }
}

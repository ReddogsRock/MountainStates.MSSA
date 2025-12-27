using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.TopDogs
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "TopDogs",
            Description = "Displays top dogs based on competition year, level, species",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.TopDogs.Manager.TopDogsManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.TopDogs.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.TopDogs",
            Categories = "Common",
            SettingsType = "MountainStates.MSSA.Module.TopDogs.Settings, MountainStates.MSSA.Client.Oqtane"
        };
    }
}

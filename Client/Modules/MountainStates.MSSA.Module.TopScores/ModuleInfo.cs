using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_TopScores
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Top Scores",
            Description = "Display top scoring dogs and handlers by year, level, and stock type",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_TopScores.Manager.MSSA_TopScoresManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.TopScores"
        };
    }
}
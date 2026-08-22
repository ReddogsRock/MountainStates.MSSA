using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Year End Standings",
            Description = "Year-end (and lifetime) point standings by Year, Level, and Species",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_YearEndStandings.Manager.MSSA_YearEndStandingsManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_YearEndStandings.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_YearEndStandings"
        };
    }
}

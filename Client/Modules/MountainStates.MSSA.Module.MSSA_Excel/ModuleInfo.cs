using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_Excel
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Excel Tools",
            Description = "Import entries, generate run orders, and import scores via Excel files",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Excel.Manager.MSSA_ExcelManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Entries.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Excel"
        };
    }
}

using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.BackOfficeEntry
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "BackOffice Entry",
            Description = "Back office result entry for herding trials - enter all results after an event",
            Version = "1.0.0"
        };
    }
}

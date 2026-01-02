using Oqtane.Models;
using Oqtane.Modules;
using System.Collections.Generic;

namespace MountainStates.MSSA.Module.MSSA_Handlers
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Handlers",
            Description = "Manage competition handlers/competitors for MSSA herding trials",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Handlers.Manager.MSSA_HandlerManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_Handlers.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_Handlers"
        };
    }
}
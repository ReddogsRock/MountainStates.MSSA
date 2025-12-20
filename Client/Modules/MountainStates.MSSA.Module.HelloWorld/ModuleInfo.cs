using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.HelloWorld
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "HelloWorld",
            Description = "My first test",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.HelloWorld.Manager.HelloWorldManager, MountainStates.MSSA.Server.Oqtane"
        };
    }
}

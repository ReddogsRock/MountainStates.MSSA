using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_MailingList
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Mailing List",
            Description = "Admin management of event-announcement mailing list subscribers",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Handlers.Manager.MSSA_MailingListManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_MailingList.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_MailingList"
        };
    }
}

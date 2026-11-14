using Oqtane.Models;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.MSSA_MailingListSubscribe
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "MSSA Mailing List Subscribe",
            Description = "Public signup form so site visitors can join the event-announcement mailing list",
            Version = "1.0.0",
            ServerManagerType = "MountainStates.MSSA.Module.MSSA_Handlers.Manager.MSSA_MailingListManager, MountainStates.MSSA.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "MountainStates.MSSA.Module.MSSA_MailingListSubscribe.Shared.Oqtane",
            PackageName = "MountainStates.MSSA.Module.MSSA_MailingListSubscribe"
        };
    }
}

using System.ComponentModel.DataAnnotations;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Models
{
    // Repopulates a named list's membership to exactly match everyone currently
    // holding the given Oqtane role (e.g. "Trial Secretary") - a full resync, same as
    // how "Sync Expiring Members" works: re-running it later picks up role changes
    // since the last sync (added/removed) rather than only ever adding people.
    public class SyncNamedListFromRoleDto
    {
        [Required]
        public string RoleName { get; set; }

        [Required]
        public int SiteId { get; set; }
    }
}

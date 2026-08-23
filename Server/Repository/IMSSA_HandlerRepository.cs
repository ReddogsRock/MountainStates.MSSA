using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public interface IMSSA_HandlerRepository
    {
        Task<IEnumerable<MSSA_Handler>> GetHandlersAsync(int moduleId);
        Task<MSSA_Handler> GetHandlerAsync(int handlerId);
        Task<MSSA_Handler> AddHandlerAsync(MSSA_Handler handler);
        Task<MSSA_Handler> UpdateHandlerAsync(MSSA_Handler handler);
        Task DeleteHandlerAsync(int handlerId);

        // Search and filter
        Task<IEnumerable<MSSA_Handler>> SearchHandlersAsync(
            string searchTerm = null,
            string stateCode = null,
            string handlerLevel = null);

        // Entries for detail view
        Task<IEnumerable<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId);

        // Memberships
        Task<List<MSSA_Membership>> GetHandlerMembershipsAsync(int handlerId);
        Task<MSSA_Membership> AddMembershipAsync(MSSA_Membership membership);
        Task<MSSA_Membership> UpdateMembershipAsync(MSSA_Membership membership);
        Task DeleteMembershipAsync(int membershipId);
        Task<List<MembershipMemberInfo>> AddMemberToMembershipAsync(int membershipId, int handlerId);
        Task<List<MembershipMemberInfo>> RemoveMemberFromMembershipAsync(int membershipId, int handlerId);
        Task<List<MSSA_Membership>> SearchMembershipsAsync(string filter, string searchTerm);
    }
}
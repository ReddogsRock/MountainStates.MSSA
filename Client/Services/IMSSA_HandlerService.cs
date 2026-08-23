using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Services
{
    public interface IMSSA_HandlerService
    {
        Task<List<MSSA_Handler>> GetHandlersAsync(int moduleId);
        Task<MSSA_Handler> GetHandlerAsync(int handlerId, int moduleId);
        Task<MSSA_Handler> AddHandlerAsync(MSSA_Handler handler, int moduleId);
        Task<MSSA_Handler> UpdateHandlerAsync(MSSA_Handler handler, int moduleId);
        Task DeleteHandlerAsync(int handlerId, int moduleId);

        Task<List<MSSA_Handler>> SearchHandlersAsync(
            string searchTerm,
            string stateCode,
            string handlerLevel,
            int moduleId);

        // Entries
        Task<List<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId, int moduleId);

        // Memberships
        Task<List<MSSA_Membership>> GetHandlerMembershipsAsync(int handlerId, int moduleId);
        Task<MSSA_Membership> AddMembershipAsync(MSSA_Membership membership, int moduleId);
        Task<MSSA_Membership> UpdateMembershipAsync(MSSA_Membership membership, int moduleId);
        Task DeleteMembershipAsync(int membershipId, int moduleId);
        Task<List<MembershipMemberInfo>> AddMemberToMembershipAsync(int membershipId, int handlerId, int moduleId);
        Task<List<MembershipMemberInfo>> RemoveMemberFromMembershipAsync(int membershipId, int handlerId, int moduleId);
        Task<List<MSSA_Membership>> SearchMembershipsAsync(string filter, string searchTerm, int moduleId);
    }
}
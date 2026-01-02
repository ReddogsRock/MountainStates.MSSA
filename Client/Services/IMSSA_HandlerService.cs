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
            bool? hasActiveMembership,
            int moduleId);

        // Memberships
        Task<List<MSSA_HandlerMembership>> GetHandlerMembershipsAsync(int handlerId, int moduleId);
        Task<MSSA_HandlerMembership> AddMembershipAsync(MSSA_HandlerMembership membership, int moduleId);
        Task<MSSA_HandlerMembership> UpdateMembershipAsync(MSSA_HandlerMembership membership, int moduleId);
        Task DeleteMembershipAsync(int membershipId, int moduleId);

        // Entries
        Task<List<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId, int moduleId);
    }
}
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
            string handlerLevel = null,
            bool? hasActiveMembership = null);

        // Memberships
        Task<IEnumerable<MSSA_HandlerMembership>> GetHandlerMembershipsAsync(int handlerId);
        Task<MSSA_HandlerMembership> AddMembershipAsync(MSSA_HandlerMembership membership);
        Task<MSSA_HandlerMembership> UpdateMembershipAsync(MSSA_HandlerMembership membership);
        Task DeleteMembershipAsync(int membershipId);

        // Entries for detail view
        Task<IEnumerable<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId);
    }
}
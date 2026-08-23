using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Handlers.Repository;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Manager
{
    public class MSSA_HandlerManager : IMSSA_HandlerManager, ITransientService
    {
        private readonly IMSSA_HandlerRepository _repository;

        public MSSA_HandlerManager(IMSSA_HandlerRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MSSA_Handler>> GetHandlersAsync(int moduleId)
        {
            return await _repository.GetHandlersAsync(moduleId);
        }

        public async Task<MSSA_Handler> GetHandlerAsync(int handlerId, int moduleId)
        {
            return await _repository.GetHandlerAsync(handlerId);
        }

        public async Task<MSSA_Handler> AddHandlerAsync(MSSA_Handler handler, int moduleId)
        {
            return await _repository.AddHandlerAsync(handler);
        }

        public async Task<MSSA_Handler> UpdateHandlerAsync(MSSA_Handler handler, int moduleId)
        {
            return await _repository.UpdateHandlerAsync(handler);
        }

        public async Task DeleteHandlerAsync(int handlerId, int moduleId)
        {
            await _repository.DeleteHandlerAsync(handlerId);
        }

        public async Task<IEnumerable<MSSA_Handler>> SearchHandlersAsync(
            string searchTerm,
            string stateCode,
            string handlerLevel,
            int moduleId)
        {
            return await _repository.SearchHandlersAsync(
                searchTerm,
                stateCode,
                handlerLevel);
        }

        // Entries
        public async Task<IEnumerable<MSSA_HandlerEntry>> GetHandlerEntriesAsync(int handlerId, int moduleId)
        {
            return await _repository.GetHandlerEntriesAsync(handlerId);
        }

        // Memberships
        public async Task<List<MSSA_Membership>> GetHandlerMembershipsAsync(int handlerId, int moduleId)
        {
            return await _repository.GetHandlerMembershipsAsync(handlerId);
        }

        public async Task<MSSA_Membership> AddMembershipAsync(MSSA_Membership membership, int moduleId)
        {
            return await _repository.AddMembershipAsync(membership);
        }

        public async Task<MSSA_Membership> UpdateMembershipAsync(MSSA_Membership membership, int moduleId)
        {
            return await _repository.UpdateMembershipAsync(membership);
        }

        public async Task DeleteMembershipAsync(int membershipId, int moduleId)
        {
            await _repository.DeleteMembershipAsync(membershipId);
        }

        public async Task<List<MembershipMemberInfo>> AddMemberToMembershipAsync(int membershipId, int handlerId, int moduleId)
        {
            return await _repository.AddMemberToMembershipAsync(membershipId, handlerId);
        }

        public async Task<List<MembershipMemberInfo>> RemoveMemberFromMembershipAsync(int membershipId, int handlerId, int moduleId)
        {
            return await _repository.RemoveMemberFromMembershipAsync(membershipId, handlerId);
        }

        public async Task<List<MSSA_Membership>> SearchMembershipsAsync(string filter, string searchTerm, int moduleId)
        {
            return await _repository.SearchMembershipsAsync(filter, searchTerm);
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public interface IMSSA_StateRepository
    {
        Task<IEnumerable<MSSA_State>> GetStatesAsync();
    }
}

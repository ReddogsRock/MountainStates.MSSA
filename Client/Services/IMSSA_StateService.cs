using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Services
{
    public interface IMSSA_StateService
    {
        Task<List<MSSA_State>> GetStatesAsync(int moduleId);
        Task<List<MSSA_State>> GetUSStatesAsync(int moduleId);
        Task<List<MSSA_State>> GetCanadianProvincesAsync(int moduleId);
    }
}
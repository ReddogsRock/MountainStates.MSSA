using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Repository
{
    public class MSSA_StateRepository : IMSSA_StateRepository, ITransientService
    {
        private readonly IDbContextFactory<MSSADbContext> _dbContextFactory;

        public MSSA_StateRepository(IDbContextFactory<MSSADbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<MSSA_State>> GetStatesAsync()
        {
            using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.MSSA_States
                .Where(s => s.IsActive)
                .OrderBy(s => s.Country)
                .ThenBy(s => s.StateName)
                .ToListAsync();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Repository;

namespace MountainStates.MSSA.Server.Controllers
{
    // NOTE: this controller previously had no route, methods, or base class at all -
    // just an empty class - so GET api/MSSA_State never actually existed. That's why
    // every States dropdown in the app came back empty regardless of the actual data.
    //
    // States are a simple, read-only lookup not owned by any particular module, so this
    // goes straight to the repository rather than adding a pass-through Manager layer
    // that wouldn't do anything - unlike every other module's Controller->Manager->
    // Repository chain. Add a Manager here later if you want strict consistency.
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_StateController : ModuleControllerBase
    {
        private readonly IMSSA_StateRepository _repository;

        public MSSA_StateController(IMSSA_StateRepository repository, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _repository = repository;
        }

        // GET: api/MSSA_State?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_State>> Get(int moduleId)
        {
            try
            {
                return await _repository.GetStatesAsync();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting states");
                throw;
            }
        }
    }
}

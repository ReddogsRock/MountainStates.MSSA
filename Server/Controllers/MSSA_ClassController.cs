using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Manager;
using MountainStates.MSSA.Module.MSSA_Entries.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_ClassController : ModuleControllerBase
    {
        private readonly IMSSA_EntryManager _manager;

        public MSSA_ClassController(IMSSA_EntryManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Class?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Class>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetClassesAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting classes");
                throw;
            }
        }

        // GET: api/MSSA_Class/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Class> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetClassAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting class {ClassId}", id);
                throw;
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace MountainStates.MSSA.Module.MSSA_Handlers.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_HandlerEntryController : ModuleControllerBase
    {
        private readonly IMSSA_HandlerManager _manager;

        public MSSA_HandlerEntryController(IMSSA_HandlerManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
             : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_HandlerEntry/handler/5?moduleid=x
        [HttpGet("handler/{handlerId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_HandlerEntry>> GetByHandler(int handlerId, int moduleId)
        {
            try
            {
                return await _manager.GetHandlerEntriesAsync(handlerId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entries for handler {HandlerId}", handlerId);
                throw;
            }
        }
    }
}
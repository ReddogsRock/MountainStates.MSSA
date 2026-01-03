using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Events.Manager;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;

namespace MountainStates.MSSA.Module.MSSA_Events.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_EventController : ModuleControllerBase
    {
        private readonly IMSSA_EventManager _manager;

        public MSSA_EventController(IMSSA_EventManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Event?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Event>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetEventsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting events");
                throw;
            }
        }

        // GET: api/MSSA_Event/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Event> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetEventAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting event {EventId}", id);
                throw;
            }
        }

        // GET: api/MSSA_Event/search?searchTerm=...&moduleId=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Event>> Search(
            string searchTerm = null,
            string stateCode = null,
            int? year = null,
            bool? cattle = null,
            bool? sheep = null,
            bool? arena = null,
            bool? field = null,
            bool? onFoot = null,
            bool? horseback = null,
            bool? open = null,
            bool? nursery = null,
            bool? intermediate = null,
            bool? novice = null,
            bool? junior = null,
            int moduleId = -1)
        {
            try
            {
                return await _manager.SearchEventsAsync(
                    searchTerm,
                    stateCode,
                    year,
                    cattle,
                    sheep,
                    arena,
                    field,
                    onFoot,
                    horseback,
                    open,
                    nursery,
                    intermediate,
                    novice,
                    junior,
                    moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching events");
                throw;
            }
        }

        // POST: api/MSSA_Event?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Event> Post([FromBody] MSSA_Event evt, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    evt = await _manager.AddEventAsync(evt, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Event added {Event}", evt);
                    return evt;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating event");
                throw;
            }
        }

        // PUT: api/MSSA_Event/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Event> Put(int id, [FromBody] MSSA_Event evt, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && evt.EventId == id && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    evt = await _manager.UpdateEventAsync(evt, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Event updated {Event}", evt);
                    return evt;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating event {EventId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Event/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteEventAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Event deleted {EventId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting event {EventId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

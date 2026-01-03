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
    public class MSSA_TrialController : ModuleControllerBase
    {
        private readonly IMSSA_EventManager _manager;

        public MSSA_TrialController(IMSSA_EventManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Trial/event/5?moduleid=x
        [HttpGet("event/{eventId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Trial>> GetByEvent(int eventId, int moduleId)
        {
            try
            {
                return await _manager.GetEventTrialsAsync(eventId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting trials for event {EventId}", eventId);
                throw;
            }
        }

        // GET: api/MSSA_Trial/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Trial> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetTrialAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting trial {TrialId}", id);
                throw;
            }
        }

        // POST: api/MSSA_Trial?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Trial> Post([FromBody] MSSA_Trial trial, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    trial = await _manager.AddTrialAsync(trial, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Trial added {Trial}", trial);
                    return trial;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized trial post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating trial");
                throw;
            }
        }

        // PUT: api/MSSA_Trial/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Trial> Put(int id, [FromBody] MSSA_Trial trial, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && trial.TrialId == id && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    trial = await _manager.UpdateTrialAsync(trial, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Trial updated {Trial}", trial);
                    return trial;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized trial put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating trial {TrialId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Trial/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteTrialAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Trial deleted {TrialId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized trial delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting trial {TrialId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

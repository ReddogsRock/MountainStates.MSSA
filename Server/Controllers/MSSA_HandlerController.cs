using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using Microsoft.AspNetCore.Http;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_HandlerController : ModuleControllerBase
    {
        private readonly IMSSA_HandlerManager _manager;

        public MSSA_HandlerController(IMSSA_HandlerManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Handler?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Handler>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetHandlersAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting handlers");
                throw;
            }
        }

        // GET: api/MSSA_Handler/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Handler> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetHandlerAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting handler {HandlerId}", id);
                throw;
            }
        }

        // GET: api/MSSA_Handler/search?searchTerm=smith&stateCode=CO&moduleId=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Handler>> Search(
            string searchTerm = null,
            string stateCode = null,
            string handlerLevel = null,
            bool? hasActiveMembership = null,
            int moduleId = -1)
        {
            try
            {
                return await _manager.SearchHandlersAsync(
                    searchTerm,
                    stateCode,
                    handlerLevel,
                    hasActiveMembership,
                    moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching handlers");
                throw;
            }
        }

        // POST: api/MSSA_Handler?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Handler> Post([FromBody] MSSA_Handler handler, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    handler = await _manager.AddHandlerAsync(handler, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Handler added {Handler}", handler);
                    return handler;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized handler post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating handler");
                throw;
            }
        }

        // PUT: api/MSSA_Handler/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Handler> Put(int id, [FromBody] MSSA_Handler handler, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && handler.HandlerId == id && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    handler = await _manager.UpdateHandlerAsync(handler, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Handler updated {Handler}", handler);
                    return handler;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized handler put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating handler {HandlerId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Handler/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteHandlerAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Handler deleted {HandlerId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized handler delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting handler {HandlerId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(Constants.AdminRole);
        }
    }
}
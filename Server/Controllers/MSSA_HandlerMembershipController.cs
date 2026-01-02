using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_HandlerMembershipController : ModuleControllerBase
    {
        private readonly IMSSA_HandlerManager _manager;

        public MSSA_HandlerMembershipController(IMSSA_HandlerManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
    : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_HandlerMembership/handler/5?moduleid=x
        [HttpGet("handler/{handlerId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_HandlerMembership>> GetByHandler(int handlerId, int moduleId)
        {
            try
            {
                return await _manager.GetHandlerMembershipsAsync(handlerId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting memberships for handler {HandlerId}", handlerId);
                throw;
            }
        }

        // POST: api/MSSA_HandlerMembership?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_HandlerMembership> Post([FromBody] MSSA_HandlerMembership membership, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    membership = await _manager.AddMembershipAsync(membership, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Membership added {Membership}", membership);
                    return membership;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized membership post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating membership");
                throw;
            }
        }

        // PUT: api/MSSA_HandlerMembership/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_HandlerMembership> Put(int id, [FromBody] MSSA_HandlerMembership membership, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && membership.MembershipId == id && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    membership = await _manager.UpdateMembershipAsync(membership, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Membership updated {Membership}", membership);
                    return membership;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized membership put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating membership {MembershipId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_HandlerMembership/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteMembershipAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Membership deleted {MembershipId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized membership delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting membership {MembershipId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(Constants.AdminRole);
        }
    }
}
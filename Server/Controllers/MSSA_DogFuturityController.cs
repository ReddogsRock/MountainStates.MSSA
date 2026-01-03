using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_DogFuturityController : ModuleControllerBase
    {
        private readonly IMSSA_DogManager _manager;

        public MSSA_DogFuturityController(IMSSA_DogManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_DogFuturity/dog/5?moduleid=x
        [HttpGet("dog/{dogId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_DogFuturityParticipation>> GetByDog(int dogId, int moduleId)
        {
            try
            {
                return await _manager.GetDogFuturityParticipationAsync(dogId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting futurity participation for dog {DogId}", dogId);
                throw;
            }
        }

        // POST: api/MSSA_DogFuturity?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_DogFuturityParticipation> Post([FromBody] MSSA_DogFuturityParticipation participation, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    participation = await _manager.AddFuturityParticipationAsync(participation, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Futurity participation added {Participation}", participation);
                    return participation;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized futurity participation post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating futurity participation");
                throw;
            }
        }

        // DELETE: api/MSSA_DogFuturity/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteFuturityParticipationAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Futurity participation deleted {ParticipationId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized futurity participation delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting futurity participation {ParticipationId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

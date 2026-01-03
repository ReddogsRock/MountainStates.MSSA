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
    public class MSSA_DogController : ModuleControllerBase
    {
        private readonly IMSSA_DogManager _manager;

        public MSSA_DogController(IMSSA_DogManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Dog?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Dog>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetDogsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting dogs");
                throw;
            }
        }

        // GET: api/MSSA_Dog/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Dog> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetDogAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting dog {DogId}", id);
                throw;
            }
        }

        // GET: api/MSSA_Dog/search?searchTerm=border&breed=Border Collie&moduleId=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Dog>> Search(
            string searchTerm = null,
            string breed = null,
            bool? ownerIsMember = null,
            bool? includeInactive = null,
            int moduleId = -1)
        {
            try
            {
                return await _manager.SearchDogsAsync(
                    searchTerm,
                    breed,
                    ownerIsMember,
                    includeInactive,
                    moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching dogs");
                throw;
            }
        }

        // POST: api/MSSA_Dog?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Dog> Post([FromBody] MSSA_Dog dog, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    dog = await _manager.AddDogAsync(dog, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Dog added {Dog}", dog);
                    return dog;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized dog post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating dog");
                throw;
            }
        }

        // PUT: api/MSSA_Dog/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Dog> Put(int id, [FromBody] MSSA_Dog dog, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && dog.DogId == id && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    dog = await _manager.UpdateDogAsync(dog, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Dog updated {Dog}", dog);
                    return dog;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized dog put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating dog {DogId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Dog/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteDogAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Dog deleted {DogId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized dog delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting dog {DogId}", id);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

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

namespace MountainStates.MSSA.Module.MSSA_Dogs.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_DogEntryController : ModuleControllerBase
    {
        private readonly IMSSA_DogManager _manager;

        public MSSA_DogEntryController(IMSSA_DogManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_DogEntry/dog/5?moduleid=x
        [HttpGet("dog/{dogId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_DogEntry>> GetByDog(int dogId, int moduleId)
        {
            try
            {
                return await _manager.GetDogEntriesAsync(dogId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entries for dog {DogId}", dogId);
                throw;
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.TrialSecretary.Manager;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using Oqtane.Extensions;

namespace MountainStates.MSSA.Module.TrialSecretary.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class TrialSecretaryController : ModuleControllerBase
    {
        private readonly ITrialSecretaryManager _manager;

        public TrialSecretaryController(ITrialSecretaryManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/TrialSecretary/recentevents?moduleid=x
        [HttpGet("recentevents")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<RecentEventDto>>> GetRecentEvents(int moduleId)
        {
            try
            {
                return Ok(await _manager.GetRecentEventsWithTrialsAsync(moduleId));
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting recent events");
                return StatusCode(500, "An error occurred while retrieving recent events");
            }
        }

        // GET: api/TrialSecretary/handlers/search?searchTerm=smith&moduleid=x
        [HttpGet("handlers/search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<HandlerSearchDto>>> SearchHandlers(string searchTerm, int moduleId)
        {
            try
            {
                return Ok(await _manager.SearchHandlersAsync(searchTerm, moduleId));
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching handlers");
                return StatusCode(500, "An error occurred while searching handlers");
            }
        }

        // GET: api/TrialSecretary/handlers/123?moduleid=x
        [HttpGet("handlers/{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<HandlerSearchDto>> GetHandlerById(int id, int moduleId)
        {
            try
            {
                var handler = await _manager.GetHandlerByIdAsync(id, moduleId);
                if (handler == null)
                    return NotFound($"Handler with ID {id} not found");
                return Ok(handler);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting handler {HandlerId}", id);
                return StatusCode(500, "An error occurred while retrieving the handler");
            }
        }

        // POST: api/TrialSecretary/handlers?moduleid=x
        [HttpPost("handlers")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<MSSA_Handler>> CreateHandler([FromBody] CreateHandlerDto handlerDto, int moduleId)
        {
            try
            {
                var handler = await _manager.CreateHandlerAsync(handlerDto, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Handler {HandlerId} created", handler.HandlerId);
                return Ok(handler);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating handler");
                return StatusCode(500, "An error occurred while creating the handler");
            }
        }

        // GET: api/TrialSecretary/dogs/search?searchTerm=max&moduleid=x
        [HttpGet("dogs/search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<DogSearchDto>>> SearchDogs(string searchTerm, int moduleId)
        {
            try
            {
                return Ok(await _manager.SearchDogsAsync(searchTerm, moduleId));
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching dogs");
                return StatusCode(500, "An error occurred while searching dogs");
            }
        }

        // GET: api/TrialSecretary/dogs/456?moduleid=x
        [HttpGet("dogs/{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<DogSearchDto>> GetDogById(int id, int moduleId)
        {
            try
            {
                var dog = await _manager.GetDogByIdAsync(id, moduleId);
                if (dog == null)
                    return NotFound($"Dog with ID {id} not found");
                return Ok(dog);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting dog {DogId}", id);
                return StatusCode(500, "An error occurred while retrieving the dog");
            }
        }

        // POST: api/TrialSecretary/dogs?moduleid=x
        [HttpPost("dogs")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<MSSA_Dog>> CreateDog([FromBody] CreateDogDto dogDto, int moduleId)
        {
            try
            {
                var dog = await _manager.CreateDogAsync(dogDto, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Dog {DogId} created", dog.DogId);
                return Ok(dog);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating dog");
                return StatusCode(500, "An error occurred while creating the dog");
            }
        }

        // POST: api/TrialSecretary/entries?moduleid=x
        [HttpPost("entries")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<List<int>>> CreateEntries([FromBody] CreateEntriesDto entriesDto, int moduleId)
        {
            try
            {
                var userId = User.UserId();
                var entryIds = await _manager.CreateEntriesAsync(entriesDto, moduleId, userId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Created {Count} entries", entryIds.Count);
                return Ok(entryIds);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating entries");
                return StatusCode(500, "An error occurred while creating entries");
            }
        }

        // GET: api/TrialSecretary/events/123/entries?moduleid=x
        [HttpGet("events/{eventId}/entries")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<EventEntriesSummaryDto>> GetEventEntries(int eventId, int moduleId)
        {
            try
            {
                var entries = await _manager.GetEventEntriesAsync(eventId, moduleId);
                if (entries == null)
                    return NotFound($"Event with ID {eventId} not found");
                return Ok(entries);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entries for event {EventId}", eventId);
                return StatusCode(500, "An error occurred while retrieving event entries");
            }
        }
    }
}

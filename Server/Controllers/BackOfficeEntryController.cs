using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.BackOfficeEntry.Manager;
using MountainStates.MSSA.Module.BackOfficeEntry.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Models;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.TrialSecretary.Models;
using System;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class BackOfficeEntryController : ModuleControllerBase
    {
        private readonly IBackOfficeEntryManager _manager;

        public BackOfficeEntryController(
            IBackOfficeEntryManager manager,
            ILogManager logger,
            IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET api/BackOfficeEntry/recentevents?moduleid=x
        [HttpGet("recentevents")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<RecentEventDto>>> GetRecentEvents(int moduleId)
        {
            try { return Ok(await _manager.GetRecentEventsWithTrialsAsync(moduleId)); }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting recent events");
                return StatusCode(500, "Error retrieving recent events");
            }
        }

        // GET api/BackOfficeEntry/handlers/search?searchTerm=x&moduleid=x
        [HttpGet("handlers/search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<HandlerSearchDto>>> SearchHandlers(string searchTerm, int moduleId)
        {
            try { return Ok(await _manager.SearchHandlersAsync(searchTerm, moduleId)); }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching handlers");
                return StatusCode(500, "Error searching handlers");
            }
        }

        // GET api/BackOfficeEntry/handlers/123?moduleid=x
        [HttpGet("handlers/{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<HandlerSearchDto>> GetHandlerById(int id, int moduleId)
        {
            try
            {
                var h = await _manager.GetHandlerByIdAsync(id, moduleId);
                return h == null ? NotFound() : Ok(h);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting handler {HandlerId}", id);
                return StatusCode(500, "Error retrieving handler");
            }
        }

        // POST api/BackOfficeEntry/handlers?moduleid=x
        [HttpPost("handlers")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<MSSA_Handler>> CreateHandler([FromBody] CreateHandlerDto dto, int moduleId)
        {
            try
            {
                var h = await _manager.CreateHandlerAsync(dto, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Handler {HandlerId} created", h.HandlerId);
                return Ok(h);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating handler");
                return StatusCode(500, "Error creating handler");
            }
        }

        // GET api/BackOfficeEntry/dogs/search?searchTerm=x&moduleid=x
        [HttpGet("dogs/search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<DogSearchDto>>> SearchDogs(string searchTerm, int moduleId)
        {
            try { return Ok(await _manager.SearchDogsAsync(searchTerm, moduleId)); }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching dogs");
                return StatusCode(500, "Error searching dogs");
            }
        }

        // GET api/BackOfficeEntry/dogs/456?moduleid=x
        [HttpGet("dogs/{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<DogSearchDto>> GetDogById(int id, int moduleId)
        {
            try
            {
                var d = await _manager.GetDogByIdAsync(id, moduleId);
                return d == null ? NotFound() : Ok(d);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting dog {DogId}", id);
                return StatusCode(500, "Error retrieving dog");
            }
        }

        // POST api/BackOfficeEntry/dogs?moduleid=x
        [HttpPost("dogs")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<MSSA_Dog>> CreateDog([FromBody] CreateDogDto dto, int moduleId)
        {
            try
            {
                var d = await _manager.CreateDogAsync(dto, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Dog {DogId} created", d.DogId);
                return Ok(d);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating dog");
                return StatusCode(500, "Error creating dog");
            }
        }

        // POST api/BackOfficeEntry/entries?moduleid=x  (create new)
        [HttpPost("entries")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<int>> CreateEntry([FromBody] SaveResultEntryDto dto, int moduleId)
        {
            try
            {
                dto.EntryId = 0;  // ensure create path
                var id = await _manager.SaveResultEntryAsync(dto, moduleId, User.UserId());
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "BackOffice entry {EntryId} created", id);
                return Ok(id);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating entry");
                return StatusCode(500, "Error creating entry");
            }
        }

        // POST api/BackOfficeEntry/entries/update?moduleid=x  (update existing)
        [HttpPost("entries/update")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<int>> UpdateEntry([FromBody] SaveResultEntryDto dto, int moduleId)
        {
            try
            {
                dto.EntryId = dto.EntryId > 0 ? dto.EntryId : throw new InvalidOperationException("EntryId required for update");
                var id = await _manager.SaveResultEntryAsync(dto, moduleId, User.UserId());
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "BackOffice entry {EntryId} updated", id);
                return Ok(id);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating entry");
                return StatusCode(500, "Error updating entry");
            }
        }

        // GET api/BackOfficeEntry/entries/123?moduleid=x
        [HttpGet("entries/{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<SaveResultEntryDto>> GetEntry(int id, int moduleId)
        {
            try
            {
                var e = await _manager.GetResultEntryAsync(id, moduleId);
                return e == null ? NotFound() : Ok(e);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entry {EntryId}", id);
                return StatusCode(500, "Error retrieving entry");
            }
        }

        // GET api/BackOfficeEntry/trials/1/classes/2/entries?moduleid=x
        [HttpGet("trials/{trialId}/classes/{classId}/entries")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<ResultEntryListItem>>> GetTrialClassEntries(
            int trialId, int classId, int moduleId)
        {
            try { return Ok(await _manager.GetTrialClassEntriesAsync(trialId, classId, moduleId)); }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex,
                    "Error getting entries for trial {TrialId} class {ClassId}", trialId, classId);
                return StatusCode(500, "Error retrieving entries");
            }
        }

        // POST api/BackOfficeEntry/trials/1/classes/2/finalize?moduleid=x
        [HttpPost("trials/{trialId}/classes/{classId}/finalize")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<FinalizeResultDto>> FinalizeTrialClass(
            int trialId, int classId, int moduleId)
        {
            try
            {
                var result = await _manager.FinalizeTrialClassAsync(trialId, classId, moduleId, User.UserId());
                _logger.Log(LogLevel.Information, this, LogFunction.Update,
                    "Finalized trial {TrialId} class {ClassId}: {Count} entries", trialId, classId, result.EntriesFinalized);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex,
                    "Error finalizing trial {TrialId} class {ClassId}", trialId, classId);
                return StatusCode(500, "Error finalizing entries");
            }
        }
    }
}

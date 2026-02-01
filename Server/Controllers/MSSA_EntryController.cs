using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Entries.Manager;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using MountainStates.MSSA.Module.MSSA_Events.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_EntryController : ModuleControllerBase
    {
        private readonly IMSSA_EntryManager _manager;

        public MSSA_EntryController(IMSSA_EntryManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Entry?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Entry>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetEntriesAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entries");
                throw;
            }
        }

        // GET: api/MSSA_Entry/trial/5?moduleid=x
        [HttpGet("trial/{trialId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<EntryListItem>> GetByTrial(int trialId, int moduleId)
        {
            try
            {
                return await _manager.GetTrialEntriesAsync(trialId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entries for trial {TrialId}", trialId);
                throw;
            }
        }

        // GET: api/MSSA_Entry/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Entry> Get(int id, int moduleId)
        {
            try
            {
                return await _manager.GetEntryAsync(id, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting entry {EntryId}", id);
                throw;
            }
        }

        // POST: api/MSSA_Entry?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Entry> Post([FromBody] MSSA_Entry entry, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && (IsAuthorizedForRole(MSSARoles.Admin) || IsAuthorizedForRole(MSSARoles.Scorekeeper)))
                {
                    entry = await _manager.AddEntryAsync(entry, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Entry added {Entry}", entry);
                    return entry;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized entry post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating entry");
                throw;
            }
        }

        // PUT: api/MSSA_Entry/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Entry> Put(int id, [FromBody] MSSA_Entry entry, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && entry.EntryId == id && (IsAuthorizedForRole(MSSARoles.Admin) || IsAuthorizedForRole(MSSARoles.Scorekeeper)))
                {
                    entry = await _manager.UpdateEntryAsync(entry, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Entry updated {Entry}", entry);
                    return entry;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized entry put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating entry {EntryId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Entry/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin) || IsAuthorizedForRole(MSSARoles.Scorekeeper))
                {
                    await _manager.DeleteEntryAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Entry deleted {EntryId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized entry delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting entry {EntryId}", id);
                throw;
            }
        }

        // POST: api/MSSA_Entry/generaterunorder?trialId=5&classId=3&moduleid=x
        [HttpPost("generaterunorder")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> GenerateRunOrder(int trialId, int classId, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin) || IsAuthorizedForRole(MSSARoles.Scorekeeper))
                {
                    await _manager.GenerateRunOrderAsync(trialId, classId, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Run order generated for trial {TrialId} class {ClassId}", trialId, classId);
                    return Ok();
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized run order generation attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return Forbid();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error generating run order");
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

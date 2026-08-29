using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.Extensions;
using System.Collections.Generic;
using System.Linq;
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
                if (ModelState.IsValid && await IsAuthorizedToCreateEntryAsync(entry, moduleId))
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
                var existing = await _manager.GetEntryAsync(id, moduleId);

                if (ModelState.IsValid && entry.EntryId == id && existing != null && IsAuthorizedForEntry(existing))
                {
                    // Trial cannot be changed after creation (enforced client-side too) -
                    // pin it server-side so a tampered payload can't move an entry into a
                    // different trial/event.
                    entry.TrialId = existing.TrialId;
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
                var existing = await _manager.GetEntryAsync(id, moduleId);

                if (IsAuthorizedForEntry(existing))
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

        // GET: api/MSSA_Entry/runorder/proposal/5?moduleid=x
        // Builds a proposed run order for the trial - does NOT persist anything.
        [HttpGet("runorder/proposal/{trialId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<RunOrderEntry>> GetProposedRunOrder(int trialId, int moduleId)
        {
            try
            {
                return await _manager.GetProposedRunOrderAsync(trialId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error building run order proposal for trial {TrialId}", trialId);
                throw;
            }
        }

        // POST: api/MSSA_Entry/runorder?moduleid=x
        // Persists a (possibly user-edited) run order.
        [HttpPost("runorder")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<List<RunOrderEntry>> SaveRunOrder([FromBody] List<RunOrderEntry> assignments, int moduleId)
        {
            try
            {
                assignments ??= new List<RunOrderEntry>();

                if (!await IsAuthorizedForRunOrderAsync(assignments, moduleId))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized run order save attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                var saved = await _manager.SaveRunOrderAsync(assignments, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Run order saved ({Count} entries)", saved.Count);
                return saved;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error saving run order");
                throw;
            }
        }

        // POST: api/MSSA_Entry/scores/import?moduleid=x
        [HttpPost("scores/import")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<List<ScoreImportRow>> ImportScores([FromBody] List<ScoreImportRow> rows, int moduleId)
        {
            try
            {
                if (!IsAuthorizedForRole(MSSARoles.Admin) && !IsAuthorizedForRole(MSSARoles.Scorekeeper))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized score import attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                var updatedRows = await _manager.ImportScoresAsync(rows ?? new List<ScoreImportRow>(), moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Scores imported ({Count} entries updated)", updatedRows.Count);
                return updatedRows;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error importing scores");
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }

        // Admin/Scorekeeper can add an entry to any trial. A Trial Secretary only to a
        // trial whose Event they own.
        private async Task<bool> IsAuthorizedToCreateEntryAsync(MSSA_Entry entry, int moduleId)
        {
            if (User.IsInRole(RoleNames.Admin) || User.IsInRole(MSSARoles.Scorekeeper))
            {
                return true;
            }

            if (!User.IsInRole(MSSARoles.TrialSecretary))
            {
                return false;
            }

            var ownerId = await _manager.GetEventOwnerForTrialAsync(entry.TrialId, moduleId);
            return ownerId.HasValue && ownerId.Value == User.UserId();
        }

        // Admin/Scorekeeper can save a run order for any trial. A Trial Secretary only
        // for trials whose Event they own - checked for every distinct trial referenced,
        // since a tampered payload could otherwise mix in entries from a trial they
        // don't own.
        private async Task<bool> IsAuthorizedForRunOrderAsync(List<RunOrderEntry> assignments, int moduleId)
        {
            if (User.IsInRole(RoleNames.Admin) || User.IsInRole(MSSARoles.Scorekeeper))
            {
                return true;
            }

            if (!User.IsInRole(MSSARoles.TrialSecretary) || !assignments.Any())
            {
                return false;
            }

            var trialIds = assignments.Select(a => a.TrialId).Distinct();
            foreach (var trialId in trialIds)
            {
                var ownerId = await _manager.GetEventOwnerForTrialAsync(trialId, moduleId);
                if (!ownerId.HasValue || ownerId.Value != User.UserId())
                {
                    return false;
                }
            }

            return true;
        }

        // Admin/Scorekeeper can edit/delete any entry. A Trial Secretary only an entry
        // whose Trial's Event they own - checked against the DB record, never the
        // request payload.
        private bool IsAuthorizedForEntry(MSSA_Entry existing)
        {
            if (User.IsInRole(RoleNames.Admin) || User.IsInRole(MSSARoles.Scorekeeper))
            {
                return true;
            }

            return existing != null
                && User.IsInRole(MSSARoles.TrialSecretary)
                && existing.EventCreatedByUserId.HasValue
                && existing.EventCreatedByUserId.Value == User.UserId();
        }
    }
}

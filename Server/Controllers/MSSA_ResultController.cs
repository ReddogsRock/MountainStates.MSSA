using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using MountainStates.MSSA.Module.MSSA_Results.Manager;
using MountainStates.MSSA.Module.MSSA_Results.Models;

namespace MountainStates.MSSA.Module.MSSA_Results.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_ResultController : ModuleControllerBase
    {
        private readonly IMSSA_ResultManager _manager;

        public MSSA_ResultController(IMSSA_ResultManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET api/MSSA_Result/events?moduleid=x
        // Admin/Scorekeeper see every event still needing attention; a Trial Secretary
        // only their own.
        [HttpGet("events")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<EventScoringSummary>> GetScoringEvents(int moduleId)
        {
            try
            {
                int? ownerUserId = IsAdminOrScorekeeper() ? (int?)null : User.UserId();
                return await _manager.GetScoringEventsAsync(ownerUserId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting scoring events");
                throw;
            }
        }

        // GET api/MSSA_Result/pending?moduleid=x  (Admin only - the approval queue)
        [HttpGet("pending")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<EventScoringSummary>>> GetPendingApprovalEvents(int moduleId)
        {
            try
            {
                if (!User.IsInRole(RoleNames.Admin))
                {
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);
                }

                return await _manager.GetPendingApprovalEventsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting pending approval events");
                throw;
            }
        }

        // GET api/MSSA_Result/trial/5/rows?moduleid=x
        [HttpGet("trial/{trialId}/rows")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<ResultRunRow>> GetTrialRunRows(int trialId, int moduleId)
        {
            try
            {
                return await _manager.GetTrialRunRowsAsync(trialId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting run rows for trial {TrialId}", trialId);
                throw;
            }
        }

        // POST api/MSSA_Result/trial/5/rows/save?moduleid=x
        [HttpPost("trial/{trialId}/rows/save")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> SaveResultRow(int trialId, [FromBody] SaveResultRowDto dto, int moduleId)
        {
            try
            {
                if (!await IsAuthorizedForTrialAsync(trialId, moduleId))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized result row save attempt for trial {TrialId}", trialId);
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);
                }

                await _manager.SaveResultRowAsync(dto, moduleId, User.UserId());
                return Ok(true);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error saving result row for entry {EntryId}", dto?.EntryId);
                throw;
            }
        }

        // POST api/MSSA_Result/trial/5/calculate?moduleid=x
        [HttpPost("trial/{trialId}/calculate")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> CalculatePlacingAndPoints(int trialId, int moduleId)
        {
            try
            {
                if (!await IsAuthorizedForTrialAsync(trialId, moduleId))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized calculate attempt for trial {TrialId}", trialId);
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);
                }

                await _manager.CalculatePlacingAndPointsAsync(trialId, moduleId, User.UserId());
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Placing/points calculated for trial {TrialId}", trialId);
                return Ok(true);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error calculating placing/points for trial {TrialId}", trialId);
                throw;
            }
        }

        // POST api/MSSA_Result/events/5/submit?moduleid=x
        [HttpPost("events/{eventId}/submit")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<SubmitEventResultsDto>> SubmitEventForApproval(int eventId, int moduleId)
        {
            try
            {
                if (!await IsAuthorizedForEventAsync(eventId, moduleId))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized submit-for-approval attempt for event {EventId}", eventId);
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);
                }

                var result = await _manager.SubmitEventForApprovalAsync(eventId, moduleId, User.UserId());
                if (result.Success)
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Event {EventId} submitted for approval", eventId);
                }
                return result;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error submitting event {EventId} for approval", eventId);
                throw;
            }
        }

        // POST api/MSSA_Result/events/5/approve?moduleid=x  (Admin only)
        [HttpPost("events/{eventId}/approve")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<SubmitEventResultsDto>> ApproveEvent(int eventId, int moduleId)
        {
            try
            {
                if (!User.IsInRole(RoleNames.Admin))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized approve attempt for event {EventId}", eventId);
                    return StatusCode((int)System.Net.HttpStatusCode.Forbidden);
                }

                var result = await _manager.ApproveEventAsync(eventId, moduleId, User.UserId());
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Event {EventId} approved", eventId);
                return result;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error approving event {EventId}", eventId);
                throw;
            }
        }

        private bool IsAdminOrScorekeeper()
        {
            return User.IsInRole(RoleNames.Admin) || User.IsInRole(MSSARoles.Scorekeeper);
        }

        // Admin/Scorekeeper can act on any trial. A Trial Secretary only on a trial
        // whose Event they own.
        private async Task<bool> IsAuthorizedForTrialAsync(int trialId, int moduleId)
        {
            if (IsAdminOrScorekeeper())
            {
                return true;
            }

            if (!User.IsInRole(MSSARoles.TrialSecretary))
            {
                return false;
            }

            var ownerId = await _manager.GetEventOwnerForTrialAsync(trialId, moduleId);
            return ownerId.HasValue && ownerId.Value == User.UserId();
        }

        private async Task<bool> IsAuthorizedForEventAsync(int eventId, int moduleId)
        {
            if (IsAdminOrScorekeeper())
            {
                return true;
            }

            if (!User.IsInRole(MSSARoles.TrialSecretary))
            {
                return false;
            }

            var ownerId = await _manager.GetEventOwnerAsync(eventId, moduleId);
            return ownerId.HasValue && ownerId.Value == User.UserId();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using Oqtane.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Events.Manager;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Events.Enums;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using MountainStates.MSSA.Module.MSSA_Entries.Models;
using MountainStates.MSSA.Module.MSSA_Results.Enums;
using MountainStates.MSSA.Module.MSSA_Results.Manager;
using System.Linq;
using Oqtane.Repository;
using MountainStates.MSSA.Server.Startup;

namespace MountainStates.MSSA.Module.MSSA_Events.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_EventController : ModuleControllerBase
    {
        private readonly IMSSA_EventManager _manager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IMSSA_ResultManager _resultManager;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMSSA_AdminNotificationService _adminNotificationService;

        public MSSA_EventController(IMSSA_EventManager manager, IWebHostEnvironment hostEnvironment, IMSSA_ResultManager resultManager, IUserRoleRepository userRoleRepository, IMSSA_AdminNotificationService adminNotificationService, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
            _hostEnvironment = hostEnvironment;
            _resultManager = resultManager;
            _userRoleRepository = userRoleRepository;
            _adminNotificationService = adminNotificationService;
        }

        // GET: api/MSSA_Event?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Event>> Get(int moduleId)
        {
            try
            {
                var events = await _manager.GetEventsAsync(moduleId);
                return await FilterVisibleEventsAsync(events, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting events");
                throw;
            }
        }

        // GET: api/MSSA_Event/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<MSSA_Event> Get(int id, int moduleId)
        {
            try
            {
                var evt = await _manager.GetEventAsync(id, moduleId);
                return await IsEventVisibleAsync(evt, moduleId) ? evt : null;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting event {EventId}", id);
                throw;
            }
        }

        // GET: api/MSSA_Event/search?searchTerm=...&moduleId=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Event>> Search(
            string searchTerm = null,
            string stateCode = null,
            int? year = null,
            bool? cattle = null,
            bool? sheep = null,
            bool? arena = null,
            bool? field = null,
            bool? onFoot = null,
            bool? horseback = null,
            bool? open = null,
            bool? nursery = null,
            bool? intermediate = null,
            bool? novice = null,
            bool? junior = null,
            int moduleId = -1)
        {
            try
            {
                var events = await _manager.SearchEventsAsync(
                    searchTerm,
                    stateCode,
                    year,
                    cattle,
                    sheep,
                    arena,
                    field,
                    onFoot,
                    horseback,
                    open,
                    nursery,
                    intermediate,
                    novice,
                    junior,
                    moduleId);
                return await FilterVisibleEventsAsync(events, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching events");
                throw;
            }
        }

        // GET: api/MSSA_Event/trial/5/entries?moduleid=x
        [HttpGet("trial/{trialId}/entries")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<EntryListItem>> GetTrialEntries(int trialId, int moduleId)
        {
            try
            {
                var entries = await _manager.GetTrialEntriesAsync(trialId, moduleId);

                // Only actively-pending results are hidden from the public - NotSubmitted
                // covers every event that predates this workflow (or simply never uses it),
                // and those must keep displaying exactly as they always have. Empty list
                // rather than Forbidden since this is a passive view (e.g. expanding a
                // trial row on the public Calendar), not an explicit action attempt.
                var first = entries.FirstOrDefault();
                if (first != null
                    && first.EventResultsApprovalStatus == EventResultsStatus.PendingApproval
                    && !await IsAuthorizedForEventAsync(first.EventCreatedByUserId, trialId, moduleId))
                {
                    return new List<EntryListItem>();
                }

                return entries;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting trial entries for {TrialId}", trialId);
                throw;
            }
        }

        // POST: api/MSSA_Event/5/sanctionfee/checkout?moduleid=x
        // Admin only - real money, and the association's sanctioning-fee accounting isn't
        // something a Trial Secretary should be able to trigger. Sanctioned Runs is always
        // recomputed here from actual scored entries, never trusted from the client -
        // only Unsanctioned Runs (inherently honor-system) comes from the request body.
        [HttpPost("{eventId}/sanctionfee/checkout")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<SanctionFeeCheckoutResult>> CreateSanctionFeeCheckout(int eventId, [FromBody] CreateSanctionFeeCheckoutDto dto, int moduleId)
        {
            try
            {
                if (dto == null || dto.EventId != eventId || dto.UnsanctionedRuns < 0
                    || string.IsNullOrEmpty(dto.SuccessUrl) || string.IsNullOrEmpty(dto.CancelUrl))
                {
                    return BadRequest();
                }

                var evt = await _manager.GetEventAsync(eventId, moduleId);
                if (evt == null)
                {
                    return NotFound();
                }

                // Admin can pay for any event; a Trial Secretary only the ones they
                // created - same rule as editing the event, so self-service pay is
                // limited to the person who'd know the run counts are right.
                if (!await IsAuthorizedForEventAsync(evt, moduleId))
                {
                    return Forbid();
                }

                var sanctionedRuns = await _resultManager.GetScoredRunCountAsync(eventId, moduleId);
                var quantity = sanctionedRuns + dto.UnsanctionedRuns;
                if (quantity <= 0)
                {
                    return BadRequest("No runs to charge a sanctioning fee for.");
                }

                var checkoutUrl = await _manager.CreateSanctioningFeeCheckoutSessionAsync(eventId, quantity, dto.SuccessUrl, dto.CancelUrl, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create,
                    "Sanctioning fee checkout session created for event {EventId}, quantity {Quantity}", eventId, quantity);

                return new SanctionFeeCheckoutResult { CheckoutUrl = checkoutUrl };
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating sanctioning fee checkout session for event {EventId}", eventId);
                throw;
            }
        }

        // For call sites that only have the entry/trial-level EventCreatedByUserId
        // projection on hand (not a full MSSA_Event), rather than the owner id - a
        // team member passes via trialId too.
        private async Task<bool> IsAuthorizedForEventAsync(int? eventOwnerUserId, int trialId, int moduleId)
        {
            if (User.IsInRole(RoleNames.Admin))
            {
                return true;
            }

            if (!User.IsInRole(MSSARoles.TrialSecretary))
            {
                return false;
            }

            if (eventOwnerUserId.HasValue && eventOwnerUserId.Value == User.UserId())
            {
                return true;
            }

            return await _manager.IsUserOnEventTeamForTrialAsync(trialId, User.UserId(), moduleId);
        }

        // GET: api/MSSA_Event/trialsecretaries?siteId=x&moduleid=x
        // Users holding the Trial Secretary role, for the Event edit form's team
        // picker - mirrors MSSA_TrialController.GetScorekeepers.
        [HttpGet("trialsecretaries")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public Task<IEnumerable<UserOptionDto>> GetTrialSecretaries(int siteId, int moduleId)
        {
            try
            {
                var userRoles = _userRoleRepository.GetUserRoles(siteId);
                var result = userRoles
                    .Where(ur => ur.Role.Name == MSSARoles.TrialSecretary)
                    .Select(ur => new UserOptionDto { UserId = ur.UserId, DisplayName = ur.User.DisplayName })
                    .OrderBy(u => u.DisplayName)
                    .ToList();

                return Task.FromResult<IEnumerable<UserOptionDto>>(result);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting trial secretaries");
                throw;
            }
        }

        // GET: api/MSSA_Event/5/team?siteId=x&moduleid=x
        [HttpGet("{eventId}/team")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<UserOptionDto>> GetEventTeam(int eventId, int siteId, int moduleId)
        {
            try
            {
                var memberIds = await _manager.GetEventTeamMemberUserIdsAsync(eventId, moduleId);
                var userRoles = _userRoleRepository.GetUserRoles(siteId);

                return userRoles
                    .Where(ur => memberIds.Contains(ur.UserId))
                    .Select(ur => new UserOptionDto { UserId = ur.UserId, DisplayName = ur.User.DisplayName })
                    .GroupBy(u => u.UserId).Select(g => g.First()) // a user can hold more than one role
                    .OrderBy(u => u.DisplayName)
                    .ToList();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting team for event {EventId}", eventId);
                throw;
            }
        }

        // POST: api/MSSA_Event/5/team/7?moduleid=x
        // Only the Event's creator (or an Admin) may add teammates - not the
        // teammates themselves, even once added.
        [HttpPost("{eventId}/team/{userId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> AddTeamMember(int eventId, int userId, int moduleId)
        {
            try
            {
                var existing = await _manager.GetEventAsync(eventId, moduleId);
                if (!IsCreatorOrAdmin(existing))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized team member add attempt for event {EventId}", eventId);
                    return Forbid();
                }

                await _manager.AddEventTeamMemberAsync(eventId, userId, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "User {UserId} added to team for event {EventId}", userId, eventId);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error adding team member {UserId} to event {EventId}", userId, eventId);
                throw;
            }
        }

        // DELETE: api/MSSA_Event/5/team/7?moduleid=x
        [HttpDelete("{eventId}/team/{userId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> RemoveTeamMember(int eventId, int userId, int moduleId)
        {
            try
            {
                var existing = await _manager.GetEventAsync(eventId, moduleId);
                if (!IsCreatorOrAdmin(existing))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized team member remove attempt for event {EventId}", eventId);
                    return Forbid();
                }

                await _manager.RemoveEventTeamMemberAsync(eventId, userId, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "User {UserId} removed from team for event {EventId}", userId, eventId);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error removing team member {UserId} from event {EventId}", userId, eventId);
                throw;
            }
        }

        // GET: api/MSSA_Event/5/offerings?moduleid=x
        [HttpGet("{eventId}/offerings")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<MSSA_EventClassOffering>> GetOfferings(int eventId, int moduleId)
        {
            try
            {
                return await _manager.GetEventOfferingsAsync(eventId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting offerings for event {EventId}", eventId);
                throw;
            }
        }

        // POST: api/MSSA_Event/5/offerings?moduleid=x
        // Replaces the full set of offerings for this event in one call.
        [HttpPost("{eventId}/offerings")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<List<MSSA_EventClassOffering>> SaveOfferings(int eventId, [FromBody] List<MSSA_EventClassOffering> offerings, int moduleId)
        {
            try
            {
                var existing = await _manager.GetEventAsync(eventId, moduleId);
                if (!await IsAuthorizedForEventAsync(existing, moduleId))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized offerings save attempt for event {EventId}", eventId);
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                var saved = await _manager.SaveEventOfferingsAsync(eventId, offerings ?? new List<MSSA_EventClassOffering>(), moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Offerings saved for event {EventId}", eventId);
                return saved;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error saving offerings for event {EventId}", eventId);
                throw;
            }
        }

        // POST: api/MSSA_Event?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Event> Post([FromBody] MSSA_Event evt, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.TrialSecretary))
                {
                    if (!IsValidFlyerUpload(evt))
                    {
                        HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                        return null;
                    }

                    // Owner is whoever creates the event - never trust a client-supplied value.
                    evt.CreatedByUserId = User.UserId();

                    // Admin-created events are approved immediately; anything created by
                    // a Trial Secretary starts Pending until an Admin approves it.
                    if (User.IsInRole(RoleNames.Admin))
                    {
                        evt.ApprovalStatus = EventApprovalStatus.Approved;
                        evt.ApprovedDate = DateTime.UtcNow;
                        evt.ApprovedByUserId = User.UserId();
                    }
                    else
                    {
                        evt.ApprovalStatus = EventApprovalStatus.Pending;
                        evt.ApprovedDate = null;
                        evt.ApprovedByUserId = null;
                    }

                    SaveFlyerIfPresent(evt);
                    evt = await _manager.AddEventAsync(evt, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Event added {Event}", evt);

                    if (evt.ApprovalStatus == EventApprovalStatus.Pending)
                    {
                        _adminNotificationService.NotifyAdmins(HttpContext.GetAlias().SiteId, $"Event Pending Approval: {evt.EventName}",
                            $"{evt.EventName} ({evt.EventIdentifier}) was submitted by a Trial Secretary and needs Admin approval before it's visible.");
                    }

                    return evt;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating event");
                throw;
            }
        }

        // PUT: api/MSSA_Event/5?moduleid=x
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Event> Put(int id, [FromBody] MSSA_Event evt, int moduleId)
        {
            try
            {
                var existing = await _manager.GetEventAsync(id, moduleId);

                if (ModelState.IsValid && evt.EventId == id && existing != null && await IsAuthorizedForEventAsync(existing, moduleId))
                {
                    if (!IsValidFlyerUpload(evt))
                    {
                        HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                        return null;
                    }

                    // Ownership is set at creation and never changes via edit, regardless of
                    // what the submitted payload contains.
                    evt.CreatedByUserId = existing.CreatedByUserId;

                    // Administrative Information (sanction fee, fee/results tracking) is
                    // Admin-only - a Trial Secretary can edit their own event's other
                    // fields, but these are locked to whatever's already in the DB
                    // regardless of what the submitted payload contains, same as
                    // CreatedByUserId above. The client already disables these inputs for
                    // a Trial Secretary; this is the actual enforcement.
                    if (!User.IsInRole(RoleNames.Admin))
                    {
                        evt.SanctionFee = existing.SanctionFee;
                        evt.FeeReceivedDate = existing.FeeReceivedDate;
                        evt.ResultsUploaded = existing.ResultsUploaded;
                        evt.ResultsReceivedDate = existing.ResultsReceivedDate;
                    }

                    SaveFlyerIfPresent(evt);
                    evt = await _manager.UpdateEventAsync(evt, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Event updated {Event}", evt);
                    return evt;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event put attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating event {EventId}", id);
                throw;
            }
        }

        // PUT: api/MSSA_Event/5/approve?moduleid=x
        // Admin-only - approving a Trial Secretary's event is the sign-off that makes
        // it visible beyond its creator.
        [HttpPut("{id}/approve")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Event> Approve(int id, int moduleId)
        {
            try
            {
                if (!User.IsInRole(RoleNames.Admin))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event approve attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                var evt = await _manager.ApproveEventAsync(id, User.UserId(), moduleId);
                if (evt == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Event {EventId} approved", id);
                return evt;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error approving event {EventId}", id);
                throw;
            }
        }

        // DELETE: api/MSSA_Event/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                var existing = await _manager.GetEventAsync(id, moduleId);

                if (await IsAuthorizedForEventAsync(existing, moduleId))
                {
                    await _manager.DeleteEventAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Event deleted {EventId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized event delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting event {EventId}", id);
                throw;
            }
        }

        // GET: api/MSSA_Event/5/flyer?moduleid=x - anyone browsing the public Calendar
        // needs to be able to download a flyer, logged in or not, so this can't require
        // the ViewModule permission check the rest of this controller uses.
        [HttpGet("{eventId}/flyer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFlyer(int eventId, int moduleId)
        {
            try
            {
                var evt = await _manager.GetEventAsync(eventId, moduleId);
                if (evt == null || string.IsNullOrEmpty(evt.FlyerPath) || !System.IO.File.Exists(evt.FlyerPath))
                {
                    return NotFound();
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(evt.FlyerPath);
                var contentType = GetContentType(evt.FlyerFileName);
                return File(bytes, contentType, evt.FlyerFileName);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error retrieving flyer for event {EventId}", eventId);
                throw;
            }
        }

        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        // Flyers are PDF-only. No upload this request (UploadFlyerContentBase64 empty) is
        // always valid - nothing to check. The client's accept filter and extension check
        // are UX only; this is the actual enforcement boundary.
        private static bool IsValidFlyerUpload(MSSA_Event evt)
        {
            if (string.IsNullOrEmpty(evt.UploadFlyerContentBase64))
            {
                return true;
            }

            return string.Equals(Path.GetExtension(evt.UploadFlyerFileName), ".pdf", StringComparison.OrdinalIgnoreCase);
        }

        // Writes an uploaded flyer to disk and sets FlyerFileName/FlyerPath on the event.
        // If no file was uploaded this request (UploadFlyerContentBase64 empty), the event's
        // existing FlyerFileName/FlyerPath - already set from the client's copy of the record -
        // pass through unchanged.
        private void SaveFlyerIfPresent(MSSA_Event evt)
        {
            if (string.IsNullOrEmpty(evt.UploadFlyerContentBase64))
            {
                return;
            }

            var bytes = Convert.FromBase64String(evt.UploadFlyerContentBase64);
            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(evt.UploadFlyerFileName)}";
            var folder = Path.Combine(_hostEnvironment.ContentRootPath, "Content", "MSSA_EventFlyers");
            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, safeFileName);
            System.IO.File.WriteAllBytes(fullPath, bytes);

            evt.FlyerFileName = evt.UploadFlyerFileName;
            evt.FlyerPath = fullPath;
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }

        // Who can manage an event's team roster - the creator or an Admin only, not
        // the teammates themselves (even once added). Narrower than
        // IsAuthorizedForEventAsync below on purpose.
        private bool IsCreatorOrAdmin(MSSA_Event existing)
        {
            if (User.IsInRole(RoleNames.Admin))
            {
                return true;
            }

            return existing != null
                && User.IsInRole(MSSARoles.TrialSecretary)
                && existing.CreatedByUserId.HasValue
                && existing.CreatedByUserId.Value == User.UserId();
        }

        // Admins can edit/delete any event. Trial Secretaries their own, or an event
        // they've been added to as a team member (see MSSA_EventTeamMembers) -
        // ownership/team membership is always checked against the DB record, never
        // the request payload.
        private async Task<bool> IsAuthorizedForEventAsync(MSSA_Event existing, int moduleId)
        {
            if (User.IsInRole(RoleNames.Admin))
            {
                return true;
            }

            if (existing == null || !User.IsInRole(MSSARoles.TrialSecretary))
            {
                return false;
            }

            if (existing.CreatedByUserId.HasValue && existing.CreatedByUserId.Value == User.UserId())
            {
                return true;
            }

            return await _manager.IsUserOnEventTeamAsync(existing.EventId, User.UserId(), moduleId);
        }

        // A Pending event is hidden from everyone except an Admin, the Trial
        // Secretary who created it, or a team member - same rule as who can edit it,
        // so this just reuses IsAuthorizedForEventAsync. Approved events are visible
        // to anyone with module view access, same as before this feature existed.
        private async Task<bool> IsEventVisibleAsync(MSSA_Event evt, int moduleId)
        {
            return evt != null && (evt.ApprovalStatus != EventApprovalStatus.Pending || await IsAuthorizedForEventAsync(evt, moduleId));
        }

        private async Task<IEnumerable<MSSA_Event>> FilterVisibleEventsAsync(IEnumerable<MSSA_Event> events, int moduleId)
        {
            var visible = new List<MSSA_Event>();
            foreach (var evt in events)
            {
                if (await IsEventVisibleAsync(evt, moduleId))
                {
                    visible.Add(evt);
                }
            }
            return visible;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Events.Manager;
using MountainStates.MSSA.Module.MSSA_Events.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using MountainStates.MSSA.Module.MSSA_Entries.Models;

namespace MountainStates.MSSA.Module.MSSA_Events.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_EventController : ModuleControllerBase
    {
        private readonly IMSSA_EventManager _manager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MSSA_EventController(IMSSA_EventManager manager, IWebHostEnvironment hostEnvironment, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: api/MSSA_Event?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_Event>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetEventsAsync(moduleId);
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
                return await _manager.GetEventAsync(id, moduleId);
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
                return await _manager.SearchEventsAsync(
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
                return await _manager.GetTrialEntriesAsync(trialId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting trial entries for {TrialId}", trialId);
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
                if (!IsAuthorizedForRole(MSSARoles.Admin))
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
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    SaveFlyerIfPresent(evt);
                    evt = await _manager.AddEventAsync(evt, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Event added {Event}", evt);
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
                if (ModelState.IsValid && evt.EventId == id && IsAuthorizedForRole(MSSARoles.Admin))
                {
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

        // DELETE: api/MSSA_Event/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
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

        // GET: api/MSSA_Event/5/flyer?moduleid=x
        [HttpGet("{eventId}/flyer")]
        [Authorize(Policy = PolicyNames.ViewModule)]
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
    }
}

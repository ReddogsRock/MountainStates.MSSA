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
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;
using MountainStates.MSSA.Module.MSSA_Dogs.Models;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_DogController : ModuleControllerBase
    {
        private readonly IMSSA_DogManager _manager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MSSA_DogController(IMSSA_DogManager manager, IWebHostEnvironment hostEnvironment, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
            _hostEnvironment = hostEnvironment;
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
        // Open to everyone (including anonymous visitors) per project decision:
        // there is currently no link between an authenticated person and dog
        // ownership, so gating "add" behind login wouldn't buy real protection -
        // only editing existing records requires authentication (see Put below).
        [HttpPost]
        [AllowAnonymous]
        public async Task<MSSA_Dog> Post([FromBody] MSSA_Dog dog, int moduleId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SaveNurseryDocumentIfPresent(dog);
                    dog = await _manager.AddDogAsync(dog, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Dog added {Dog}", dog);
                    return dog;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
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
        // Requires the caller to be logged in (any authenticated user, not
        // limited to Admin) since we can't yet verify ownership of the dog.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<MSSA_Dog> Put(int id, [FromBody] MSSA_Dog dog, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && dog.DogId == id)
                {
                    SaveNurseryDocumentIfPresent(dog);
                    dog = await _manager.UpdateDogAsync(dog, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Dog updated {Dog}", dog);
                    return dog;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating dog {DogId}", id);
                throw;
            }
        }

        // PUT: api/MSSA_Dog/5/status?moduleid=x
        // Open to everyone, per project decision - narrowly scoped to just
        // Active/Deceased so this doesn't also expose breed, registration number,
        // etc. to anonymous editing the way opening the full Put above would.
        [HttpPut("{id}/status")]
        [AllowAnonymous]
        public async Task<MSSA_Dog> UpdateStatus(int id, [FromBody] UpdateDogStatusDto dto, int moduleId)
        {
            try
            {
                if (dto == null || dto.DogId != id)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var dog = await _manager.UpdateDogStatusAsync(id, dto.IsActive, dto.IsDeceased, moduleId);
                if (dog == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Dog {DogId} status updated", id);
                return dog;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error updating status for dog {DogId}", id);
                throw;
            }
        }

        // POST: api/MSSA_Dog/5/transfer-ownership?moduleid=x
        // Open to everyone, same reasoning as UpdateStatus above.
        [HttpPost("{id}/transfer-ownership")]
        [AllowAnonymous]
        public async Task<MSSA_Dog> TransferOwnership(int id, [FromBody] TransferDogOwnershipDto dto, int moduleId)
        {
            try
            {
                if (dto == null || dto.DogId != id || string.IsNullOrWhiteSpace(dto.NewOwnerName))
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var dog = await _manager.TransferDogOwnershipAsync(id, dto.NewOwnerName.Trim(), moduleId);
                if (dog == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Dog {DogId} ownership transferred to {NewOwnerName}", id, dto.NewOwnerName);
                return dog;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error transferring ownership for dog {DogId}", id);
                throw;
            }
        }

        // POST: api/MSSA_Dog/merge?moduleid=x
        // Admin-only - unlike Status/OwnershipTransfer above, this repoints history
        // across Entries/Futurity/Ownership/Finals and deactivates a record, so it
        // isn't something to open up to anonymous use.
        [HttpPost("merge")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_Dog> Merge([FromBody] MergeDogsDto dto, int moduleId)
        {
            try
            {
                if (!IsAuthorizedForRole(MSSARoles.Admin))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized dog merge attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                if (dto == null || dto.KeepDogId <= 0 || dto.MergeDogId <= 0 || dto.KeepDogId == dto.MergeDogId)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return null;
                }

                var dog = await _manager.MergeDogsAsync(dto.KeepDogId, dto.MergeDogId, moduleId);
                if (dog == null)
                {
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return null;
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Dog {MergeDogId} merged into {KeepDogId}", dto.MergeDogId, dto.KeepDogId);
                return dog;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error merging dog {MergeDogId} into {KeepDogId}", dto?.MergeDogId, dto?.KeepDogId);
                throw;
            }
        }

        // DELETE: api/MSSA_Dog/5?moduleid=x
        // Left as Admin-only - not part of today's change.
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

        // Writes an uploaded Nursery age-eligibility document to disk and sets
        // NurseryDocumentFileName/Path on the dog. PDF-only is enforced client-side via
        // the file picker's accept filter, but re-checked here too since a client-side
        // filter can always be bypassed.
        private void SaveNurseryDocumentIfPresent(MSSA_Dog dog)
        {
            if (string.IsNullOrEmpty(dog.UploadNurseryDocContentBase64))
            {
                return;
            }

            if (!string.Equals(Path.GetExtension(dog.UploadNurseryDocFileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Nursery documentation must be a PDF file.");
            }

            var bytes = Convert.FromBase64String(dog.UploadNurseryDocContentBase64);
            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dog.UploadNurseryDocFileName)}";
            var folder = Path.Combine(_hostEnvironment.ContentRootPath, "Content", "MSSA_NurseryDocs");
            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, safeFileName);
            System.IO.File.WriteAllBytes(fullPath, bytes);

            dog.NurseryDocumentFileName = dog.UploadNurseryDocFileName;
            dog.NurseryDocumentPath = fullPath;
            dog.NurseryDocumentUploadedDate = DateTime.UtcNow;
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

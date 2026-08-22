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
    public class MSSA_DogFuturityController : ModuleControllerBase
    {
        private readonly IMSSA_DogManager _manager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MSSA_DogFuturityController(IMSSA_DogManager manager, IWebHostEnvironment hostEnvironment, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: api/MSSA_DogFuturity/dog/5?moduleid=x
        [HttpGet("dog/{dogId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_DogFuturityParticipation>> GetByDog(int dogId, int moduleId)
        {
            try
            {
                return await _manager.GetDogFuturityParticipationAsync(dogId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting futurity participation for dog {DogId}", dogId);
                throw;
            }
        }

        // POST: api/MSSA_DogFuturity?moduleid=x
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_DogFuturityParticipation> Post([FromBody] MSSA_DogFuturityParticipation participation, int moduleId)
        {
            try
            {
                if (ModelState.IsValid && IsAuthorizedForRole(MSSARoles.Admin))
                {
                    participation = await _manager.AddFuturityParticipationAsync(participation, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Futurity participation added {Participation}", participation);
                    return participation;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized futurity participation post attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error creating futurity participation");
                throw;
            }
        }

        // DELETE: api/MSSA_DogFuturity/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleId)
        {
            try
            {
                if (IsAuthorizedForRole(MSSARoles.Admin))
                {
                    await _manager.DeleteFuturityParticipationAsync(id, moduleId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Futurity participation deleted {ParticipationId}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized futurity participation delete attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error deleting futurity participation {ParticipationId}", id);
                throw;
            }
        }

        // POST: api/MSSA_DogFuturity/document?moduleid=x
        [HttpPost("document")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<MSSA_DogFuturityParticipation> UploadDocument([FromBody] MSSA_DogFuturityParticipation participation, int moduleId)
        {
            try
            {
                // Note: this payload only carries ParticipationId + the two Upload*
                // transient fields, so we don't run full ModelState validation here
                // (DogId/Year required-field rules don't apply to this endpoint).
                if (!IsAuthorizedForRole(MSSARoles.Admin) || string.IsNullOrEmpty(participation.UploadContentBase64))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized or empty futurity document upload attempt");
                    HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                var bytes = Convert.FromBase64String(participation.UploadContentBase64);
                var safeFileName = $"{participation.ParticipationId}_{Path.GetFileName(participation.UploadFileName)}";
                var folder = Path.Combine(_hostEnvironment.ContentRootPath, "Content", "MSSA_FuturityDocs");
                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, safeFileName);
                await System.IO.File.WriteAllBytesAsync(fullPath, bytes);

                var saved = await _manager.SaveFuturityDocumentAsync(participation.ParticipationId, participation.UploadFileName, fullPath, moduleId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Futurity document uploaded {ParticipationId}", participation.ParticipationId);
                return saved;
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error uploading futurity document {ParticipationId}", participation.ParticipationId);
                throw;
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

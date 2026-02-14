using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Excel.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Enums;
using Oqtane.Extensions;

namespace MountainStates.MSSA.Module.MSSA_Excel.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_ExcelController : ModuleControllerBase
    {
        private readonly IMSSA_ExcelManager _manager;

        public MSSA_ExcelController(IMSSA_ExcelManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // POST: api/MSSA_Excel/importentries?moduleid=x
        [HttpPost("importentries")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> ImportEntries(IFormFile file, int moduleId)
        {
            try
            {
                if (!IsAuthorizedForRole(MSSARoles.Admin) && !IsAuthorizedForRole(MSSARoles.Scorekeeper))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized import entries attempt");
                    return Forbid();
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                using (var stream = new System.IO.MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var fileData = stream.ToArray();

                    var result = await _manager.ImportEntriesAsync(fileData, moduleId, User.UserId());
                    
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, 
                        $"Entries imported: {result.RowsInserted} inserted, {result.RowsSkipped} skipped");
                    
                    return Ok(result);
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error importing entries");
                return StatusCode(500, $"Error importing entries: {ex.Message}");
            }
        }

        // GET: api/MSSA_Excel/generaterunorder/5?moduleid=x
        [HttpGet("generaterunorder/{trialId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GenerateRunOrder(int trialId, int moduleId)
        {
            try
            {
                if (!IsAuthorizedForRole(MSSARoles.Admin) && !IsAuthorizedForRole(MSSARoles.Scorekeeper))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized generate run order attempt");
                    return Forbid();
                }

                var fileData = await _manager.GenerateRunOrderAsync(trialId, moduleId);
                
                _logger.Log(LogLevel.Information, this, LogFunction.Read, 
                    $"Run order generated for trial {trialId}");
                
                return File(fileData, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"RunOrder_Trial{trialId}_{System.DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, 
                    $"Error generating run order for trial {trialId}");
                return StatusCode(500, $"Error generating run order: {ex.Message}");
            }
        }

        // POST: api/MSSA_Excel/importscores?moduleid=x
        [HttpPost("importscores")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> ImportScores(IFormFile file, int moduleId)
        {
            try
            {
                if (!IsAuthorizedForRole(MSSARoles.Admin) && !IsAuthorizedForRole(MSSARoles.Scorekeeper))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized import scores attempt");
                    return Forbid();
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                using (var stream = new System.IO.MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var fileData = stream.ToArray();

                    var result = await _manager.ImportScoresAsync(fileData, moduleId, User.UserId());
                    
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, 
                        $"Scores imported: {result.RowsUpdated} updated");
                    
                    return Ok(result);
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error importing scores");
                return StatusCode(500, $"Error importing scores: {ex.Message}");
            }
        }

        private bool IsAuthorizedForRole(string role)
        {
            return User.IsInRole(role) || User.IsInRole(RoleNames.Admin);
        }
    }
}

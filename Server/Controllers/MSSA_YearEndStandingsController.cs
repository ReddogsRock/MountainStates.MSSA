using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Manager;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Models;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_YearEndStandingsController : ModuleControllerBase
    {
        private readonly IMSSA_YearEndStandingsManager _manager;

        public MSSA_YearEndStandingsController(IMSSA_YearEndStandingsManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_YearEndStandings?year=2027&level=Open&species=Cattle&moduleid=x
        // Omit year (or pass nothing) for Lifetime standings across all years.
        // level is a ClassName ("Open", "Novice", etc.) or "Futurity".
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<MSSA_YearEndStanding>> Get(int? year, string level, string species, int moduleId)
        {
            try
            {
                return await _manager.GetStandingsAsync(year, level, species, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting year-end standings");
                throw;
            }
        }

        // GET: api/MSSA_YearEndStandings/details?dogId=5&handlerId=3&year=2027&level=Open&species=Cattle&moduleid=x
        [HttpGet("details")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<YearEndStandingDetail>> GetDetails(int dogId, int? handlerId, int? year, string level, string species, int moduleId)
        {
            try
            {
                return await _manager.GetStandingDetailsAsync(dogId, handlerId, year, level, species, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting year-end standing details for dog {DogId}", dogId);
                throw;
            }
        }

        // GET: api/MSSA_YearEndStandings/years?moduleid=x
        [HttpGet("years")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<int>> GetYears(int moduleId)
        {
            try
            {
                return await _manager.GetAvailableYearsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting available years");
                throw;
            }
        }

        // GET: api/MSSA_YearEndStandings/levels?moduleid=x
        [HttpGet("levels")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<string>> GetLevels(int moduleId)
        {
            try
            {
                return await _manager.GetLevelsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting levels");
                throw;
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainStates.MSSA.Module.MSSA_Finals.Manager;
using MountainStates.MSSA.Module.MSSA_Finals.Models;

namespace MountainStates.MSSA.Module.MSSA_Finals.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_FinalsController : ModuleControllerBase
    {
        private readonly IMSSA_FinalsManager _manager;

        public MSSA_FinalsController(IMSSA_FinalsManager manager, ILogManager logger, IHttpContextAccessor httpContextAccessor)
            : base(logger, httpContextAccessor)
        {
            _manager = manager;
        }

        // GET: api/MSSA_Finals?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_FinalsResult>> Get(int moduleId)
        {
            try
            {
                return await _manager.GetFinalsResultsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting finals results");
                throw;
            }
        }

        // GET: api/MSSA_Finals/search?year=2024&level=Open&stock=Cattle&moduleId=x
        [HttpGet("search")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_FinalsResult>> Search(
            int? year = null,
            string level = null,
            string stock = null,
            string round = null,
            int? place = null,
            int moduleId = -1)
        {
            try
            {
                return await _manager.SearchFinalsResultsAsync(
                    year,
                    level,
                    stock,
                    round,
                    place,
                    moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error searching finals results");
                throw;
            }
        }

        // GET: api/MSSA_Finals/years?moduleid=x
        [HttpGet("years")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<int>> GetYears(int moduleId)
        {
            try
            {
                return await _manager.GetAvailableYearsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting years");
                throw;
            }
        }

        // GET: api/MSSA_Finals/levels?moduleid=x
        [HttpGet("levels")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<string>> GetLevels(int moduleId)
        {
            try
            {
                return await _manager.GetAvailableLevelsAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting levels");
                throw;
            }
        }

        // GET: api/MSSA_Finals/stocks?moduleid=x
        [HttpGet("stocks")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<string>> GetStocks(int moduleId)
        {
            try
            {
                return await _manager.GetAvailableStocksAsync(moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting stocks");
                throw;
            }
        }

        // GET: api/MSSA_Finals/rounds?year=2024&level=Open&stock=Cattle&moduleId=x
        [HttpGet("rounds")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<string>> GetRounds(
            int? year = null,
            string level = null,
            string stock = null,
            int moduleId = -1)
        {
            try
            {
                return await _manager.GetAvailableRoundsAsync(year, level, stock, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting rounds");
                throw;
            }
        }

        // GET: api/MSSA_Finals/ultimate/breakdown?year=2024&level=Open&handlerId=1&dogId=2&moduleId=x
        [HttpGet("ultimate/breakdown")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<MSSA_UltimateBreakdown>> GetUltimateBreakdown(
            int year,
            string level,
            int handlerId,
            int dogId,
            int moduleId)
        {
            try
            {
                return await _manager.GetUltimateBreakdownAsync(year, level, handlerId, dogId, moduleId);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting ultimate breakdown");
                throw;
            }
        }
    }
}

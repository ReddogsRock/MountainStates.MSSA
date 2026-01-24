using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MountainStates.MSSA.Module.MSSA_TopScores.Manager;
using MountainStates.MSSA.Module.MSSA_TopScores.Models;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.MSSA_TopScores.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class MSSA_TopScoresController : Controller
    {
        private readonly IMSSA_TopScoresManager _manager;
        private readonly ILogManager _logger;

        public MSSA_TopScoresController(IMSSA_TopScoresManager manager, ILogManager logger)
        {
            _manager = manager;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]  // Changed from [Authorize]
        public async Task<IEnumerable<TopScoreResult>> Post(int entityId, [FromBody] TopScoreParameters parameters)
        {
            try
            {
                return await _manager.GetTopScoresAsync(entityId, parameters);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Error retrieving top scores: {Error}", ex.Message);
                return null;
            }
        }

        [HttpGet("years")]
        [Authorize(Policy = PolicyNames.EditModule)]  // Keep this restricted to editors only
        public async Task<IEnumerable<int>> GetYears(int entityId)
        {
            try
            {
                return await _manager.GetAvailableYearsAsync(entityId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Error retrieving available years: {Error}", ex.Message);
                return new List<int>();
            }
        }
    }
}
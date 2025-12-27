using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MountainStates.MSSA.Module.TopDogs.Manager;
using MountainStates.MSSA.Module.TopDogs.Models;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.TopDogs.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class TopDogsController : Controller
    {
        private readonly ITopDogsManager _manager;
        private readonly ILogManager _logger;

        public TopDogsController(ITopDogsManager manager, ILogManager logger)
        {
            _manager = manager;
            _logger = logger;
        }

        [HttpGet("list")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<TopDog>> GetList(int year, int level, string species, int quantity)
        {
            try
            {
                return await _manager.GetTopDogsAsync(year, level, species, quantity);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error getting top dogs");
                return new List<TopDog>();
            }
        }
    }
}
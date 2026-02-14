using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Shared;

namespace MountainStates.MSSA.Module.Health.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    [AllowAnonymous]
    [EnableCors("Default")]  // <-- Add this line
    public class HealthController : ControllerBase
    {
        // GET: api/Health
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = System.DateTime.UtcNow,
                message = "MSSA API is running"
            });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.HelloWorld.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.HelloWorld.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class HelloWorldController : ModuleControllerBase
    {
        private readonly IHelloWorldService _HelloWorldService;

        public HelloWorldController(IHelloWorldService HelloWorldService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _HelloWorldService = HelloWorldService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.HelloWorld>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _HelloWorldService.GetHelloWorldsAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.HelloWorld> Get(int id, int moduleid)
        {
            Models.HelloWorld HelloWorld = await _HelloWorldService.GetHelloWorldAsync(id, moduleid);
            if (HelloWorld != null && IsAuthorizedEntityId(EntityNames.Module, HelloWorld.ModuleId))
            {
                return HelloWorld;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Get Attempt {HelloWorldId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.HelloWorld> Post([FromBody] Models.HelloWorld HelloWorld)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, HelloWorld.ModuleId))
            {
                HelloWorld = await _HelloWorldService.AddHelloWorldAsync(HelloWorld);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Post Attempt {HelloWorld}", HelloWorld);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                HelloWorld = null;
            }
            return HelloWorld;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.HelloWorld> Put(int id, [FromBody] Models.HelloWorld HelloWorld)
        {
            if (ModelState.IsValid && HelloWorld.HelloWorldId == id && IsAuthorizedEntityId(EntityNames.Module, HelloWorld.ModuleId))
            {
                HelloWorld = await _HelloWorldService.UpdateHelloWorldAsync(HelloWorld);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Put Attempt {HelloWorld}", HelloWorld);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                HelloWorld = null;
            }
            return HelloWorld;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.HelloWorld HelloWorld = await _HelloWorldService.GetHelloWorldAsync(id, moduleid);
            if (HelloWorld != null && IsAuthorizedEntityId(EntityNames.Module, HelloWorld.ModuleId))
            {
                await _HelloWorldService.DeleteHelloWorldAsync(id, HelloWorld.ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Delete Attempt {HelloWorldId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}

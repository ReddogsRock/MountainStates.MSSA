using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using MountainStates.MSSA.Module.HelloWorld.Repository;

namespace MountainStates.MSSA.Module.HelloWorld.Services
{
    public class ServerHelloWorldService : IHelloWorldService
    {
        private readonly IHelloWorldRepository _HelloWorldRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerHelloWorldService(IHelloWorldRepository HelloWorldRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _HelloWorldRepository = HelloWorldRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.HelloWorld>> GetHelloWorldsAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_HelloWorldRepository.GetHelloWorlds(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.HelloWorld> GetHelloWorldAsync(int HelloWorldId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_HelloWorldRepository.GetHelloWorld(HelloWorldId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Get Attempt {HelloWorldId} {ModuleId}", HelloWorldId, ModuleId);
                return null;
            }
        }

        public Task<Models.HelloWorld> AddHelloWorldAsync(Models.HelloWorld HelloWorld)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, HelloWorld.ModuleId, PermissionNames.Edit))
            {
                HelloWorld = _HelloWorldRepository.AddHelloWorld(HelloWorld);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "HelloWorld Added {HelloWorld}", HelloWorld);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Add Attempt {HelloWorld}", HelloWorld);
                HelloWorld = null;
            }
            return Task.FromResult(HelloWorld);
        }

        public Task<Models.HelloWorld> UpdateHelloWorldAsync(Models.HelloWorld HelloWorld)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, HelloWorld.ModuleId, PermissionNames.Edit))
            {
                HelloWorld = _HelloWorldRepository.UpdateHelloWorld(HelloWorld);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "HelloWorld Updated {HelloWorld}", HelloWorld);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Update Attempt {HelloWorld}", HelloWorld);
                HelloWorld = null;
            }
            return Task.FromResult(HelloWorld);
        }

        public Task DeleteHelloWorldAsync(int HelloWorldId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _HelloWorldRepository.DeleteHelloWorld(HelloWorldId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "HelloWorld Deleted {HelloWorldId}", HelloWorldId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized HelloWorld Delete Attempt {HelloWorldId} {ModuleId}", HelloWorldId, ModuleId);
            }
            return Task.CompletedTask;
        }
    }
}

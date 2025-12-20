using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using MountainStates.MSSA.Module.HelloWorld.Repository;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.HelloWorld.Manager
{
    public class HelloWorldManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IHelloWorldRepository _HelloWorldRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public HelloWorldManager(IHelloWorldRepository HelloWorldRepository, IDBContextDependencies DBContextDependencies)
        {
            _HelloWorldRepository = HelloWorldRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new HelloWorldContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new HelloWorldContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.HelloWorld> HelloWorlds = _HelloWorldRepository.GetHelloWorlds(module.ModuleId).ToList();
            if (HelloWorlds != null)
            {
                content = JsonSerializer.Serialize(HelloWorlds);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.HelloWorld> HelloWorlds = null;
            if (!string.IsNullOrEmpty(content))
            {
                HelloWorlds = JsonSerializer.Deserialize<List<Models.HelloWorld>>(content);
            }
            if (HelloWorlds != null)
            {
                foreach(var HelloWorld in HelloWorlds)
                {
                    _HelloWorldRepository.AddHelloWorld(new Models.HelloWorld { ModuleId = module.ModuleId, Name = HelloWorld.Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var HelloWorld in _HelloWorldRepository.GetHelloWorlds(pageModule.ModuleId))
           {
               if (HelloWorld.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "MountainStates.MSSAHelloWorld",
                       EntityId = HelloWorld.HelloWorldId.ToString(),
                       Title = HelloWorld.Name,
                       Body = HelloWorld.Name,
                       ContentModifiedBy = HelloWorld.ModifiedBy,
                       ContentModifiedOn = HelloWorld.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}

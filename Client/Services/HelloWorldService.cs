using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace MountainStates.MSSA.Module.HelloWorld.Services
{
    public interface IHelloWorldService 
    {
        Task<List<Models.HelloWorld>> GetHelloWorldsAsync(int ModuleId);

        Task<Models.HelloWorld> GetHelloWorldAsync(int HelloWorldId, int ModuleId);

        Task<Models.HelloWorld> AddHelloWorldAsync(Models.HelloWorld HelloWorld);

        Task<Models.HelloWorld> UpdateHelloWorldAsync(Models.HelloWorld HelloWorld);

        Task DeleteHelloWorldAsync(int HelloWorldId, int ModuleId);
    }

    public class HelloWorldService : ServiceBase, IHelloWorldService
    {
        public HelloWorldService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("HelloWorld");

        public async Task<List<Models.HelloWorld>> GetHelloWorldsAsync(int ModuleId)
        {
            List<Models.HelloWorld> HelloWorlds = await GetJsonAsync<List<Models.HelloWorld>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.HelloWorld>().ToList());
            return HelloWorlds.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.HelloWorld> GetHelloWorldAsync(int HelloWorldId, int ModuleId)
        {
            return await GetJsonAsync<Models.HelloWorld>(CreateAuthorizationPolicyUrl($"{Apiurl}/{HelloWorldId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.HelloWorld> AddHelloWorldAsync(Models.HelloWorld HelloWorld)
        {
            return await PostJsonAsync<Models.HelloWorld>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, HelloWorld.ModuleId), HelloWorld);
        }

        public async Task<Models.HelloWorld> UpdateHelloWorldAsync(Models.HelloWorld HelloWorld)
        {
            return await PutJsonAsync<Models.HelloWorld>(CreateAuthorizationPolicyUrl($"{Apiurl}/{HelloWorld.HelloWorldId}", EntityNames.Module, HelloWorld.ModuleId), HelloWorld);
        }

        public async Task DeleteHelloWorldAsync(int HelloWorldId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{HelloWorldId}/{ModuleId}", EntityNames.Module, ModuleId));
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using MountainStates.MSSA.Module.HelloWorld.Services;

namespace MountainStates.MSSA.Module.HelloWorld.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IHelloWorldService)))
            {
                services.AddScoped<IHelloWorldService, HelloWorldService>();
            }
        }
    }
}

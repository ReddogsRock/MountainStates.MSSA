using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using MountainStates.MSSA.Module.TopDogs.Services;

namespace MountainStates.MSSA.Module.TopDogs.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(ITopDogsService)))
            {
                services.AddScoped<ITopDogsService, TopDogsService>();
            }
        }
    }
}

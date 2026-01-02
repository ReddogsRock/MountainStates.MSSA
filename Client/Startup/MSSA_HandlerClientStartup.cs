using Microsoft.Extensions.DependencyInjection;
using Oqtane.Services;
using MountainStates.MSSA.Module.MSSA_Handlers.Services;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Startup
{
    public class MSSA_HandlerClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register client services - Oqtane should auto-register IService implementations
            // But you can explicitly register if needed:
            services.AddScoped<IMSSA_HandlerService, MSSA_HandlerService>();
            services.AddScoped<IMSSA_StateService, MSSA_StateService>();
        }
    }
}

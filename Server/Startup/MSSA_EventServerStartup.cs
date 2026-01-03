using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Events.Repository;
using MountainStates.MSSA.Module.MSSA_Events.Manager;

namespace MountainStates.MSSA.Module.MSSA_Events.Startup
{
    public class MSSA_EventServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure middleware if needed
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // Configure MVC if needed
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register repositories
            services.AddTransient<IMSSA_EventRepository, MSSA_EventRepository>();

            // Register managers
            services.AddTransient<IMSSA_EventManager, MSSA_EventManager>();
        }
    }
}

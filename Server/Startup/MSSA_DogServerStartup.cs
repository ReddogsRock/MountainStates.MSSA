using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Dogs.Repository;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Startup
{
    public class MSSA_DogServerStartup : IServerStartup
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
            services.AddTransient<IMSSA_DogRepository, MSSA_DogRepository>();

            // Register managers
            services.AddTransient<IMSSA_DogManager, MSSA_DogManager>();
        }
    }
}

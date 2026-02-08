using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Finals.Repository;
using MountainStates.MSSA.Module.MSSA_Finals.Manager;

namespace MountainStates.MSSA.Module.MSSA_Finals.Startup
{
    public class MSSA_FinalsServerStartup : IServerStartup
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
            services.AddTransient<IMSSA_FinalsRepository, MSSA_FinalsRepository>();

            // Register managers
            services.AddTransient<IMSSA_FinalsManager, MSSA_FinalsManager>();
        }
    }
}

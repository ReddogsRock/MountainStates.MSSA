using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Entries.Repository;
using MountainStates.MSSA.Module.MSSA_Entries.Manager;

namespace MountainStates.MSSA.Module.MSSA_Entries.Startup
{
    public class MSSA_EntryServerStartup : IServerStartup
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
            services.AddTransient<IMSSA_EntryRepository, MSSA_EntryRepository>();

            // Register managers
            services.AddTransient<IMSSA_EntryManager, MSSA_EntryManager>();
        }
    }
}

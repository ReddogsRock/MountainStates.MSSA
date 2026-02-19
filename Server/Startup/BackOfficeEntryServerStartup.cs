using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MountainStates.MSSA.Module.BackOfficeEntry.Manager;
using MountainStates.MSSA.Module.BackOfficeEntry.Repository;
using Oqtane.Infrastructure;

namespace MountainStates.MSSA.Module.BackOfficeEntry.Startup
{
    public class BackOfficeEntryServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { }
        public void ConfigureMvc(IMvcBuilder mvcBuilder) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IBackOfficeEntryRepository, BackOfficeEntryRepository>();
            services.AddTransient<IBackOfficeEntryManager, BackOfficeEntryManager>();
        }
    }
}

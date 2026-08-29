using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Results.Manager;
using MountainStates.MSSA.Module.MSSA_Results.Repository;

namespace MountainStates.MSSA.Module.MSSA_Results.Startup
{
    public class MSSA_ResultServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IMSSA_ResultRepository, MSSA_ResultRepository>();
            services.AddTransient<IMSSA_ResultManager, MSSA_ResultManager>();
        }
    }
}

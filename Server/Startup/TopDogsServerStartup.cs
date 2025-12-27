using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.TopDogs.Repository;
using MountainStates.MSSA.Module.TopDogs.Services;
using Oqtane.Repository;

namespace MountainStates.MSSA.Module.TopDogs.Startup
{
    public class TopDogsServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<TenantDBContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MountainStates.MSSA.Module.TrialSecretary.Manager;
using MountainStates.MSSA.Module.TrialSecretary.Repository;
using Oqtane.Infrastructure;

namespace MountainStates.MSSA.Module.TrialSecretary.Startup
{
    public class TrialSecretaryServerStartup : IServerStartup
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
            services.AddTransient<ITrialSecretaryRepository, TrialSecretaryRepository>();
            services.AddTransient<ITrialSecretaryManager, TrialSecretaryManager>();
        }
    }
}

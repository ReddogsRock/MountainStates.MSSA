using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.HelloWorld.Repository;
using MountainStates.MSSA.Module.HelloWorld.Services;

namespace MountainStates.MSSA.Module.HelloWorld.Startup
{
    public class HelloWorldServerStartup : IServerStartup
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
            services.AddTransient<IHelloWorldService, ServerHelloWorldService>();
            services.AddDbContextFactory<HelloWorldContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}

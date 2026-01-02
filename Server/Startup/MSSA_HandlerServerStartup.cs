using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Handlers.Repository;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;

namespace MountainStates.MSSA.Module.MSSA_Handlers.Startup
{
    public class MSSA_HandlerServerStartup : IServerStartup
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
            // Register DbContext Factory
            services.AddDbContextFactory<MSSADbContext>((serviceProvider, options) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");

                options.UseSqlServer(connectionString);
            });

            // Register services - Oqtane should auto-register ITransientService implementations
            // But you can explicitly register if needed:
            services.AddTransient<IMSSA_HandlerRepository, MSSA_HandlerRepository>();
            services.AddTransient<IMSSA_StateRepository, MSSA_StateRepository>();

            // Register managers
            services.AddTransient<IMSSA_HandlerManager, MSSA_HandlerManager>();
        }
    }
}

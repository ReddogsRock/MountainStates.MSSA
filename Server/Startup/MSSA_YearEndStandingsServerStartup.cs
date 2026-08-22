using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Repository;
using MountainStates.MSSA.Module.MSSA_YearEndStandings.Manager;
using MountainStates.MSSA.Module.MSSA_Handlers.Data;

namespace MountainStates.MSSA.Module.MSSA_YearEndStandings.Startup
{
    public class MSSA_YearEndStandingsServerStartup : IServerStartup
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
            services.AddTransient<IMSSA_YearEndStandingsRepository, MSSA_YearEndStandingsRepository>();

            // Register managers
            services.AddTransient<IMSSA_YearEndStandingsManager, MSSA_YearEndStandingsManager>();
        }
    }
}

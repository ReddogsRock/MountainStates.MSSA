using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using MountainStates.MSSA.Module.MSSA_Excel.Manager;
using OfficeOpenXml;

namespace MountainStates.MSSA.Module.MSSA_Excel.Startup
{
    public class MSSA_ExcelServerStartup : IServerStartup
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
            // Register Excel Manager
            services.AddTransient<IMSSA_ExcelManager, MSSA_ExcelManager>();
        }
    }
}

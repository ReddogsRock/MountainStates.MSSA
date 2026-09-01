using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Dogs.Manager;
using MountainStates.MSSA.Module.MSSA_Dogs.Startup;
using MountainStates.MSSA.Module.MSSA_Handlers.Manager;

namespace MountainStates.MSSA.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AppDomain.CurrentDomain.SetData(Constants.DataDirectory, Path.Combine(builder.Environment.ContentRootPath, "Data"));

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            builder.Services.AddOqtane(configuration, builder.Environment);

            var app = builder.Build();

            var corsService = app.Services.GetRequiredService<ICorsService>();
            var corsPolicyProvider = app.Services.GetRequiredService<ICorsPolicyProvider>();
            var syncManager = app.Services.GetRequiredService<ISyncManager>();

            // Registered before UseOqtane deliberately: Oqtane's own pipeline (routing,
            // antiforgery, auth) is entirely set up inside that one call, with no
            // supported extension point for a module to run earlier. Stripe's webhook
            // needs to run before all of that - it's authenticated by verifying the
            // Stripe-Signature header (see StripeWebhookHandler), not by anything Oqtane
            // would recognize, and Oqtane's site-wide antiforgery check has no opt-out
            // that applies to it. This is the only point in the whole app that's
            // guaranteed to run first.
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/StripeWebhook") && context.Request.Method == "POST")
                {
                    try
                    {
                        // Plain ILogger, not Oqtane's ILogManager - ILogManager needs the
                        // current Site/Alias resolved, and that happens inside UseOqtane,
                        // which hasn't run yet here. A prior version of this used
                        // ILogManager and every failure was silently swallowed before it
                        // could even be logged - Console.WriteLine below is a deliberate
                        // second, unconditional fallback so that can never happen again.
                        var stripeService = context.RequestServices.GetRequiredService<IStripeService>();
                        var dogManager = context.RequestServices.GetRequiredService<IMSSA_DogManager>();
                        var handlerManager = context.RequestServices.GetRequiredService<IMSSA_HandlerManager>();
                        var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("StripeWebhook");

                        await StripeWebhookHandler.HandleAsync(context, stripeService, dogManager, handlerManager, logger);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[StripeWebhook] Unhandled exception: {ex}");
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }

                    return;
                }

                await next();
            });

            app.UseOqtane(configuration, builder.Environment, corsService, corsPolicyProvider, syncManager);

            var databaseManager = app.Services.GetService<IDatabaseManager>();
            var install = databaseManager.Install();
            if (!string.IsNullOrEmpty(install.Message))
            {
                var filelogger = app.Services.GetRequiredService<ILogger<Program>>();
                if (filelogger != null)
                {
                    filelogger.LogError($"[Oqtane.Server.Program.Main] {install.Message}");
                }
            }
            else
            {
                app.Run();
            }
        }
    }
}

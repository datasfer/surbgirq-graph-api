using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Surbgirq.GraphApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                    });
                    webBuilder.UseStartup<Startup>().CaptureStartupErrors(true);

                }).ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.logs.json", optional: true);

                }).UseSerilog((context, logger) => {
                    logger.ReadFrom
                        .Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                        .Enrich.WithProperty("Environment", context.HostingEnvironment);

#if DEBUG
                    // Used to filter out potentially bad data due debugging.
                    // Very useful when doing Seq dashboards and want to remove logs under debugging session.
                    logger.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
                });
    }
}

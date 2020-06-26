using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using Neo4jClient;
using Surbgirq.GraphApi.Configs;

namespace Surbgirq.GraphApi
{
    public class Startup
    {

        IWebHostEnvironment CurrentEnv;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            CurrentEnv = env;
            Configuration = configuration;
        }


        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddLogging();

            services.AddSwaggerGen(c =>
                 {
                     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Surbgirq graph db access API", Version = "v1" });
                 });

            var neo4jConfig = new Neo4jConfig();
            Configuration.GetSection(nameof(Neo4jConfig)).Bind(neo4jConfig);
            services.AddSingleton(GraphDatabase.Driver(neo4jConfig.InstanceUri, AuthTokens.None));

            var gc = new GraphClient(new Uri(neo4jConfig.InstanceUri));
            services.AddSingleton(gc);
            //services.AddSingleton(GraphDatabase.Driver(neo4jConfig.InstanceUri, AuthTokens.Basic("neo4j", "CHANGE_ME")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            

            var swaggerOptions = new SwaggerOptions();
            Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);

            app.UseSwagger(options => { options.RouteTemplate = swaggerOptions.JsonRoute; });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Description);
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

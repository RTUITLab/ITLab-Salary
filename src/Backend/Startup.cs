using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ITLab.Salary.Backend.Formatting;
using ITLab.Salary.Backend.Services.Configure;
using ITLab.Salary.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RTUITLab.AspNetCore.Configure.Configure;

namespace ITLab.Salary.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddTransient(s => new SalaryContext(
                s.GetRequiredService<IConfiguration>().GetConnectionString("MongoDb"),
                s.GetRequiredService<ILogger<SalaryContext>>()
                )
            );


            services.AddAutoMapper(typeof(Requests));


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Salary API", Version = "v1" });
            });

        services.AddWebAppConfigure()
                .AddTransientConfigure<MigrateMongoDbWork>(0);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(setup =>
            {
                setup.RouteTemplate = "/salary/swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "salary/swagger";
                c.SwaggerEndpoint("/salary/swagger/v1/swagger.json", "Salary API");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

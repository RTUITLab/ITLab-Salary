using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using ITLab.Salary.Backend.Formatting;
using ITLab.Salary.Backend.Models.Options;
using ITLab.Salary.Backend.Services;
using ITLab.Salary.Backend.Services.Configure;
using ITLab.Salary.Backend.Swagger;
using ITLab.Salary.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using RTUITLab.AspNetCore.Configure.Configure;
using RTUITLab.AspNetCore.Configure.Invokations;
using Swashbuckle.AspNetCore.SwaggerGen;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1822 // Mark members as static

namespace ITLab.Salary.Backend
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private bool IsTests => Configuration.GetValue<bool>("TESTS");

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));

            services.AddControllers(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .AddAuthenticationSchemes("Bearer")
                     .RequireClaim("scope", Configuration.GetSection(nameof(JwtOptions)).GetValue<string>(nameof(JwtOptions.Scope)))
                     .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddSingleton<IDbFactory, ConcurrentDictionaryDbFactory>();
            services.AddTransient<EventSalaryContext>();

            services.AddAutoMapper(typeof(Requests));


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtOptions = Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
                    options.Audience = jwtOptions.Audience;
                    options.TokenValidationParameters.ValidateAudience = true;
                    if (IsTests)
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.IssuerSigningKey = JwtTestsHelper.IssuerSigningKey(jwtOptions.DebugKey);
                        options.TokenValidationParameters.ValidateLifetime = false;
                        options.TokenValidationParameters.ValidateIssuer = false;
                    }
                    else
                    {
                        options.Authority = jwtOptions.Authority;
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters.ValidateLifetime = true;
                    }
                });

            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                });
            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                options =>
                {
                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    // integrate xml comments
                    options.IncludeXmlComments(XmlCommentsFilePath);
                });

            services.AddWebAppConfigure()
                .AddTransientConfigure<MigrateMongoDbWork>(0)
                .AddTransientConfigure<ShowTestAdminTokenWork>(IsTests, 1);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebAppConfigure();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger(setup =>
            {
                setup.RouteTemplate = "/salary/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "salary/swagger";
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/salary/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }
        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA1822 // Mark members as static

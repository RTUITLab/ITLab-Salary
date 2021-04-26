using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel.Client;
using ITLab.Salary.Backend.Authorization;
using ITLab.Salary.Backend.Models.Options;
using ITLab.Salary.Backend.Services;
using ITLab.Salary.Backend.Services.Configure;
using ITLab.Salary.Backend.Swagger;
using ITLab.Salary.Database;
using ITLab.Salary.Mappings;
using ITLab.Salary.Services.Events;
using ITLab.Salary.Services.Events.Remote;
using ITLab.Salary.Services.Reports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.CodeAnalysis.Options;
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
        readonly string AllowAllOrigins = "_myAllowSpecificOrigins";

        public IConfiguration Configuration { get; }

        private bool IsTests => Configuration.GetValue<bool>("TESTS");

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtOptions>(Configuration.GetSection(nameof(JwtOptions)));

            services.AddControllers();

            services.AddSingleton<IDbFactory, ConcurrentDictionaryDbFactory>();
            services.AddTransient<EventSalaryContext>();
            services.AddTransient<ReportSalaryContext>();

            switch (Configuration.GetValue<EventsServiceType>(nameof(EventsServiceType)))
            {
                case EventsServiceType.SelfReferenced:
                    services.AddScoped<IEventsService, SelfReferencedEventsService>();
                    break;
                case EventsServiceType.FromEventsApi:

                    var options = Configuration
                        .GetSection(nameof(RemoteApiEventsServiceOptions))
                        .Get<RemoteApiEventsServiceOptions>();

                    services.AddAccessTokenManagement(atmo =>
                    {
                        atmo.Client.Clients.Add("identityserver", new ClientCredentialsTokenRequest
                        {
                            Address = options.TokenUrl,
                            ClientId = "itlab_salary",
                            Scope = "itlab.events",
                            ClientSecret = options.ClientSecret
                        });
                    });

                    services.AddClientAccessTokenClient(RemoteApiEventsService.HttpClientName, configureClient: client =>
                    {
                        client.BaseAddress = new Uri(options.BaseUrl);
                    });
                    services.AddScoped<IEventsService, RemoteApiEventsService>();
                    break;
                default:
                    throw new ApplicationException($"Incorrect {nameof(EventsServiceType)}");
            }
            services.AddScoped<IEventSalaryService, EventSalaryService>();

            services.AddHttpClient(WithReportsApiSalaryService.HTTP_CLIENT_NAME, client =>
            {
                var options = Configuration
                                .GetSection(nameof(RemoteApiReportsServiceOptions))
                                .Get<RemoteApiReportsServiceOptions>();

                client.BaseAddress = new Uri(options.BaseUrl);
            });
            services.AddScoped<IReportSalaryService, WithReportsApiSalaryService>();

            services.AddAutoMapper(typeof(Requests));


            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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

            services.AddAuthorization(options =>
            {
                var defaultPolicy = new AuthorizationPolicyBuilder()
                     .RequireClaim("scope", "itlab.salary")
                     .RequireClaim("itlab", "user")
                     .RequireClaim("sub")
                     .Build();
                options.DefaultPolicy = defaultPolicy;

                options.AddSalaryAdminPolicy();
                options.AddReportsAdminPolicy();
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
                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    };
                    options.AddSecurityDefinition("Bearer", securityScheme);
                    var securityRequirement = new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    };
                    options.AddSecurityRequirement(securityRequirement);

                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    // integrate xml comments
                    options.IncludeXmlComments(XmlCommentsFilePath);
                });

            services.AddWebAppConfigure()
                .AddTransientConfigure<MigrateMongoDbWork>(0)
                .AddTransientConfigure<ShowTestAdminTokenWork>(IsTests, 1);

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(AllowAllOrigins);
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
                setup.RouteTemplate = "/api/salary/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api/salary/swagger";
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/api/salary/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
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

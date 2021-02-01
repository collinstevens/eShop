using AutoMapper;
using Cart.Api.Data;
using Core.Api;
using Core.Api.Serilog;
using Core.Api.Utility;
using FluentValidation.AspNetCore;
using Grpc.Core;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System;
using static Grpc.Core.Metadata;

namespace Cart.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.AddControllers()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>(lifetime: ServiceLifetime.Singleton));

            services.AddAutoMapper(typeof(Startup));

            services.AddDbContextPool<CartContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddElasticsearch(Configuration["ElasticConfiguration:Uri"])
                .AddSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart API", Version = "v1" });
            });

            services.AddOpenTelemetryTracing((builder) => builder
              .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Configuration["Otlp:ServiceName"]))
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddOtlpExporter(options =>
              {
                  options.Endpoint = new Uri(Configuration["Otlp:Endpoint"]);
                  options.Headers = new Metadata
                  {
                      new Entry("x-honeycomb-team", Configuration["HoneycombSettings:TeamId"]),
                      new Entry("x-honeycomb-dataset", Configuration["HoneycombSettings:DefaultDataSet"]),
                  };
              }));

            services.AddSingleton<IClock, Clock>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IMapper mapper)
        {
            if (app is null)
                throw new ArgumentNullException(nameof(app));

            if (env is null)
                throw new ArgumentNullException(nameof(env));

            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));

            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            Shared.LoggerFactory = loggerFactory;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API v1"));
            }

            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest;
                options.GetLevel = LogHelper.ExcludeHealthChecks;
            });

            var supportedCultures = new[] { "en-US" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}

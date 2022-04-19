using Demo.MedTech.Api.Application.Filters;
using Demo.MedTech.Api.Extensions;
using Demo.MedTech.Utility.Converter;
using Demo.MedTech.ValidationEngine.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace Demo.MedTech.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.MapConfigurationToTypedClass(Configuration);
            services.AddInternalServices(Configuration);
            services.RegisterRuleValidationEngine();
            services.AddApiVersioning(1);

            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new SbsDateTimeConverter()));
            services.AddHeaderPropagation();

            services.AddSwaggerGen(swaggerOptions =>
            {
                swaggerOptions.CustomSchemaIds(x => x.FullName);
                swaggerOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "Core Api", Version = "v1" });
            });

            services.AddGzipCompression();
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.RegisterSwagger();
            app.RegisterMiddleware();

            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
using Demo.MedTech.Api.Helpers;
using Demo.MedTech.DAL;
using Demo.MedTech.DAL.Cosmos;
using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Service;
using Demo.MedTech.Utility.Extension;
using Demo.MedTech.Utility.Helper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace Demo.MedTech.Api.Extensions
{
    /// <summary>
    /// Extension methods on IServiceCollection.
    /// Other required extension methods for IServiceCollection can be created in this class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register api versioning here.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="majorVersion">Major version of api</param>
        internal static void AddApiVersioning(this IServiceCollection services, int majorVersion)
        {
            services.AddApiVersioning(c =>
            {
                c.DefaultApiVersion = new ApiVersion(majorVersion, 0);
                c.AssumeDefaultVersionWhenUnspecified = true;
                c.ReportApiVersions = true;
                c.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
        }

        /// <summary>
        /// Register all internal dependencies i.e helpers, wrappers, etc. here.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        internal static void AddInternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Request pipe line
            services.AddScoped(typeof(IRequestPipe), typeof(RequestPipe));

            //Register compression decompression
            services.RegisterGzipCompressorDeCompressor();

            services.AddSingleton<ILotDataAccess, CosmosDataAccess>();
            services.AddScoped<IAuctioneerService, AuctioneerService>();

            //Used for SBS Egress
            services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();
        }

        /// <summary>
        /// Map configuration related to AppSettings, Azure AppConfig, etc. here.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        internal static void MapConfigurationToTypedClass(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind app config values in case of static classes.
            services.Configure<SbsConfigurationOptions>(configuration.GetSection(nameof(SbsConfigurationOptions)));
        }

        /// <summary>
        /// Configure Gzip response compression here
        /// </summary>
        /// <param name="services"></param>
        internal static void AddGzipCompression(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
        }
    }
}
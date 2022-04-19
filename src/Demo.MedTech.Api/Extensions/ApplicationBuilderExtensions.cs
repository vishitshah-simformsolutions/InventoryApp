using System.Diagnostics.CodeAnalysis;
using Demo.MedTech.Api.Application.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Demo.MedTech.Api.Extensions
{
    /// <summary>
    /// Extension methods on IApplicationBuilder.
    /// Other required extension methods for IApplicationBuilder can be created in this class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Register all custom middleware for IApplicationBuilder here.
        /// </summary>
        /// <param name="app"></param>
        internal static void RegisterMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            //app.UseMiddleware<HeaderValidationMiddleware>();
        }

        /// <summary>
        /// Register swagger and its options here.
        /// </summary>
        /// <param name="app"></param>
        internal static void RegisterSwagger(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a json endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui.
            app.UseSwaggerUI(swaggerOptions =>
            {
                swaggerOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo.MedTech.Api v1");
                swaggerOptions.DisplayRequestDuration();
            });
        }
    }
}
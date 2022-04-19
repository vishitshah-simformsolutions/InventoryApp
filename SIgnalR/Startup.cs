using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using SIgnalR.Model;
using SIgnalR.Service;
using SIgnalR.Service.IService;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace SIgnalR
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIgnalR", Version = "v1" });
            });

            services.AddSignalR().AddAzureSignalR(Configuration["ApiConfigs:SignalR:Uri"]);

            services.Add(new ServiceDescriptor(typeof(IMessageHandler), typeof(MessageHandler), ServiceLifetime.Transient)); // Transient
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpClient<IPlaceBid, PlaceBidHandler>(c =>
                c.BaseAddress = new Uri(Configuration["ApiConfigs:Bid:UriQA"]))
                 .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            services.AddSingleton<ConfigurationLoader>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase(new PathString(Configuration["BasePath"]));

            //if (env.IsDevelopment())
            //{
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIgnalR v1"));
            // }


            app.UseStaticFiles();
            app.UseHttpsRedirection();


            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<ConnectionHub>(Configuration["ConnectionHubPath"]);
            });

        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(1.5, retryAttempt) * 1000),
                    (_, waitingTime) =>
                    {
                        Console.WriteLine("Retrying due to Polly retry policy");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(15));
        }

        public class ConfigurationLoader
        {

            public static string PublishDetailsJson = File.ReadAllText("Model\\PublishDetails.json");

            public static BidResponse PublishData;

            static ConfigurationLoader()
            {
                PublishData = JsonSerializer.Deserialize<BidResponse>(PublishDetailsJson);
            }
        }
    }
}

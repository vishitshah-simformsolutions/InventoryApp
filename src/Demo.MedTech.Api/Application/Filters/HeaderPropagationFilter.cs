using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Demo.MedTech.Api.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Demo.MedTech.Api.Application.Filters
{
    [ExcludeFromCodeCoverage]
    public static class HeaderPropagationExtensions
    {
        public static void AddHeaderPropagation(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HeaderPropagationMessageHandlerBuilderFilter>());
        }
    }

    [ExcludeFromCodeCoverage]
    internal class HeaderPropagationMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly Headers _headersOptions;
        private readonly IHttpContextAccessor _contextAccessor;

        public HeaderPropagationMessageHandlerBuilderFilter(IOptions<Headers> headersOptions, IHttpContextAccessor contextAccessor)
        {
            _headersOptions = headersOptions.Value;
            _contextAccessor = contextAccessor;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                builder.AdditionalHandlers.Add(new HeaderPropagationMessageHandler(_headersOptions, _contextAccessor, builder.Name));
                next(builder);
            };
        }
    }

    /// <summary>
    /// Pass the current request headers to the next service being called using http.
    /// When another service is called from this service, this handler enables passing of the mandatory headers to the next handler on the basis of the service being present in the HttpClients section of configuration (Internal service).
    /// DO NOT MODIFY THE CONTENTS OF THIS FILE.
    /// Changes to this file may cause incorrect behavior.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HeaderPropagationMessageHandler : DelegatingHandler
    {
        private readonly Headers _headersOptions;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _builderName;

        public HeaderPropagationMessageHandler(Headers options, IHttpContextAccessor contextAccessor, string builderName)
        {
            _headersOptions = options;
            _contextAccessor = contextAccessor;
            _builderName = builderName;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var currentActivity = System.Diagnostics.Activity.Current;
            if (_contextAccessor.HttpContext != null)
            {
                foreach (var headerName in _headersOptions.Request)
                {
                    var headerValue = _contextAccessor.HttpContext.Request.Headers[headerName];
                    if (StringValues.IsNullOrEmpty(headerValue))
                    {
                        continue;
                    }

                    request.Headers.TryAddWithoutValidation(headerName, (string[])headerValue);
                }
            }
            else
            {
                // Stops sending activity info (baggage) to external clients
                System.Diagnostics.Activity.Current = null;
            }

            var response = base.SendAsync(request, cancellationToken);

            // Restores current activity after call to external client was made
            if (System.Diagnostics.Activity.Current == null && currentActivity != null)
            {
                System.Diagnostics.Activity.Current = currentActivity;
            }

            return response;
        }
    }
}
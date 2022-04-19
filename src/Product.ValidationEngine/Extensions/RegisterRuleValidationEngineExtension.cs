using Microsoft.Azure.Amqp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Product.ValidationEngine.Extensions
{
    public static class RegisterRuleValidationEngineExtension
    {
        public static IServiceCollection RegisterRuleValidationEngine(this IServiceCollection services)
        {
            return services;
        }
    }
}

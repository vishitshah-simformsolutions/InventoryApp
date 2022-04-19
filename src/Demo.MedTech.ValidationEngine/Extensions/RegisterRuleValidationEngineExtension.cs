using Microsoft.Azure.Amqp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using Demo.MedTech.ValidationEngine.Rules;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Demo.MedTech.ValidationEngine.Extensions
{
    public static class RegisterRuleValidationEngineExtension
    {
        public static IServiceCollection RegisterRuleValidationEngine(this IServiceCollection services)
        {
            var ruleInstances = typeof(IRule).Assembly.GetTypes()
                .Where(t => typeof(IRule).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as IRule);
            services.TryAddEnumerable(ruleInstances.Select(Singleton<IRule>));

            var transformRuleInstances = typeof(ITransform).Assembly.GetTypes()
                .Where(t => typeof(ITransform).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as ITransform);
            services.TryAddEnumerable(transformRuleInstances.Select(Singleton<ITransform>));

            return services;
        }
    }
}

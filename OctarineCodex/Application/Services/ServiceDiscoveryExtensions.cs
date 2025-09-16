// OctarineCodex/Services/ServiceDiscoveryExtensions.cs

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Application.Services;

/// <summary>
///     Extensions for automatic service discovery and registration via reflection.
/// </summary>
public static class ServiceDiscoveryExtensions
{
    /// <summary>
    ///     Automatically registers all services marked with [Service&lt;TImplementation&gt;] attributes
    ///     from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">Assembly to scan (defaults to executing assembly).</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        // Only scan interfaces that have the Service<T> attribute (opt-in, not opt-out)
        var interfacesWithServiceAttribute = assembly.GetTypes()
            .Where(t => t.IsInterface || t.IsClass)
            .Where(HasServiceAttribute)
            .ToList();

        var registeredCount = 0;

        foreach (var interfaceType in interfacesWithServiceAttribute)
        {
            var serviceInfo = GetServiceAttributeInfo(interfaceType);
            if (serviceInfo == null)
            {
                continue;
            }

            // Validate that implementation actually implements the interface
            if (!interfaceType.IsAssignableFrom(serviceInfo.ImplementationType))
            {
                throw new InvalidOperationException(
                    $"Implementation type {serviceInfo.ImplementationType.FullName} does not implement interface {interfaceType.FullName}");
            }

            // Register the service with the specified lifetime
            var serviceDescriptor =
                new ServiceDescriptor(interfaceType, serviceInfo.ImplementationType, serviceInfo.Lifetime);
            services.Add(serviceDescriptor);

            registeredCount++;
        }

        // Log registration summary if logging is available
        try
        {
            var provider = services.BuildServiceProvider();
            var logger = provider.GetService<ILoggingService>();
            logger?.Info($"Auto-registered {registeredCount} services from assembly {assembly.GetName().Name}");
            provider.Dispose();
        }
        catch
        {
            // Ignore logging errors during service registration
        }

        return services;
    }

    private static bool HasServiceAttribute(Type interfaceType)
    {
        return interfaceType.GetCustomAttributes()
            .Any(attr => attr.GetType().IsGenericType &&
                         attr.GetType().GetGenericTypeDefinition() == typeof(ServiceAttribute<>));
    }

    private static ServiceAttributeInfo? GetServiceAttributeInfo(Type interfaceType)
    {
        // Look for ServiceAttribute<T> on the interface
        var serviceAttribute = interfaceType.GetCustomAttributes()
            .FirstOrDefault(attr => attr.GetType().IsGenericType &&
                                    attr.GetType().GetGenericTypeDefinition() == typeof(ServiceAttribute<>));

        if (serviceAttribute == null)
        {
            return null;
        }

        // Extract the generic type argument (TImplementation) from ServiceAttribute<TImplementation>
        var attributeType = serviceAttribute.GetType();
        var implementationType = attributeType.GetGenericArguments()[0];

        // Get the lifetime property value
        var lifetime = (ServiceLifetime)attributeType
            .GetProperty(nameof(ServiceAttribute<object>.Lifetime))!
            .GetValue(serviceAttribute)!;

        return new ServiceAttributeInfo { ImplementationType = implementationType, Lifetime = lifetime };
    }
}

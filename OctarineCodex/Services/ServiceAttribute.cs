// OctarineCodex/ServiceAttribute.cs

using System;
using Microsoft.Extensions.DependencyInjection;

namespace OctarineCodex;

/// <summary>
///     Marks an interface for automatic service registration with dependency injection.
///     The generic type parameter specifies the implementation class.
/// </summary>
/// <typeparam name="TImplementation">The implementation class for this service interface</typeparam>
[AttributeUsage(AttributeTargets.Interface)]
public class ServiceAttribute<TImplementation> : Attribute
    where TImplementation : class
{
    /// <summary>
    ///     Creates a service attribute with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime (defaults to Singleton for game services)</param>
    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Lifetime = lifetime;
    }

    /// <summary>
    ///     The service lifetime for dependency injection registration
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    ///     The implementation type for this service
    /// </summary>
    public Type ImplementationType { get; } = typeof(TImplementation);
}
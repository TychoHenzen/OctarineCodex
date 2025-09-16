// OctarineCodex/ServiceAttribute.cs

using System;
using Microsoft.Extensions.DependencyInjection;

namespace OctarineCodex.Application.Services;

/// <summary>
///     Marks an interface for automatic service registration with dependency injection.
///     The generic type parameter specifies the implementation class.
/// </summary>
/// <typeparam name="TImplementation">The implementation class for this service interface.</typeparam>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class ServiceAttribute<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Singleton) : Attribute
    where TImplementation : class
{
    /// <summary>
    ///     The service lifetime for dependency injection registration.
    /// </summary>
    public ServiceLifetime Lifetime { get; } = lifetime;

    /// <summary>
    ///     Gets the implementation type for this service.
    /// </summary>
    public Type ImplementationType { get; } = typeof(TImplementation);
}

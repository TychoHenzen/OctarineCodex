using System;
using Microsoft.Extensions.DependencyInjection;

namespace OctarineCodex.Application.Services;

/// <summary>
///     Helper class to extract service attribute information.
/// </summary>
internal class ServiceAttributeInfo
{
    public Type ImplementationType { get; set; } = null!;
    public ServiceLifetime Lifetime { get; set; }
}

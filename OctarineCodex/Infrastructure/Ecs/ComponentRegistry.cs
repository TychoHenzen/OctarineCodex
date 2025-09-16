using System;
using System.Collections.Generic;
using System.Reflection;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Infrastructure.Ecs;

/// <summary>
///     Manages component type registration and validation for the ECS system.
///     Provides reflection-based component discovery and type safety enforcement.
/// </summary>
public class ComponentRegistry(ILoggingService logger)
{
    private readonly Dictionary<string, Type> _componentsByName = [];
    private readonly HashSet<Type> _registeredComponents = [];

    /// <summary>
    ///     Gets all registered component types.
    /// </summary>
    public IReadOnlyCollection<Type> RegisteredComponents => _registeredComponents;

    /// <summary>
    ///     Registers a component type with the registry.
    /// </summary>
    /// <typeparam name="TComponent">The component type to register</typeparam>
    public void RegisterComponent<TComponent>() where TComponent : struct
    {
        Type componentType = typeof(TComponent);
        RegisterComponent(componentType);
    }

    /// <summary>
    ///     Registers a component type with the registry.
    /// </summary>
    /// <param name="componentType">The component type to register</param>
    public void RegisterComponent(Type componentType)
    {
        if (!componentType.IsValueType)
        {
            throw new ArgumentException($"Component type {componentType.Name} must be a struct", nameof(componentType));
        }

        if (_registeredComponents.Add(componentType))
        {
            _componentsByName[componentType.Name] = componentType;
            logger.Debug($"Registered component type: {componentType.Name}");
        }
    }

    /// <summary>
    ///     Automatically discovers and registers all struct types in the Components namespace.
    /// </summary>
    public void DiscoverComponents()
    {
        var assembly = Assembly.GetExecutingAssembly();
        Type[] componentTypes = assembly.GetTypes();

        var discoveredCount = 0;
        foreach (Type type in componentTypes)
        {
            // Look for struct types in Components namespace
            if (type.IsValueType &&
                type.Namespace != null &&
                type.Namespace.Contains("Components") &&
                !type.IsEnum &&
                !type.IsPrimitive)
            {
                RegisterComponent(type);
                discoveredCount++;
            }
        }

        logger.Info($"Component discovery completed. Found {discoveredCount} component types");
    }

    /// <summary>
    ///     Checks if a component type is registered.
    /// </summary>
    /// <typeparam name="TComponent">The component type to check</typeparam>
    /// <returns>True if registered, false otherwise</returns>
    public bool IsRegistered<TComponent>() where TComponent : struct
    {
        return _registeredComponents.Contains(typeof(TComponent));
    }

    /// <summary>
    ///     Gets a component type by name.
    /// </summary>
    /// <param name="componentName">The component type name</param>
    /// <returns>The component type if found, null otherwise</returns>
    public Type? GetComponentType(string componentName)
    {
        _componentsByName.TryGetValue(componentName, out Type? componentType);
        return componentType;
    }

    /// <summary>
    ///     Validates that all registered components follow ECS best practices.
    /// </summary>
    public ComponentRegistryValidationResult ValidateComponents()
    {
        var result = new ComponentRegistryValidationResult();

        foreach (Type componentType in _registeredComponents)
        {
            // Check for mutable fields (should be readonly or properties)
            FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (!field.IsInitOnly)
                {
                    result.Warnings.Add(
                        $"{componentType.Name}.{field.Name}: Consider making field readonly or use property");
                }
            }

            // Check for reference types (should generally be avoided in components)
            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsValueType && field.FieldType != typeof(string))
                {
                    result.Warnings.Add(
                        $"{componentType.Name}.{field.Name}: Reference type field may cause memory issues");
                }
            }
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }
}

/// <summary>
///     Results of component registry validation.
/// </summary>
public class ComponentRegistryValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
}

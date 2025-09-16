using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;
using OctarineCodex.Application.Systems;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Infrastructure.Ecs;

/// <summary>
///     Manages ECS system registration, execution ordering, and parallel processing.
///     Integrates with MonoGame game loop and provides system lifecycle management.
/// </summary>
[Service<SystemManager>]
public class SystemManager(IServiceProvider serviceProvider, ILoggingService logger) : IDisposable
{
    private readonly List<ISystem> _drawSystems = [];
    private readonly List<ISystem> _updateSystems = [];
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Dispose systems that implement IDisposable
        foreach (IDisposable system in _updateSystems.OfType<IDisposable>())
        {
            try
            {
                system.Dispose();
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error disposing update system {system.GetType().Name}");
            }
        }

        foreach (IDisposable system in _drawSystems.OfType<IDisposable>())
        {
            try
            {
                system.Dispose();
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error disposing draw system {system.GetType().Name}");
            }
        }

        _updateSystems.Clear();
        _drawSystems.Clear();
        _disposed = true;

        logger.Info("SystemManager disposed");
    }

    /// <summary>
    ///     Registers a system for update processing.
    ///     Systems are executed in registration order during Update phase.
    /// </summary>
    /// <typeparam name="TSystem">The system type to register</typeparam>
    public void RegisterUpdateSystem<TSystem>() where TSystem : class, ISystem
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var system = serviceProvider.GetRequiredService<TSystem>();
        _updateSystems.Add(system);

        logger.Debug($"Registered update system: {typeof(TSystem).Name}");
    }


    /// <summary>
    ///     Registers a system for draw processing.
    ///     Systems are executed in registration order during Draw phase.
    /// </summary>
    /// <typeparam name="TSystem">The system type to register</typeparam>
    public void RegisterDrawSystem<TSystem>() where TSystem : class, ISystem
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var system = serviceProvider.GetRequiredService<TSystem>();
        _drawSystems.Add(system);

        logger.Debug($"Registered draw system: {typeof(TSystem).Name}");
    }

    /// <summary>
    ///     Updates all registered update systems in order.
    ///     Called during MonoGame Update phase.
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    public void Update(GameTime gameTime)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (ISystem system in _updateSystems)
        {
            try
            {
                system.Update(gameTime);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error updating system {system.GetType().Name}");
            }
        }
    }

    /// <summary>
    ///     Draws all registered draw systems in order.
    ///     Called during MonoGame Draw phase.
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    public void Draw(GameTime gameTime)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (ISystem system in _drawSystems)
        {
            try
            {
                system.Draw(gameTime);
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Error drawing system {system.GetType().Name}");
            }
        }
    }

    /// <summary>
    ///     Gets diagnostic information about registered systems.
    /// </summary>
    public SystemManagerDiagnostics GetDiagnostics()
    {
        return new SystemManagerDiagnostics
        {
            UpdateSystemCount = _updateSystems.Count,
            DrawSystemCount = _drawSystems.Count,
            UpdateSystemTypes = _updateSystems.Select(s => s.GetType().Name).ToArray(),
            DrawSystemTypes = _drawSystems.Select(s => s.GetType().Name).ToArray()
        };
    }

    /// <summary>
    ///     Gets a registered system by type. This is a temporary method for Phase 1.
    ///     In Phase 2, we'll have a better system access pattern.
    /// </summary>
    /// <typeparam name="TSystem">The system type to get</typeparam>
    /// <returns>The system instance if found, null otherwise</returns>
    public TSystem? GetSystem<TSystem>() where TSystem : class, ISystem
    {
        TSystem? system = _updateSystems.OfType<TSystem>().FirstOrDefault() ??
                          _drawSystems.OfType<TSystem>().FirstOrDefault();
        return system;
    }
}

/// <summary>
///     Diagnostic information about the system manager state.
/// </summary>
public class SystemManagerDiagnostics
{
    public int UpdateSystemCount { get; init; }
    public int DrawSystemCount { get; init; }
    public string[] UpdateSystemTypes { get; init; } = [];
    public string[] DrawSystemTypes { get; init; } = [];
}

using System;
using System.Linq;
using DefaultEcs;
using OctarineCodex.Application.Services;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Infrastructure.Ecs;

/// <summary>
///     Manages DefaultEcs World instances and handles entity lifecycle and scene transitions.
///     Integrates with existing service container while providing ECS world management.
/// </summary>
[Service<WorldManager>]
public class WorldManager : IDisposable
{
    private readonly ILoggingService _logger;
    private World? _currentWorld;
    private bool _disposed;

    public WorldManager(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Gets the current active ECS world instance.
    /// </summary>
    public World CurrentWorld => _currentWorld ?? throw new InvalidOperationException("World has not been initialized");

    /// <summary>
    ///     Gets whether the world manager has been initialized with a world.
    /// </summary>
    public bool IsInitialized => _currentWorld != null;

    /// <summary>
    ///     Gets the total number of entities in the current world.
    /// </summary>
    public int EntityCount => _currentWorld?.GetEntities().AsEnumerable().Count() ?? 0;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _currentWorld?.Dispose();
        _currentWorld = null;
        _disposed = true;

        _logger.Debug("WorldManager disposed");
    }

    /// <summary>
    ///     Initializes the ECS world manager with a new world instance.
    /// </summary>
    public void Initialize()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WorldManager));
        }

        if (_currentWorld != null)
        {
            _logger.Warn("World is already initialized. Disposing existing world.");
            _currentWorld.Dispose();
        }

        _currentWorld = new World();
        _logger.Debug("ECS World initialized successfully");
    }

    /// <summary>
    ///     Creates a new entity in the current world.
    /// </summary>
    /// <returns>A new entity instance</returns>
    public Entity CreateEntity()
    {
        if (_currentWorld == null)
        {
            throw new InvalidOperationException("World must be initialized before creating entities");
        }

        return _currentWorld.CreateEntity();
    }

    /// <summary>
    ///     Transitions to a new world, disposing the current one if it exists.
    ///     Used for scene transitions and level loading.
    /// </summary>
    public void TransitionToNewWorld()
    {
        _logger.Debug("Transitioning to new ECS world");

        _currentWorld?.Dispose();
        _currentWorld = new World();

        _logger.Debug("New ECS world created for scene transition");
    }
}

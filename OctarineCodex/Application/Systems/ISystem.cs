using System;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Application.Systems;

/// <summary>
///     Base interface for all ECS systems in OctarineCodex.
///     Systems contain behavior logic and operate on component data.
/// </summary>
public interface ISystem
{
    /// <summary>
    ///     Updates the system logic. Called during MonoGame Update phase.
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    void Update(GameTime gameTime);

    /// <summary>
    ///     Draws system output. Called during MonoGame Draw phase.
    ///     Not all systems need to draw - many will have empty implementations.
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    void Draw(GameTime gameTime);
}

/// <summary>
///     Extended interface for systems that need dependency declaration for parallel processing.
///     Used in Phase 4 for advanced system scheduling.
/// </summary>
public interface ISystemWithDependencies : ISystem
{
    /// <summary>
    ///     Gets the component types this system reads from.
    ///     Used for parallel processing dependency analysis.
    /// </summary>
    Type[] ReadDependencies { get; }

    /// <summary>
    ///     Gets the component types this system writes to.
    ///     Used for parallel processing dependency analysis.
    /// </summary>
    Type[] WriteDependencies { get; }
}

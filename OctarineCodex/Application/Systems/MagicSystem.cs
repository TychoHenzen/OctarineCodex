// OctarineCodex/Application/Systems/MagicSystem.cs

using System;
using System.Collections.Generic;
using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Components;
using OctarineCodex.Domain.Magic;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Application.Systems;

/// <summary>
///     ECS system responsible for processing magical interactions between entities.
///     Handles magic vector calculations, spell effects, and magical damage resolution.
/// </summary>
[Service<ISystem>]
public class MagicSystem : AEntitySetSystem<GameTime>, ISystemWithDependencies
{
    private readonly ILoggingService _logger;
    private readonly IMagicCalculator _magicCalculator;
    private readonly IMessageBus _messageBus;

    /// <summary>
    ///     Initializes the magic system with required dependencies.
    /// </summary>
    public MagicSystem(World world, IMagicCalculator magicCalculator, IMessageBus messageBus, ILoggingService logger)
        : base(world.GetEntities().With<MagicVectorComponent>().AsSet())
    {
        _magicCalculator = magicCalculator;
        _messageBus = messageBus;
        _logger = logger;
    }

    // Component type dependencies for parallel processing
    public Type[] ReadDependencies => new[] { typeof(MagicVectorComponent), typeof(PositionComponent) };
    public Type[] WriteDependencies => new[] { typeof(MagicVectorComponent) };

    /// <summary>
    ///     Draws magical effects (currently no visual output).
    /// </summary>
    public void Draw(GameTime gameTime)
    {
        // Magic system doesn't directly render - visual effects handled by rendering systems
        // Could add debug visualization here in the future
    }

    /// <summary>
    ///     Updates magical interactions and processes magic-related effects.
    /// </summary>
    protected override void Update(GameTime gameTime, ReadOnlySpan<Entity> entities)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var interactions = new List<(Entity source, Entity target, MagicSignature result)>();

        // Process each magical entity
        // Create a list to track processed entity pairs to avoid double processing
        var processedPairs = new HashSet<(int, int)>();

// Process each magical entity
        foreach (Entity entity in entities)
        {
            ref MagicVectorComponent magicComponent = ref entity.Get<MagicVectorComponent>();

            // Skip inactive magical entities
            if (!magicComponent.IsActive || !magicComponent.HasMagicalPresence)
            {
                continue;
            }

            ProcessMagicalDecay(ref magicComponent, deltaTime);

            // Find nearby magical entities for interactions
            if (entity.Has<PositionComponent>())
            {
                Vector2 position = entity.Get<PositionComponent>().Position;
                List<(Entity source, Entity target, MagicSignature result)> nearbyInteractions =
                    FindNearbyMagicalInteractions(entity, position, entities, processedPairs);
                interactions.AddRange(nearbyInteractions);
            }
        }

        // Process all magical interactions
        foreach (var (source, target, result) in interactions)
        {
            ProcessMagicalInteraction(source, target, result, deltaTime);
        }
    }

    private List<(Entity source, Entity target, MagicSignature result)> FindNearbyMagicalInteractions(
        Entity sourceEntity, Vector2 sourcePosition, ReadOnlySpan<Entity> allEntities,
        HashSet<(int, int)> processedPairs)
    {
        const float maxInteractionDistance = 100f;
        const float minInteractionStrength = 0.1f;

        var interactions = new List<(Entity, Entity, MagicSignature)>();
        var sourceMagic = sourceEntity.Get<MagicVectorComponent>();

        foreach (Entity targetEntity in allEntities)
        {
            // Skip self and inactive entities
            if (targetEntity == sourceEntity)
            {
                continue;
            }

            // Prevent double processing by using entity hash codes
            var sourceId = sourceEntity.GetHashCode();
            var targetId = targetEntity.GetHashCode();
            (int, int) pairId = sourceId < targetId ? (sourceId, targetId) : (targetId, sourceId);

            if (processedPairs.Contains(pairId))
            {
                continue;
            }

            if (!targetEntity.Has<MagicVectorComponent>() || !targetEntity.Has<PositionComponent>())
            {
                continue;
            }

            var targetMagic = targetEntity.Get<MagicVectorComponent>();
            if (!targetMagic.IsActive || !targetMagic.HasMagicalPresence)
            {
                continue;
            }

            // Check distance
            Vector2 targetPosition = targetEntity.Get<PositionComponent>().Position;
            var distance = Vector2.Distance(sourcePosition, targetPosition);

            if (distance > maxInteractionDistance)
            {
                continue;
            }

            // Calculate interaction strength
            var interactionStrength = AspectCalculator.CalculateInteractionStrength(
                sourceMagic.EffectiveVector, targetMagic.EffectiveVector);

            if (interactionStrength < minInteractionStrength)
            {
                continue;
            }

            // Mark this pair as processed
            processedPairs.Add(pairId);

            // Calculate the magical interaction result
            MagicSignature interactionResult = _magicCalculator.CalculateInteraction(
                sourceMagic.EffectiveVector, targetMagic.EffectiveVector);

            // Apply distance falloff
            var distanceFalloff = Math.Max(0.1f, 1f - (distance / maxInteractionDistance));
            MagicSignature finalResult = interactionResult * distanceFalloff;

            interactions.Add((sourceEntity, targetEntity, finalResult));

            _logger.Debug($"Magic interaction queued - Distance: {distance:F2}, Strength: {interactionStrength:F2}");
        }

        return interactions;
    }

    /// <summary>
    ///     Processes magical decay and temporary effect expiration.
    /// </summary>
    private void ProcessMagicalDecay(ref MagicVectorComponent magicComponent, float deltaTime)
    {
        // Check if magic has been modified and should decay back toward base
        var currentTime = DateTime.UtcNow.Ticks / 10000000.0;
        var timeSinceModification = currentTime - magicComponent.LastModifiedTime;

        // Apply decay if vector differs significantly from base and enough time has passed
        if (timeSinceModification > 1.0 && // 1 second minimum before decay
            MagicSignature.Distance(magicComponent.Vector, magicComponent.BaseVector) > 0.1f)
        {
            var decayRate = 0.5f * deltaTime; // Decay to base over ~2 seconds
            magicComponent.Vector = MagicSignature.Lerp(magicComponent.Vector, magicComponent.BaseVector, decayRate);

            _logger.Debug(
                $"Magic decay applied - Distance to base: {MagicSignature.Distance(magicComponent.Vector, magicComponent.BaseVector):F3}");
        }
    }

    /// <summary>
    ///     Finds nearby magical entities that should interact with the given entity.
    /// </summary>
    private List<(Entity source, Entity target, MagicSignature result)> FindNearbyMagicalInteractions(
        Entity sourceEntity, Vector2 sourcePosition, ReadOnlySpan<Entity> allEntities)
    {
        const float maxInteractionDistance = 100f; // Configurable interaction range
        const float minInteractionStrength = 0.1f; // Minimum strength for interaction

        var interactions = new List<(Entity, Entity, MagicSignature)>();
        var sourceMagic = sourceEntity.Get<MagicVectorComponent>();

        foreach (Entity targetEntity in allEntities)
        {
            // Skip self and inactive entities
            if (targetEntity == sourceEntity)
            {
                continue;
            }

            if (!targetEntity.Has<MagicVectorComponent>() || !targetEntity.Has<PositionComponent>())
            {
                continue;
            }

            var targetMagic = targetEntity.Get<MagicVectorComponent>();
            if (!targetMagic.IsActive || !targetMagic.HasMagicalPresence)
            {
                continue;
            }

            // Check distance
            Vector2 targetPosition = targetEntity.Get<PositionComponent>().Position;
            var distance = Vector2.Distance(sourcePosition, targetPosition);

            if (distance > maxInteractionDistance)
            {
                continue;
            }

            // Calculate interaction strength
            var interactionStrength = AspectCalculator.CalculateInteractionStrength(
                sourceMagic.EffectiveVector, targetMagic.EffectiveVector);

            if (interactionStrength < minInteractionStrength)
            {
                continue;
            }

            // Calculate the magical interaction result
            MagicSignature interactionResult = _magicCalculator.CalculateInteraction(
                sourceMagic.EffectiveVector, targetMagic.EffectiveVector);

            // Apply distance falloff
            var distanceFalloff = Math.Max(0.1f, 1f - (distance / maxInteractionDistance));
            MagicSignature finalResult = interactionResult * distanceFalloff;

            interactions.Add((sourceEntity, targetEntity, finalResult));

            _logger.Debug($"Magic interaction queued - Distance: {distance:F2}, Strength: {interactionStrength:F2}");
        }

        return interactions;
    }

    /// <summary>
    ///     Processes a specific magical interaction between two entities.
    /// </summary>
    private void ProcessMagicalInteraction(Entity source, Entity target, MagicSignature interactionResult,
        float deltaTime)
    {
        ref MagicVectorComponent sourceMagic = ref source.Get<MagicVectorComponent>();
        ref MagicVectorComponent targetMagic = ref target.Get<MagicVectorComponent>();

        // Apply interaction effects to both entities
        MagicSignature sourceEffect = interactionResult * 0.1f * deltaTime;
        MagicSignature targetEffect = interactionResult * -0.05f * deltaTime;

        sourceMagic.ModifyTemporary(sourceEffect);
        targetMagic.ModifyTemporary(targetEffect);

        // Send interaction message with simpler entity IDs
        var interactionMessage = new MagicalInteractionMessage
        {
            SourceEntityId = $"Entity_{source.GetHashCode():X8}", // Use hex hash as ID
            TargetEntityId = $"Entity_{target.GetHashCode():X8}",
            InteractionResult = interactionResult,
            InteractionStrength = AspectCalculator.CalculateInteractionStrength(
                sourceMagic.EffectiveVector, targetMagic.EffectiveVector),
            Timestamp = DateTime.UtcNow
        };

        _messageBus.SendMessage(interactionMessage);

        _logger.Debug(
            $"Magical interaction processed - Source: {interactionMessage.SourceEntityId}, Target: {interactionMessage.TargetEntityId}, Result magnitude: {interactionResult.Magnitude:F2}");
    }
}

/// <summary>
///     Message sent when a magical interaction occurs between two entities.
/// </summary>
public class MagicalInteractionMessage
{
    public required string SourceEntityId { get; init; }
    public required string TargetEntityId { get; init; }
    public required MagicSignature InteractionResult { get; init; }
    public required float InteractionStrength { get; init; }
    public required DateTime Timestamp { get; init; }
}

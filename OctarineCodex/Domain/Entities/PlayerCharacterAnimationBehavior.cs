using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Characters;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Messages;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Domain.Characters;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Presentation.Input;

namespace OctarineCodex.Domain.Entities;

[EntityBehavior(EntityType = "Player", Priority = 500)]
public class PlayerCharacterAnimationBehavior(
    ICharacterCustomization characterCustomization,
    IInputService inputService,
    ILoggingService logger)
    : EntityBehavior,
        IMessageHandler<PlayerMovedMessage>,
        IMessageHandler<MovementBlockedMessage>,
        IMessageHandler<PlayerIdleMessage>
{
    private string _currentFacingDirection = "Down";

    private PlayerAnimationState _currentState = PlayerAnimationState.Idle;
    private string _lastPlayedAnimation = string.Empty;

    public void HandleMessage(MovementBlockedMessage? message, string? senderId = null)
    {
        if (message != null && message.IntendedDirection != Vector2.Zero)
        {
            _currentFacingDirection = GetDirectionFromVector(message.IntendedDirection);
        }

        _currentState = PlayerAnimationState.Push;
        logger.Debug($"Player pushing against wall: {_currentFacingDirection}");
        UpdateAnimation();
    }

    public void HandleMessage(PlayerIdleMessage message, string? senderId = null)
    {
        _currentState = PlayerAnimationState.Idle;
        logger.Debug($"Player idle: {_currentFacingDirection}");
        UpdateAnimation();
    }

    public void HandleMessage(PlayerMovedMessage message, string? senderId = null)
    {
        Vector2 movement = message.Delta;

        if (movement == Vector2.Zero)
        {
            return;
        }

        _currentState = PlayerAnimationState.Walk;
        _currentFacingDirection = GetDirectionFromVector(movement);
        logger.Debug($"Player moved: {_currentFacingDirection}, State: {_currentState}");
        UpdateAnimation();
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity)
    {
        base.Initialize(entity);

        logger.Info("Initializing player character animation system...");

        try
        {
            // Initialize character with minimal layer setup
            InitializeDefaultCharacter();

            // Start with idle animation facing down
            UpdateAnimation();
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to initialize PlayerCharacterAnimationBehavior: {ex.Message}");
            logger.Error($"Stack trace: {ex.StackTrace}");
        }
    }

    public override void Update(GameTime gameTime)
    {
        try
        {
            characterCustomization?.Update(gameTime);
        }
        catch (Exception ex)
        {
            logger.Error($"Error in PlayerCharacterAnimationBehavior Update: {ex.Message}");
        }
    }

    public IEnumerable<LayerRenderData> GetLayerRenderData()
    {
        return characterCustomization.GetLayerRenderData();
    }

    private static Dictionary<string, CharacterLayerDefinition> CreateDefaultLayerDefinitions()
    {
        return new Dictionary<string, CharacterLayerDefinition>
        {
            ["Bodies"] = new(
                "Bodies",
                0,
                GetAvailableAssets("Bodies"),
                "animation.json")
        };
    }

    private static List<string> GetAvailableAssets(string category)
    {
        return category switch
        {
            "Bodies" => ["Body_01"],
            _ => [$"{category}_01"]
        };
    }

    private static string GetDirectionFromVector(Vector2 movement)
    {
        if (MathF.Abs(movement.X) > MathF.Abs(movement.Y))
        {
            return movement.X > 0 ? "Right" : "Left";
        }

        return movement.Y > 0 ? "Down" : "Up";
    }

    private void UpdateAnimation()
    {
        var animationName = GetAnimationName();

        if (animationName == _lastPlayedAnimation)
        {
            return;
        }

        logger.Info($"Playing animation: {animationName}");
        characterCustomization.PlayAnimation(animationName);
        _lastPlayedAnimation = animationName;
    }

    private string GetAnimationName()
    {
        var stateName = _currentState switch
        {
            PlayerAnimationState.Walk => "Walk",
            PlayerAnimationState.Push => "Push",
            _ => "Idle"
        };

        return $"{stateName}_{_currentFacingDirection}";
    }

    private void InitializeDefaultCharacter()
    {
        logger.Info("Loading default character layers...");

        try
        {
            characterCustomization.InitializeLayers(CreateDefaultLayerDefinitions());
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to initialize character layers: {ex.Message}");
        }
    }
}

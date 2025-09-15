using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Characters;
using OctarineCodex.Application.Entities;
using OctarineCodex.Application.Messages;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Domain.Characters;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Presentation.Input;

namespace OctarineCodex.Domain.Entities;

/// <summary>
///     Player animation states for proper state machine behavior
/// </summary>
public enum PlayerAnimationState
{
    Idle, // No input, not moving
    Walk, // Input active, movement successful
    Push // Input active, movement blocked (pushing against wall)
}

[EntityBehavior(EntityType = "Player", Priority = 500)]
public class PlayerCharacterAnimationBehavior : EntityBehavior,
    IMessageHandler<PlayerMovedMessage>,
    IMessageHandler<MovementBlockedMessage>,
    IMessageHandler<PlayerIdleMessage>
{
    private readonly ICharacterCustomizationService _characterCustomizationService;
    private readonly IInputService _inputService;
    private readonly ILoggingService _logger;
    private string _currentFacingDirection = "Down";

    private PlayerAnimationState _currentState = PlayerAnimationState.Idle;
    private string _lastPlayedAnimation = "";

    public PlayerCharacterAnimationBehavior(
        ICharacterCustomizationService characterCustomizationService,
        IInputService inputService,
        ILoggingService logger)
    {
        _characterCustomizationService = characterCustomizationService ??
                                         throw new ArgumentNullException(nameof(characterCustomizationService));
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Add public property to expose character system for rendering
    public ICharacterCustomization? CharacterSystem { get; private set; }

    public void HandleMessage(MovementBlockedMessage message, string? senderId = null)
    {
        if (message.IntendedDirection != Vector2.Zero)
        {
            _currentFacingDirection = GetDirectionFromVector(message.IntendedDirection);
        }

        _currentState = PlayerAnimationState.Push;
        _logger.Debug($"Player pushing against wall: {_currentFacingDirection}");
        UpdateAnimation();
    }

    public void HandleMessage(PlayerIdleMessage message, string? senderId = null)
    {
        _currentState = PlayerAnimationState.Idle;
        _logger.Debug($"Player idle: {_currentFacingDirection}");
        UpdateAnimation();
    }

    public void HandleMessage(PlayerMovedMessage message, string? senderId = null)
    {
        Vector2 movement = message.Delta;

        if (movement != Vector2.Zero)
        {
            _currentState = PlayerAnimationState.Walk;
            _currentFacingDirection = GetDirectionFromVector(movement);
            _logger.Debug($"Player moved: {_currentFacingDirection}, State: {_currentState}");
            UpdateAnimation();
        }
    }

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        return HasEntityType(entity, "Player");
    }

    public override void Initialize(EntityWrapper entity)
    {
        base.Initialize(entity);

        _logger.Info("Initializing player character animation system...");

        try
        {
            // Create character system via factory instead of manual instantiation
            CharacterSystem = _characterCustomizationService.CreateCustomization();

            // Initialize character with minimal layer setup
            InitializeDefaultCharacter();

            // Start with idle animation facing down
            UpdateAnimation();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to initialize PlayerCharacterAnimationBehavior: {ex.Message}");
            _logger.Error($"Stack trace: {ex.StackTrace}");
        }
    }

    public override void Update(GameTime gameTime)
    {
        try
        {
            CharacterSystem?.Update(gameTime);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in PlayerCharacterAnimationBehavior Update: {ex.Message}");
        }
    }

    private void UpdateAnimation()
    {
        if (CharacterSystem == null)
        {
            return;
        }

        var animationName = GetAnimationName();

        if (animationName != _lastPlayedAnimation)
        {
            _logger.Info($"Playing animation: {animationName}");
            CharacterSystem.PlayAnimation(animationName);
            _lastPlayedAnimation = animationName;
        }
    }

    private string GetAnimationName()
    {
        var stateName = _currentState switch
        {
            PlayerAnimationState.Walk => "Walk",
            PlayerAnimationState.Push => "Push",
            PlayerAnimationState.Idle => "Idle",
            _ => "Idle"
        };

        return $"{stateName}_{_currentFacingDirection}";
    }

    private static string GetDirectionFromVector(Vector2 movement)
    {
        if (MathF.Abs(movement.X) > MathF.Abs(movement.Y))
        {
            return movement.X > 0 ? "Right" : "Left";
        }

        return movement.Y > 0 ? "Down" : "Up";
    }

    private void InitializeDefaultCharacter()
    {
        _logger.Info("Loading default character layers...");

        try
        {
            CharacterSystem?.InitializeLayers(CreateDefaultLayerDefinitions());
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to initialize character layers: {ex.Message}");
        }
    }

    private Dictionary<string, CharacterLayerDefinition> CreateDefaultLayerDefinitions()
    {
        return new Dictionary<string, CharacterLayerDefinition>
        {
            ["Bodies"] = new(
                "Bodies",
                0,
                GetAvailableAssets("Bodies"),
                AsepriteJsonPath: "animation.json")
        };
    }

    private string[] GetAvailableAssets(string category)
    {
        return category switch
        {
            "Bodies" => new[] { "Body_01" },
            _ => new[] { $"{category}_01" }
        };
    }
}

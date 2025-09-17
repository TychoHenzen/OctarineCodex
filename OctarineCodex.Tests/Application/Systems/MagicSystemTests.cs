// OctarineCodex.Tests/Application/Systems/MagicSystemTests.cs

using DefaultEcs;
using FluentAssertions;
using Microsoft.Xna.Framework;
using NSubstitute;
using NSubstitute.Core;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Application.Systems;
using OctarineCodex.Domain.Components;
using OctarineCodex.Domain.Magic;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Tests.Application.Systems;

/// <summary>
///     Unit tests for the MagicSystem ECS system.
/// </summary>
public class MagicSystemTests : IDisposable
{
    private readonly MagicSystem _magicSystem;
    private readonly LoggingService _mockLogger;
    private readonly IMagicCalculator _mockMagicCalculator;
    private readonly IMessageBus _mockMessageBus;
    private readonly World _world;

    public MagicSystemTests()
    {
        _world = new World();
        _mockMagicCalculator = Substitute.For<IMagicCalculator>();
        _mockMessageBus = Substitute.For<IMessageBus>();
        _mockLogger = Substitute.For<LoggingService>();
        _magicSystem = new MagicSystem(_world, _mockMagicCalculator, _mockMessageBus, _mockLogger);
    }

    public void Dispose()
    {
        _magicSystem?.Dispose();
        _world?.Dispose();
    }

    [Fact]
    public void Update_WithInactiveMagicalEntity_SkipsProcessing()
    {
        // Arrange
        Entity entity = _world.CreateEntity();
        entity.Set(new MagicVectorComponent(MagicSignature.One, 1f, false)); // Inactive magic
        entity.Set(new PositionComponent(Vector2.Zero));

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));

        // Clear any previous calls to avoid argument matcher conflicts
        _mockMagicCalculator.ClearReceivedCalls();
        _mockMessageBus.ClearReceivedCalls();

        // Act
        _magicSystem.Update(gameTime);

        // Assert - Use DidNotReceiveWithAnyArgs to avoid argument matcher issues
        _mockMagicCalculator.DidNotReceiveWithAnyArgs()
            .CalculateInteraction(default(MagicSignature), default(MagicSignature));
        _mockMessageBus.DidNotReceiveWithAnyArgs().SendMessage(default(MagicalInteractionMessage));
    }

    [Fact]
    public void Update_WithActiveMagicalEntity_ProcessesDecay()
    {
        // Arrange
        var baseVector = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var modifiedVector = new MagicSignature(2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        Entity entity = _world.CreateEntity();
        var magicComponent = new MagicVectorComponent(modifiedVector, 1f, true, baseVector);
        magicComponent.LastModifiedTime = (DateTime.UtcNow.Ticks / 10000000.0) - 2.0; // 2 seconds ago
        entity.Set(magicComponent);
        entity.Set(new PositionComponent(Vector2.Zero));

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)); // 100ms frame

        // Act
        _magicSystem.Update(gameTime);

        // Assert
        var updatedMagic = entity.Get<MagicVectorComponent>();
        var distance = MagicSignature.Distance(updatedMagic.Vector, baseVector);
        var originalDistance = MagicSignature.Distance(modifiedVector, baseVector);

        distance.Should().BeLessThan(originalDistance); // Should have decayed toward base
    }

    [Fact]
    public void Update_WithTwoNearbyMagicalEntities_ProcessesInteraction()
    {
        // Arrange
        var vector1 = new MagicSignature(2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var vector2 = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        Entity entity1 = _world.CreateEntity();
        entity1.Set(new MagicVectorComponent(vector1));
        entity1.Set(new PositionComponent(new Vector2(0, 0)));

        Entity entity2 = _world.CreateEntity();
        entity2.Set(new MagicVectorComponent(vector2));
        entity2.Set(new PositionComponent(new Vector2(50, 0))); // Within interaction range

        var interactionResult = new MagicSignature(0.5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Clear previous calls and setup mocks without using Arg.Any
        _mockMagicCalculator.ClearReceivedCalls();
        _mockMessageBus.ClearReceivedCalls();

        // Setup return values for potential interaction calls
        _mockMagicCalculator.CalculateInteraction(vector1, vector2).Returns(interactionResult);
        _mockMagicCalculator.CalculateInteraction(vector2, vector1).Returns(interactionResult);

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));

        // Act
        _magicSystem.Update(gameTime);

        // Assert - Use ReceivedWithAnyArgs to avoid specific argument matching
        _mockMagicCalculator.ReceivedWithAnyArgs(1)
            .CalculateInteraction(default(MagicSignature), default(MagicSignature));
        _mockMessageBus.ReceivedWithAnyArgs(1).SendMessage(default(MagicalInteractionMessage));
    }

    [Fact]
    public void Update_WithDistantMagicalEntities_SkipsInteraction()
    {
        // Arrange
        Entity entity1 = _world.CreateEntity();
        entity1.Set(new MagicVectorComponent(new MagicSignature(2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f)));
        entity1.Set(new PositionComponent(new Vector2(0, 0)));

        Entity entity2 = _world.CreateEntity();
        entity2.Set(new MagicVectorComponent(new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f)));
        entity2.Set(new PositionComponent(new Vector2(200, 0))); // Beyond interaction range

        // Clear any previous calls
        _mockMagicCalculator.ClearReceivedCalls();
        _mockMessageBus.ClearReceivedCalls();

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));

        // Act
        _magicSystem.Update(gameTime);

        // Assert
        _mockMagicCalculator.DidNotReceiveWithAnyArgs()
            .CalculateInteraction(default(MagicSignature), default(MagicSignature));
        _mockMessageBus.DidNotReceiveWithAnyArgs().SendMessage(default(MagicalInteractionMessage));
    }

    [Fact]
    public void Update_WithWeakMagicalInteraction_SkipsInteraction()
    {
        // Arrange
        Entity entity1 = _world.CreateEntity();
        entity1.Set(new MagicVectorComponent(new MagicSignature(0.01f, 0f, 0f, 0f, 0f, 0f, 0f, 0f)));
        entity1.Set(new PositionComponent(new Vector2(0, 0)));

        Entity entity2 = _world.CreateEntity();
        entity2.Set(new MagicVectorComponent(new MagicSignature(0f, 0.01f, 0f, 0f, 0f, 0f, 0f, 0f)));
        entity2.Set(new PositionComponent(new Vector2(10, 0))); // Close but weak interaction

        // Clear any previous calls
        _mockMagicCalculator.ClearReceivedCalls();
        _mockMessageBus.ClearReceivedCalls();

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));

        // Act
        _magicSystem.Update(gameTime);

        // Assert
        // Should skip due to low interaction strength
        _mockMagicCalculator.DidNotReceiveWithAnyArgs()
            .CalculateInteraction(default(MagicSignature), default(MagicSignature));
    }

    [Fact]
    public void Update_WithMagicalInteraction_SendsCorrectMessage()
    {
        // Arrange
        var vector1 = new MagicSignature(3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var vector2 = new MagicSignature(2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        Entity entity1 = _world.CreateEntity();
        var magic1 = new MagicVectorComponent(vector1);
        entity1.Set(magic1);
        entity1.Set(new PositionComponent(Vector2.Zero));

        Entity entity2 = _world.CreateEntity();
        var magic2 = new MagicVectorComponent(vector2);
        entity2.Set(magic2);
        entity2.Set(new PositionComponent(new Vector2(30, 0)));

        var interactionResult = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Clear previous calls and setup mock
        _mockMagicCalculator.ClearReceivedCalls();
        _mockMessageBus.ClearReceivedCalls();

        // Setup interaction result
        _mockMagicCalculator.CalculateInteraction(vector1, vector2).Returns(interactionResult);
        _mockMagicCalculator.CalculateInteraction(vector2, vector1).Returns(interactionResult);

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));

        // Act
        _magicSystem.Update(gameTime);

        // Assert - Capture the actual message and verify properties individually
        _mockMessageBus.Received(1).SendMessage(Arg.Any<MagicalInteractionMessage>());

        // Get the actual call to examine the message
        List<ICall> calls = _mockMessageBus.ReceivedCalls().ToList();
        ICall? messageCall = calls.FirstOrDefault(c => c.GetMethodInfo().Name == nameof(IMessageBus.SendMessage));
        messageCall.Should().NotBeNull();

        var actualMessage = messageCall.GetArguments()[0] as MagicalInteractionMessage;
        actualMessage.Should().NotBeNull();

        // Verify message properties individually for better error reporting
        actualMessage.SourceEntityId.Should().NotBeNullOrEmpty();
        actualMessage.TargetEntityId.Should().NotBeNullOrEmpty();
        actualMessage.InteractionStrength.Should().BeGreaterThan(0f);
        actualMessage.InteractionResult.Magnitude.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void Update_WithInteraction_ModifiesMagicVectors()
    {
        // Arrange
        var originalVector1 = new MagicSignature(3f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        var originalVector2 = new MagicSignature(2f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        Entity entity1 = _world.CreateEntity();
        entity1.Set(new MagicVectorComponent(originalVector1));
        entity1.Set(new PositionComponent(Vector2.Zero));

        Entity entity2 = _world.CreateEntity();
        entity2.Set(new MagicVectorComponent(originalVector2));
        entity2.Set(new PositionComponent(new Vector2(30, 0)));

        var interactionResult = new MagicSignature(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        // Clear previous calls and setup mock
        _mockMagicCalculator.ClearReceivedCalls();
        _mockMessageBus.ClearReceivedCalls();

        // Setup interaction result
        _mockMagicCalculator.CalculateInteraction(originalVector1, originalVector2).Returns(interactionResult);
        _mockMagicCalculator.CalculateInteraction(originalVector2, originalVector1).Returns(interactionResult);

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));

        // Act
        _magicSystem.Update(gameTime);

        // Assert
        var magic1 = entity1.Get<MagicVectorComponent>();
        var magic2 = entity2.Get<MagicVectorComponent>();

        magic1.Vector.Should().NotBe(originalVector1); // Should be modified by interaction
        magic2.Vector.Should().NotBe(originalVector2); // Should be modified by interaction

        // Verify LastModifiedTime was updated
        magic1.LastModifiedTime.Should().BeGreaterThan(0);
        magic2.LastModifiedTime.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Draw_DoesNotThrow()
    {
        // Arrange
        var gameTime = new GameTime();

        // Act & Assert
        Action act = () => _magicSystem.Draw(gameTime);
        act.Should().NotThrow();
    }

    [Fact]
    public void ReadDependencies_ContainsRequiredComponentTypes()
    {
        // Act
        Type[] dependencies = _magicSystem.ReadDependencies;

        // Assert
        dependencies.Should().Contain(typeof(MagicVectorComponent));
        dependencies.Should().Contain(typeof(PositionComponent));
    }

    [Fact]
    public void WriteDependencies_ContainsMagicVectorComponent()
    {
        // Act
        Type[] dependencies = _magicSystem.WriteDependencies;

        // Assert
        dependencies.Should().Contain(typeof(MagicVectorComponent));
    }
}

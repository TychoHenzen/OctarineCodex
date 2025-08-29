using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NSubstitute;
using OctarineCodex.Input;
using Xunit;

namespace OctarineCodex.Tests.Input;

public class CompositeInputServiceTests
{
    private readonly IKeyboardInputProvider _mockKeyboard;
    private readonly IControllerInputProvider _mockController;
    private readonly CompositeInputService _inputService;

    public CompositeInputServiceTests()
    {
        _mockKeyboard = Substitute.For<IKeyboardInputProvider>();
        _mockController = Substitute.For<IControllerInputProvider>();
        _inputService = new CompositeInputService(_mockKeyboard, _mockController);
    }

    [Fact]
    public void Constructor_WithNullKeyboard_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CompositeInputService(null!, _mockController));
    }

    [Fact]
    public void Constructor_WithNullController_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CompositeInputService(_mockKeyboard, null!));
    }

    [Fact]
    public void Update_CallsUpdateOnBothProviders()
    {
        // Arrange
        var gameTime = new GameTime();

        // Act
        _inputService.Update(gameTime);

        // Assert
        _mockKeyboard.Received(1).Update(gameTime);
        _mockController.Received(1).Update(gameTime);
    }

    [Fact]
    public void GetMovementDirection_WithKeyboardWASD_ReturnsCorrectDirection()
    {
        // Arrange
        _mockKeyboard.IsKeyDown(Keys.W).Returns(true);
        _mockKeyboard.IsKeyDown(Keys.D).Returns(true);
        _mockController.IsConnected.Returns(false);

        // Act
        var direction = _inputService.GetMovementDirection();

        // Assert
        var expected = Vector2.Normalize(new Vector2(1f, -1f));
        Assert.Equal(expected.X, direction.X, 3);
        Assert.Equal(expected.Y, direction.Y, 3);
    }

    [Fact]
    public void GetMovementDirection_WithControllerThumbstick_ReturnsCorrectDirection()
    {
        // Arrange
        _mockController.IsConnected.Returns(true);
        _mockController.LeftThumbstick.Returns(new Vector2(0.8f, 0.6f));

        // Act
        var direction = _inputService.GetMovementDirection();

        // Assert
        var expected = Vector2.Normalize(new Vector2(0.8f, -0.6f)); // Y inverted
        Assert.Equal(expected.X, direction.X, 3);
        Assert.Equal(expected.Y, direction.Y, 3);
    }

    [Fact]
    public void GetMovementDirection_WithBothInputs_CombinesAndNormalizes()
    {
        // Arrange
        _mockKeyboard.IsKeyDown(Keys.A).Returns(true); // Left
        _mockController.IsConnected.Returns(true);
        _mockController.LeftThumbstick.Returns(new Vector2(0.5f, 0f));

        // Act
        var direction = _inputService.GetMovementDirection();

        // Assert
        var combined = new Vector2(-1f + 0.5f, 0f);
        var expected = Vector2.Normalize(combined);
        Assert.Equal(expected.X, direction.X, 3);
        Assert.Equal(expected.Y, direction.Y, 3);
    }

    [Fact]
    public void IsExitPressed_WithKeyboardEscape_ReturnsTrue()
    {
        // Arrange
        _mockKeyboard.IsKeyDown(Keys.Escape).Returns(true);

        // Act
        var result = _inputService.IsExitPressed();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExitPressed_WithControllerBack_ReturnsTrue()
    {
        // Arrange
        _mockController.IsConnected.Returns(true);
        _mockController.IsButtonDown(Buttons.Back).Returns(true);

        // Act
        var result = _inputService.IsExitPressed();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExitPressed_WithNoInput_ReturnsFalse()
    {
        // Arrange
        _mockKeyboard.IsKeyDown(Keys.Escape).Returns(false);
        _mockController.IsConnected.Returns(true);
        _mockController.IsButtonDown(Buttons.Back).Returns(false);

        // Act
        var result = _inputService.IsExitPressed();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsPrimaryActionPressed_WithKeyboardSpace_ReturnsTrue()
    {
        // Arrange
        _mockKeyboard.IsKeyPressed(Keys.Space).Returns(true);

        // Act
        var result = _inputService.IsPrimaryActionPressed();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPrimaryActionPressed_WithControllerA_ReturnsTrue()
    {
        // Arrange
        _mockController.IsConnected.Returns(true);
        _mockController.IsButtonPressed(Buttons.A).Returns(true);

        // Act
        var result = _inputService.IsPrimaryActionPressed();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSecondaryActionPressed_WithKeyboardShift_ReturnsTrue()
    {
        // Arrange
        _mockKeyboard.IsKeyPressed(Keys.LeftShift).Returns(true);

        // Act
        var result = _inputService.IsSecondaryActionPressed();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSecondaryActionPressed_WithControllerB_ReturnsTrue()
    {
        // Arrange
        _mockController.IsConnected.Returns(true);
        _mockController.IsButtonPressed(Buttons.B).Returns(true);

        // Act
        var result = _inputService.IsSecondaryActionPressed();

        // Assert
        Assert.True(result);
    }
}
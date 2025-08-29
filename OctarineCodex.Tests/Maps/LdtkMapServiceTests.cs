using FluentAssertions;
using OctarineCodex.Maps;
using System.Text.Json;
using Xunit;

namespace OctarineCodex.Tests.Maps;

public class LdtkMapServiceTests
{
    private readonly LdtkMapService _service;

    public LdtkMapServiceTests()
    {
        _service = new LdtkMapService();
    }

    [Fact]
    public void IsProjectLoaded_WhenNoProjectLoaded_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = _service.IsProjectLoaded;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentProject_WhenNoProjectLoaded_ShouldReturnNull()
    {
        // Arrange & Act
        var result = _service.GetCurrentProject();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllLevels_WhenNoProjectLoaded_ShouldReturnEmptyArray()
    {
        // Arrange & Act
        var result = _service.GetAllLevels();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetLevel_WhenNoProjectLoaded_ShouldReturnNull()
    {
        // Arrange & Act
        var result = _service.GetLevel("TestLevel");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadProjectAsync_WithValidJsonString_ShouldLoadProject()
    {
        // Arrange
        var testJson = CreateTestLdtkJson();
        var tempFile = await CreateTempFileAsync(testJson);

        try
        {
            // Act
            var result = await _service.LoadProjectAsync(tempFile);

            // Assert
            result.Should().NotBeNull();
            result!.Levels.Should().HaveCount(1);
            result.Levels[0].Identifier.Should().Be("Level_0");
            result.DefaultGridSize.Should().Be(16);
            _service.IsProjectLoaded.Should().BeTrue();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectAsync_WithInvalidFilePath_ShouldReturnNull()
    {
        // Arrange
        var invalidPath = "non_existent_file.ldtk";

        // Act
        var result = await _service.LoadProjectAsync(invalidPath);

        // Assert
        result.Should().BeNull();
        _service.IsProjectLoaded.Should().BeFalse();
    }

    [Fact]
    public async Task LoadProjectAsync_WithInvalidJson_ShouldReturnNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var tempFile = await CreateTempFileAsync(invalidJson);

        try
        {
            // Act
            var result = await _service.LoadProjectAsync(tempFile);

            // Assert
            result.Should().BeNull();
            _service.IsProjectLoaded.Should().BeFalse();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetLevel_AfterLoadingProject_ShouldReturnCorrectLevel()
    {
        // Arrange
        var testJson = CreateTestLdtkJson();
        var tempFile = await CreateTempFileAsync(testJson);

        try
        {
            await _service.LoadProjectAsync(tempFile);

            // Act
            var result = _service.GetLevel("Level_0");

            // Assert
            result.Should().NotBeNull();
            result!.Identifier.Should().Be("Level_0");
            result.PixelWidth.Should().Be(256);
            result.PixelHeight.Should().Be(256);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetLevel_WithNonExistentIdentifier_ShouldReturnNull()
    {
        // Arrange
        var testJson = CreateTestLdtkJson();
        var tempFile = await CreateTempFileAsync(testJson);

        try
        {
            await _service.LoadProjectAsync(tempFile);

            // Act
            var result = _service.GetLevel("NonExistentLevel");

            // Assert
            result.Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetAllLevels_AfterLoadingProject_ShouldReturnAllLevels()
    {
        // Arrange
        var testJson = CreateTestLdtkJson();
        var tempFile = await CreateTempFileAsync(testJson);

        try
        {
            await _service.LoadProjectAsync(tempFile);

            // Act
            var result = _service.GetAllLevels();

            // Assert
            result.Should().HaveCount(1);
            result[0].Identifier.Should().Be("Level_0");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static string CreateTestLdtkJson()
    {
        var testProject = new LdtkProject
        {
            Levels = [
                new LdtkLevel
                {
                    Identifier = "Level_0",
                    Uid = 0,
                    PixelWidth = 256,
                    PixelHeight = 256,
                    WorldX = 0,
                    WorldY = 0,
                    LayerInstances = [
                        new LdtkLayerInstance
                        {
                            Identifier = "Tiles",
                            Type = "Tiles",
                            CellWidth = 16,
                            CellHeight = 16,
                            GridSize = 16
                        }
                    ]
                }
            ],
            Definitions = new LdtkDefinitions
            {
                Tilesets = [],
                Entities = [],
                Layers = []
            },
            DefaultGridSize = 16,
            WorldGridWidth = 256,
            WorldGridHeight = 256
        };

        return JsonSerializer.Serialize(testProject, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    private static async Task<string> CreateTempFileAsync(string content)
    {
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);
        return tempFile;
    }
}
using System.IO;
using Xunit;
using FluentAssertions;

namespace OctarineCodex.Tests.Maps;

/// <summary>
/// Tests to investigate the LDTK file path resolution issue.
/// </summary>
public class PathResolutionTests
{
    [Fact]
    public void PathResolution_ShouldShowCurrentWorkingDirectoryAndContentPath()
    {
        // Arrange - simulate what Game1 does
        var contentRootDirectory = "Content";
        var currentWorkingDirectory = Directory.GetCurrentDirectory();
        var ldtkPath = Path.Combine(contentRootDirectory, "test_level2.ldtk");
        var fullResolvedPath = Path.GetFullPath(ldtkPath);
        var fileExists = File.Exists(ldtkPath);
        
        // Act - verify path construction behavior
        
        // Assert - this test is mainly for debugging, but let's check basic expectations
        currentWorkingDirectory.Should().NotBeNullOrEmpty("Current working directory should be set");
        ldtkPath.Should().Be(Path.Combine("Content", "test_level2.ldtk"));
    }
    
    [Fact]
    public void PathResolution_ShouldFindCorrectPathForLdtkFile()
    {
        // Arrange - try to find the correct path that should be used
        var currentWorkingDirectory = Directory.GetCurrentDirectory();
        var possiblePaths = new[]
        {
            // Relative from current directory
            Path.Combine("Content", "test_level2.ldtk"),
            Path.Combine("OctarineCodex", "Content", "test_level2.ldtk"),
            
            // Relative from parent directory
            Path.Combine("..", "OctarineCodex", "Content", "test_level2.ldtk"),
            
            // From bin directory (where the executable might run)
            Path.Combine("OctarineCodex", "bin", "Debug", "net8.0", "Content", "test_level2.ldtk"),
            Path.Combine("..", "OctarineCodex", "bin", "Debug", "net8.0", "Content", "test_level2.ldtk"),
            
            // Absolute path
            Path.Combine(currentWorkingDirectory, "OctarineCodex", "Content", "test_level2.ldtk")
        };
        
        string? workingPath = null;
        foreach (var path in possiblePaths)
        {
            var exists = File.Exists(path);
            if (exists && workingPath == null)
            {
                workingPath = path;
            }
        }
        
        // Assert
        workingPath.Should().NotBeNull("Should find at least one valid path to test_level2.ldtk");
    }
    
    [Fact]
    public void PathResolution_ShouldFindLdtkFileInOutputDirectory()
    {
        // Arrange - simulate what happens when the game runs from the output directory
        var outputDirectory = @"C:\Users\siriu\RiderProjects\OctarineCodex\OctarineCodex\bin\Debug\net8.0";
        var contentPath = "Content";
        var fileName = "test_level2.ldtk";
        
        // This simulates the game running from the output directory
        var ldtkPath = Path.Combine(contentPath, fileName);
        var fullPathFromOutput = Path.Combine(outputDirectory, ldtkPath);
        
        // Assert - the file should now exist in the output directory
        File.Exists(fullPathFromOutput).Should().BeTrue(
            $"LDTK file should be copied to output directory at '{fullPathFromOutput}'"
        );
    }
}
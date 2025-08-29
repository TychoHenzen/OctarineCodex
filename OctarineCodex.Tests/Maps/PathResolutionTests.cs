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
        
        // Act & Debug output
        System.Console.WriteLine($"[DEBUG_LOG] Current working directory: '{currentWorkingDirectory}'");
        System.Console.WriteLine($"[DEBUG_LOG] Content.RootDirectory: '{contentRootDirectory}'");
        System.Console.WriteLine($"[DEBUG_LOG] Constructed LDTK path: '{ldtkPath}'");
        System.Console.WriteLine($"[DEBUG_LOG] Full resolved LDTK path: '{fullResolvedPath}'");
        System.Console.WriteLine($"[DEBUG_LOG] LDTK file exists: {fileExists}");
        
        // Check if the expected file exists in the project directory
        var projectRoot = Directory.GetCurrentDirectory();
        var expectedPath1 = Path.Combine(projectRoot, "OctarineCodex", "Content", "test_level2.ldtk");
        var expectedPath2 = Path.Combine(projectRoot, "..", "OctarineCodex", "Content", "test_level2.ldtk");
        var expectedPath3 = Path.Combine(projectRoot, "Content", "test_level2.ldtk");
        
        System.Console.WriteLine($"[DEBUG_LOG] Checking expected path 1: '{expectedPath1}' - Exists: {File.Exists(expectedPath1)}");
        System.Console.WriteLine($"[DEBUG_LOG] Checking expected path 2: '{expectedPath2}' - Exists: {File.Exists(expectedPath2)}");
        System.Console.WriteLine($"[DEBUG_LOG] Checking expected path 3: '{expectedPath3}' - Exists: {File.Exists(expectedPath3)}");
        
        // Find where the file actually is
        var searchDirs = new[]
        {
            Path.Combine(projectRoot, "OctarineCodex", "Content"),
            Path.Combine(projectRoot, "..", "OctarineCodex", "Content"),
            Path.Combine(projectRoot, "Content"),
            Path.Combine(projectRoot, "OctarineCodex", "bin", "Debug", "net8.0", "Content"),
            Path.Combine(projectRoot, "..", "OctarineCodex", "bin", "Debug", "net8.0", "Content")
        };
        
        foreach (var searchDir in searchDirs)
        {
            var searchPath = Path.Combine(searchDir, "test_level2.ldtk");
            if (File.Exists(searchPath))
            {
                System.Console.WriteLine($"[DEBUG_LOG] ✓ Found file at: '{searchPath}'");
            }
        }
        
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
        
        System.Console.WriteLine($"[DEBUG_LOG] Searching for test_level2.ldtk in various locations:");
        
        string? workingPath = null;
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            var exists = File.Exists(path);
            System.Console.WriteLine($"[DEBUG_LOG] Path: '{path}' -> Full: '{fullPath}' -> Exists: {exists}");
            
            if (exists && workingPath == null)
            {
                workingPath = path;
            }
        }
        
        // Assert
        workingPath.Should().NotBeNull("Should find at least one valid path to test_level2.ldtk");
        
        if (workingPath != null)
        {
            System.Console.WriteLine($"[DEBUG_LOG] ✓ Recommended path to use: '{workingPath}'");
        }
    }
    
    [Fact]
    public void PathResolution_ShouldFindLdtkFileInOutputDirectory()
    {
        // Arrange - simulate what happens when the game runs from the output directory
        var currentTestDir = Directory.GetCurrentDirectory();
        System.Console.WriteLine($"[DEBUG_LOG] Current test directory: '{currentTestDir}'");
        
        // Use the known correct absolute path to the main project's output directory
        var outputDirectory = @"C:\Users\siriu\RiderProjects\OctarineCodex\OctarineCodex\bin\Debug\net8.0";
        var contentPath = "Content";
        var fileName = "test_level2.ldtk";
        
        // This simulates the game running from the output directory
        var ldtkPath = Path.Combine(contentPath, fileName);
        var fullPathFromOutput = Path.Combine(outputDirectory, ldtkPath);
        
        // Act & Debug
        System.Console.WriteLine($"[DEBUG_LOG] Simulating game running from: '{outputDirectory}'");
        System.Console.WriteLine($"[DEBUG_LOG] Game would look for: '{ldtkPath}'");
        System.Console.WriteLine($"[DEBUG_LOG] Full path from output dir: '{fullPathFromOutput}'");
        System.Console.WriteLine($"[DEBUG_LOG] File exists at output location: {File.Exists(fullPathFromOutput)}");
        
        // Assert - the file should now exist in the output directory
        File.Exists(fullPathFromOutput).Should().BeTrue(
            $"LDTK file should be copied to output directory at '{fullPathFromOutput}'"
        );
        
        System.Console.WriteLine("[DEBUG_LOG] ✓ LDTK file path resolution issue is now fixed!");
    }
}
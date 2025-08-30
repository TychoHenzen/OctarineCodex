using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LDtk;
using OctarineCodex.Maps;
using Xunit;

namespace OctarineCodex.Tests.Maps;

public class SimpleMapServiceDebugTests
{
    [Fact]
    public async Task DebugLoadRoom1File()
    {
        // Debug: Check file paths and loading
        // From test output dir: bin/Debug/net8.0, need to go up to project root, then to OctarineCodex/Content
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        var fileExists = File.Exists(filePath);
        
        Console.WriteLine($"[DEBUG_LOG] File path: {filePath}");
        Console.WriteLine($"[DEBUG_LOG] Current directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"[DEBUG_LOG] File exists: {fileExists}");
        
        // Assert to show values in test output
        Assert.True(fileExists, $"File should exist at {filePath}, current dir: {Directory.GetCurrentDirectory()}");
        
        // Test SimpleMapService directly (it has fallback mechanism for single-world files)
        Console.WriteLine($"[DEBUG_LOG] Testing SimpleMapService...");
        var mapService = new SimpleMapService();
        
        try 
        {
            var resultLevel = await mapService.LoadLevelAsync(filePath);
            Console.WriteLine($"[DEBUG_LOG] SimpleMapService result: {(resultLevel != null ? $"Level {resultLevel.Identifier}" : "null")}");
            
            if (resultLevel != null)
            {
                Console.WriteLine($"[DEBUG_LOG] Level details - Width: {resultLevel.PxWid}, Height: {resultLevel.PxHei}");
                Console.WriteLine($"[DEBUG_LOG] Level WorldX: {resultLevel.WorldX}, WorldY: {resultLevel.WorldY}");
            }
            
            // Add assertions to capture the results in test output
            if (resultLevel == null)
            {
                Assert.True(false, "SimpleMapService should have loaded a level but returned null");
            }
            
            Assert.Equal("AutoLayer", resultLevel.Identifier);
            Assert.Equal(296, resultLevel.PxWid);
            Assert.Equal(208, resultLevel.PxHei);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG_LOG] SimpleMapService failed: {ex.Message}");
            Console.WriteLine($"[DEBUG_LOG] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
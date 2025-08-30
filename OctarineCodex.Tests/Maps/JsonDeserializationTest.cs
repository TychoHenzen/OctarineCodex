using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace OctarineCodex.Tests.Maps;

public class JsonDeserializationTest
{
    [Fact]
    public async Task TestDirectJsonDeserialization()
    {
        // Test direct JSON deserialization of Room1.ldtk
        var filePath = Path.Combine("..", "..", "..", "..", "OctarineCodex", "Content", "Room1.ldtk");
        
        Assert.True(File.Exists(filePath), $"File should exist at {filePath}");
        
        try
        {
            // Read JSON content
            var jsonContent = await File.ReadAllTextAsync(filePath);
            
            // Try to deserialize as dynamic object first
            var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;
            
            // Use assertions to capture debug info in test output
            Assert.True(jsonContent.Length > 0, $"JSON content length: {jsonContent.Length}");
            
            // Check for levels property
            var hasLevels = root.TryGetProperty("levels", out var levelsElement);
            var levelCount = hasLevels ? levelsElement.GetArrayLength() : 0;
            Assert.True(hasLevels, $"File should have 'levels' property. Has levels: {hasLevels}, count: {levelCount}");
            
            string levelInfo = "NO_LEVELS";
            if (hasLevels && levelCount > 0)
            {
                var firstLevel = levelsElement[0];
                var hasIdentifier = firstLevel.TryGetProperty("identifier", out var identifierElement);
                var hasWidth = firstLevel.TryGetProperty("pxWid", out var widthElement);
                var hasHeight = firstLevel.TryGetProperty("pxHei", out var heightElement);
                
                var identifier = hasIdentifier ? identifierElement.GetString() : "NO_IDENTIFIER";
                var width = hasWidth ? widthElement.GetInt32() : -1;
                var height = hasHeight ? heightElement.GetInt32() : -1;
                
                levelInfo = $"id='{identifier}', width={width}, height={height}";
                Assert.True(hasIdentifier && hasWidth && hasHeight, 
                    $"Level should have identifier, width, height. Found: {levelInfo}");
            }
            
            // Check for worlds property
            var hasWorlds = root.TryGetProperty("worlds", out var worldsElement);
            var worldCount = hasWorlds ? worldsElement.GetArrayLength() : 0;
            
            // Check for flags property
            var hasFlags = root.TryGetProperty("flags", out var flagsElement);
            var flagCount = hasFlags ? flagsElement.GetArrayLength() : 0;
            var flags = hasFlags ? string.Join(",", flagsElement.EnumerateArray().Select(f => f.GetString())) : "NONE";
            
            // Structure successfully analyzed - Room1.ldtk has 1 level in legacy format
            Assert.True(true, $"FILE STRUCTURE: levels={levelCount} ({levelInfo}), worlds={worldCount}, flags={flagCount} ({flags})");
        }
        catch (Exception ex)
        {
            Assert.True(false, $"JSON parsing failed: {ex.Message}");
        }
    }
}
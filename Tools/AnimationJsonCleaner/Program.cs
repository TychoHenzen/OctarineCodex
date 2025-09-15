using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AnimationJsonCleaner;

/// <summary>
///     Post-processing tool to remove "empty" frame data from Aseprite-exported JSON files.
///     Removes frames that have no tag name (e.g., ".14", ".136") while keeping tagged frames (e.g., "Still_Right.0",
///     "Walk_Left.2").
/// </summary>
public class Program
{
    private static readonly Regex EmptyFramePattern = new(@"^\.(\d+)$", RegexOptions.Compiled);

    public static async Task<int> Main(string[] args)
    {
        var inputPath = args.Length > 0
            ? args[0]
            : @"C:\Users\siriu\RiderProjects\OctarineCodex\OctarineCodex\Content\animation.json";

        var outputPath = args.Length > 1 ? args[1] : inputPath;

        try
        {
            Console.WriteLine($"Cleaning animation JSON: {inputPath}");

            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: File not found: {inputPath}");
                return 1;
            }

            // Read and parse JSON
            var jsonText = await File.ReadAllTextAsync(inputPath);
            JsonDocument jsonDocument = JsonDocument.Parse(jsonText);
            var rootObject = JsonObject.Create(jsonDocument.RootElement)!;

            // Get frames section
            if (!rootObject.TryGetPropertyValue("frames", out JsonNode? framesNode) ||
                framesNode is not JsonObject framesObject)
            {
                Console.Error.WriteLine("Error: No 'frames' section found in JSON");
                return 1;
            }

            // Count frames before cleaning
            var originalCount = framesObject.Count;

            // Find and remove empty frames (those that start with just a dot and digits)
            List<string> emptyFrameKeys = framesObject.Select(kvp => kvp.Key)
                .Where(key => EmptyFramePattern.IsMatch(key))
                .ToList();

            foreach (var key in emptyFrameKeys)
            {
                framesObject.Remove(key);
            }

            // Count frames after cleaning
            var cleanedCount = framesObject.Count;
            var removedCount = originalCount - cleanedCount;

            // Write cleaned JSON back
            var options = new JsonSerializerOptions
            {
                WriteIndented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var cleanedJson = JsonSerializer.Serialize(rootObject, options);
            await File.WriteAllTextAsync(outputPath, cleanedJson);

            Console.WriteLine("✅ Cleaning complete!");
            Console.WriteLine($"   Original frames: {originalCount}");
            Console.WriteLine($"   Cleaned frames:  {cleanedCount}");
            Console.WriteLine($"   Removed frames:  {removedCount}");
            Console.WriteLine($"   Output saved to: {outputPath}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}

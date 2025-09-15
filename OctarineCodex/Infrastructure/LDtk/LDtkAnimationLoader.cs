// Infrastructure/LDtk/LDtkAnimationLoader.cs

using System.Collections.Generic;
using System.Linq;
using LDtk;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Infrastructure.LDtk;

/// <summary>
///     Loads animation data from LDtk custom fields and converts them to animation components.
///     Supports both entity and tileset animation definitions.
/// </summary>
public static class LDtkAnimationLoader
{
    /// <summary>
    ///     Loads animation data from LDtk entity custom fields.
    ///     Expected fields:
    ///     - animFrames: Array&lt;Int&gt; - Tile IDs for each frame
    ///     - animSpeed: Float - Animation speed in FPS
    ///     - animLoop: Bool - Whether animation loops
    ///     - animType: Enum(Simple,Triggered,StateMachine) - Animation type
    ///     - animLayers: Array&lt;String&gt; - For layered character animations.
    /// </summary>
    public static LDtkAnimationData? LoadFromEntity(EntityInstance entity)
    {
        var animFrames = GetFieldValue<int[]>(entity, "animFrames");
        if (animFrames == null || animFrames.Length == 0)
        {
            return null;
        }

        var animSpeed = GetFieldValue<float?>(entity, "animSpeed") ?? 12f;
        var animLoop = GetFieldValue<bool?>(entity, "animLoop") ?? true;
        var animType = GetFieldValue<string?>(entity, "animType") ?? "Simple";
        var nextAnimation = GetFieldValue<string?>(entity, "animNext");
        return LDtkAnimationData.FromLDtkFields(
            entity._Identifier,
            animFrames,
            animSpeed,
            animLoop,
            animType,
            nextAnimation);
    }

    /// <summary>
    ///     Loads layered animation data for character entities.
    /// </summary>
    public static Dictionary<string, Dictionary<string, LDtkAnimationData>> LoadLayeredAnimations(EntityInstance entity)
    {
        var result = new Dictionary<string, Dictionary<string, LDtkAnimationData>>();

        var animLayers = GetFieldValue<string[]>(entity, "animLayers");
        if (animLayers == null)
        {
            return result;
        }

        foreach (var layerName in animLayers)
        {
            var layerAnimations = new Dictionary<string, LDtkAnimationData>();

            // Load different animation states for this layer
            var states = new[] { "Idle", "Walk", "Attack", "Death" };
            foreach (var state in states)
            {
                var frames = GetFieldValue<int[]>(entity, $"{layerName}_{state}_Frames");
                if (frames != null && frames.Length > 0)
                {
                    var speed = GetFieldValue<float?>(entity, $"{layerName}_{state}_Speed") ?? 12f;
                    var loop = state != "Death"; // Death animations don't loop

                    layerAnimations[state] = LDtkAnimationData.FromLDtkFields(
                        $"{layerName}_{state}",
                        frames,
                        speed,
                        loop);
                }
            }

            if (layerAnimations.Count > 0)
            {
                result[layerName] = layerAnimations;
            }
        }

        return result;
    }

    /// <summary>
    ///     Loads tile animation data from tileset custom metadata.
    /// </summary>
    public static Dictionary<int, LDtkAnimationData> LoadTileAnimations(TilesetDefinition tileset)
    {
        var result = new Dictionary<int, LDtkAnimationData>();

        // Look for tiles with animation metadata
        foreach (TileCustomMetadata? customData in tileset.CustomData)
        {
            if (TryParseAnimationData(customData.Data, out LDtkAnimationData? animData))
            {
                result[customData.TileId] = animData.Value;
            }
        }

        return result;
    }

    private static T? GetFieldValue<T>(EntityInstance entity, string fieldName)
    {
        FieldInstance? field = entity.FieldInstances?.FirstOrDefault(f => f._Identifier == fieldName);
        return field?._Value is T value ? value : default(T?);
    }

    private static bool TryParseAnimationData(string customData, out LDtkAnimationData? animData)
    {
        animData = null;

        // Simple format: "anim:startTileId,frameCount,frameRate"
        if (customData.StartsWith("anim:"))
        {
            var parts = customData.Substring(5).Split(',');
            if (parts.Length >= 3 &&
                int.TryParse(parts[0], out var startTileId) &&
                int.TryParse(parts[1], out var frameCount) &&
                float.TryParse(parts[2], out var frameRate))
            {
                animData = LDtkAnimationData.CreateSimple("TileAnimation", startTileId, frameCount, frameRate);
                return true;
            }
        }

        return false;
    }
}

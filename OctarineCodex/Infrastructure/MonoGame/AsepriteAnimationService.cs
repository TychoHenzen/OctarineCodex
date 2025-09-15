// OctarineCodex/Infrastructure/MonoGame/AsepriteAnimationService.cs

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Infrastructure.MonoGame;

public class AsepriteAnimationService(ContentManager content) : IAsepriteAnimationService
{
    private readonly Dictionary<string, Dictionary<string, AsepriteAnimation>> _loadedAnimations = new();
    private readonly Dictionary<string, AsepriteAnimationData> _loadedData = new();

    public Dictionary<string, AsepriteAnimation> LoadAnimations(string jsonPath)
    {
        if (_loadedAnimations.TryGetValue(jsonPath, out Dictionary<string, AsepriteAnimation>? cachedAnimations))
        {
            return cachedAnimations;
        }

        // Load JSON from content pipeline
        var jsonContent = File.ReadAllText(content.RootDirectory + "/" + jsonPath);
        AsepriteAnimationData asepriteData = AsepriteAnimationData.FromJson(jsonContent);

        Dictionary<string, AsepriteAnimation> animations = AsepriteAnimationLoader.LoadAnimations(asepriteData);

        // Cache for future use
        _loadedData[jsonPath] = asepriteData;
        _loadedAnimations[jsonPath] = animations;

        return animations;
    }

    public AsepriteAnimationComponent CreateAnimationComponent(string jsonPath)
    {
        Dictionary<string, AsepriteAnimation> animations = LoadAnimations(jsonPath);
        AsepriteAnimationData asepriteData = _loadedData[jsonPath];

        return new AsepriteAnimationComponent(animations, asepriteData);
    }

    public LDtkAnimationData ConvertToLDtkFormat(string animationName, string jsonPath)
    {
        Dictionary<string, AsepriteAnimation> animations = LoadAnimations(jsonPath);

        if (animations.TryGetValue(animationName, out AsepriteAnimation? animation))
        {
            return AsepriteToLDtkBridge.ConvertToLDtkFormat(animation);
        }

        throw new ArgumentException($"Animation '{animationName}' not found in {jsonPath}");
    }
}

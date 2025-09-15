// OctarineCodex/Infrastructure/MonoGame/AsepriteAnimationService.cs

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Infrastructure.MonoGame;

/// <summary>
///     Service for loading and managing Aseprite-based animations in MonoGame
///     Integrates with existing content pipeline and magic system
/// </summary>
[Service<AsepriteAnimationService>]
public interface IAsepriteAnimationService
{
    Dictionary<string, AsepriteAnimation> LoadAnimations(string jsonPath);
    AsepriteAnimationComponent CreateAnimationComponent(string jsonPath);
    LDtkAnimationData ConvertToLDtkFormat(string animationName, string jsonPath);
}

public class AsepriteAnimationService : IAsepriteAnimationService
{
    private readonly AsepriteToLDtkBridge _bridge;
    private readonly ContentManager _content;
    private readonly Dictionary<string, Dictionary<string, AsepriteAnimation>> _loadedAnimations;
    private readonly Dictionary<string, AsepriteAnimationData> _loadedData;
    private readonly AsepriteAnimationLoader _loader;

    public AsepriteAnimationService(ContentManager content)
    {
        _loader = new AsepriteAnimationLoader();
        _bridge = new AsepriteToLDtkBridge();
        _loadedData = new Dictionary<string, AsepriteAnimationData>();
        _loadedAnimations = new Dictionary<string, Dictionary<string, AsepriteAnimation>>();
        _content = content;
    }

    public Dictionary<string, AsepriteAnimation> LoadAnimations(string jsonPath)
    {
        if (_loadedAnimations.TryGetValue(jsonPath, out Dictionary<string, AsepriteAnimation>? cachedAnimations))
        {
            return cachedAnimations;
        }

        // Load JSON from content pipeline
        var jsonContent = File.ReadAllText(_content.RootDirectory + "/" + jsonPath);
        AsepriteAnimationData asepriteData = AsepriteAnimationData.FromJson(jsonContent);

        Dictionary<string, AsepriteAnimation> animations = _loader.LoadAnimations(asepriteData);

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
            return _bridge.ConvertToLDtkFormat(animation);
        }

        throw new ArgumentException($"Animation '{animationName}' not found in {jsonPath}");
    }
}

using System.Collections.Generic;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Infrastructure.MonoGame;

/// <summary>
///     Service for loading and managing Aseprite-based animations in MonoGame
///     Integrates with existing content pipeline and magic system.
/// </summary>
[Service<AsepriteAnimationService>]
public interface IAsepriteAnimationService
{
    Dictionary<string, AsepriteAnimation> LoadAnimations(string jsonPath);
    AsepriteAnimationComponent CreateAnimationComponent(string jsonPath);
    LDtkAnimationData ConvertToLDtkFormat(string animationName, string jsonPath);
}

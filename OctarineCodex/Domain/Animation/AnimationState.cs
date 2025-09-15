using System.Collections.Generic;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Animation state definition for complex character animations.
/// </summary>
public readonly record struct AnimationState(
    string Name,
    LDtkAnimationData Animation,
    Dictionary<string, string> Transitions);

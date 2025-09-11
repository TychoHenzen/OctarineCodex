// OctarineCodex/Collision/Messages/CollisionMessage.cs

using Microsoft.Xna.Framework;

namespace OctarineCodex.Application.Messages;

public sealed class CollisionMessage(
    string entityA,
    string entityB,
    Vector2 contactPoint,
    Vector2 contactNormal,
    float penetrationDepth)
{
    public string EntityA { get; } = entityA;
    public string EntityB { get; } = entityB;
    public Vector2 ContactPoint { get; } = contactPoint;
    public Vector2 ContactNormal { get; } = contactNormal;
    public float PenetrationDepth { get; } = penetrationDepth;
}

// OctarineCodex/Collision/Messages/TriggerEnterMessage.cs

// OctarineCodex/Collision/Messages/TriggerExitMessage.cs

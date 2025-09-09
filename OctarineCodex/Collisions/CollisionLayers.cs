using System;

namespace OctarineCodex.Collisions;

[Flags]
public enum CollisionLayers
{
    None = 0,
    Solid = 1 << 0, // Blocks all movement (walls, obstacles)
    Platform = 1 << 1, // One-way collision (jump through from below)
    Trigger = 1 << 2, // No collision but fires events
    Water = 1 << 3, // Special physics, partial collision
    Hazard = 1 << 4, // Damage-dealing areas
    Entity = 1 << 5, // Dynamic entities
    Projectile = 1 << 6, // Projectiles
    Shape = 1 << 7, // Shapes

    // Common combinations
    AllSolid = Solid | Platform,
    AllTriggers = Trigger | Water | Hazard,
    All = ~None
}

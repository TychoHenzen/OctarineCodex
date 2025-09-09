using Microsoft.Xna.Framework;

namespace OctarineCodex.Collisions;

public sealed class CollisionTestResult
{
    public bool Hit { get; }
    public Vector2 Point { get; }
    public Vector2 Normal { get; }
    public float Distance { get; }
    public string? EntityId { get; }
    public Point? TileCoordinate { get; }
    public CollisionLayers Layers { get; }

    public CollisionTestResult(
        bool hit,
        Vector2 point = default,
        Vector2 normal = default,
        float distance = 0f,
        string? entityId = null,
        Point? tileCoordinate = null,
        CollisionLayers layers = CollisionLayers.None)
    {
        Hit = hit;
        Point = point;
        Normal = normal;
        Distance = distance;
        EntityId = entityId;
        TileCoordinate = tileCoordinate;
        Layers = layers;
    }

    public CollisionTestResult(
        Vector2 point = default,
        Vector2 normal = default,
        float distance = 0f)
    {
        Hit = true;
        Point = point;
        Normal = normal;
        Distance = distance;
        EntityId = null;
        TileCoordinate = null;
        Layers = CollisionLayers.Shape;
    }

    public static CollisionTestResult NoHit()
    {
        return new CollisionTestResult(false);
    }

    public static CollisionTestResult HitTile(Vector2 point, Vector2 normal, float distance, Point tile,
        CollisionLayers layers)
    {
        return new CollisionTestResult(true, point, normal, distance, null, tile, layers);
    }

    public static CollisionTestResult HitEntity(Vector2 point, Vector2 normal, float distance, string entityId,
        CollisionLayers layers)
    {
        return new CollisionTestResult(true, point, normal, distance, entityId, null, layers);
    }
}

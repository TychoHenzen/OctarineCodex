using System;
using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Extensions;
using OctarineCodex.Logging;
using OctarineCodex.Messages;
using OctarineCodex.Messaging;

namespace OctarineCodex.Collisions;

public sealed class CollisionSystem(IMessageBus messageBus, ILoggingService logger) : ICollisionSystem
{
    private readonly Dictionary<Point, CollisionLayers> _tileCollision = new();
    private readonly Dictionary<string, (CollisionComponent Component, Vector2 Position)> _entities = new();
    private readonly Dictionary<string, HashSet<string>> _currentTriggerOverlaps = new();
    private readonly HashSet<(string, string)> _processedCollisions = new();
    private int _tileSize = 16;

    public void InitializeLevels(IEnumerable<LDtkLevel> levels)
    {
        _tileCollision.Clear();
        var totalCollisionTiles = 0;
        var processedLayers = 0;

        foreach (LDtkLevel level in levels)
        {
            if (level.LayerInstances == null)
            {
                continue;
            }

            // Find collision layers by identifier - ONLY process actual collision layers
            List<LayerInstance> collisionLayers = level.LayerInstances
                .Where(l => l._Type == LayerType.IntGrid &&
                            (l._Identifier.Contains("Collision", StringComparison.OrdinalIgnoreCase) ||
                             l._Identifier.Contains("Solid", StringComparison.OrdinalIgnoreCase) ||
                             l._Identifier.Contains("Wall", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (LayerInstance layer in collisionLayers)
            {
                processedLayers++;
                _tileSize = layer._GridSize;
                var layerType = CollisionLayers.Solid; // Default to solid for collision layers

                var layerTileCount = 0;
                for (var i = 0; i < layer.IntGridCsv.Length; i++)
                {
                    var value = layer.IntGridCsv[i];
                    if (value <= 0)
                    {
                        continue;
                    }

                    var gridX = i % layer._CWid;
                    var gridY = i / layer._CWid;

                    var worldPixelX = level.WorldX + (gridX * _tileSize);
                    var worldPixelY = level.WorldY + (gridY * _tileSize);

                    var collisionGridX = (int)Math.Floor((double)worldPixelX / _tileSize);
                    var collisionGridY = (int)Math.Floor((double)worldPixelY / _tileSize);

                    var point = new Point(collisionGridX, collisionGridY);

                    // Combine layers if multiple apply to same tile
                    if (_tileCollision.ContainsKey(point))
                    {
                        _tileCollision[point] |= layerType;
                    }
                    else
                    {
                        _tileCollision[point] = layerType;
                    }

                    layerTileCount++;
                    totalCollisionTiles++;
                }
            }
        }
    }

    public void RegisterEntity(string entityId, CollisionComponent component, Vector2 position)
    {
        // Allow re-registration by overwriting existing entries
        _entities[entityId] = (component, position);

        if ((component.Layers & CollisionLayers.Trigger) != 0
            && !_currentTriggerOverlaps.ContainsKey(entityId))
        {
            _currentTriggerOverlaps[entityId] = [];
        }
    }

    public void UnregisterEntity(string entityId)
    {
        _entities.Remove(entityId);
        _currentTriggerOverlaps.Remove(entityId);

        // Clean up trigger overlaps referencing this entity
        foreach (HashSet<string> overlaps in _currentTriggerOverlaps.Values)
        {
            overlaps.Remove(entityId);
        }
    }

    public void UpdateEntityPosition(string entityId, Vector2 newPosition)
    {
        if (_entities.TryGetValue(entityId, out (CollisionComponent Component, Vector2 Position) entity))
        {
            _entities[entityId] = (entity.Component, newPosition);
        }
    }

    public CollisionTestResult Linecast(Vector2 start, Vector2 end, CollisionLayers layersMask = CollisionLayers.All)
    {
        Vector2 direction = end - start;
        var distance = direction.Length();

        if (distance.EqualsAppr(0))
        {
            return CollisionTestResult.NoHit();
        }

        direction.Normalize();
        return Raycast(start, direction, distance, layersMask);
    }

    public CollisionTestResult Raycast(Vector2 origin, Vector2 direction, float maxDistance = float.MaxValue,
        CollisionLayers layersMask = CollisionLayers.All)
    {
        direction.Normalize();
        CollisionTestResult closestHit = CollisionTestResult.NoHit();
        var minDistance = maxDistance;

        // Test against tiles using DDA algorithm
        CollisionTestResult tileHit = RaycastTiles(origin, direction, maxDistance, layersMask);
        if (tileHit.Hit && tileHit.Distance < minDistance)
        {
            closestHit = tileHit;
            minDistance = tileHit.Distance;
        }

        // Test against entities
        foreach (var (entityId, (component, position)) in _entities)
        {
            if ((component.Layers & layersMask) == 0)
            {
                continue;
            }

            ICollisionShape shape = TranslateShape(component.Shape, position);
            CollisionTestResult? hit =
                RayShapeIntersection(origin, direction, shape, minDistance);

            if (hit == null || hit.Distance >= minDistance)
            {
                continue;
            }

            minDistance = hit.Distance;
            closestHit = CollisionTestResult.HitEntity(
                hit.Point,
                hit.Normal,
                hit.Distance,
                entityId,
                component.Layers);
        }

        return closestHit;
    }

    public IEnumerable<CollisionTestResult> OverlapShape(ICollisionShape shape, Vector2 position,
        CollisionLayers layersMask = CollisionLayers.All)
    {
        var results = new List<CollisionTestResult>();
        ICollisionShape translatedShape = TranslateShape(shape, position);

        // Check tile overlaps
        Rectangle bounds = translatedShape.GetFinalBounds();
        Rectangle tileBounds = bounds.ToTileCoordinates(_tileSize);

        tileBounds.ForEachPosition(point =>
        {
            if (_tileCollision.TryGetValue(point, out CollisionLayers layer) && (layer & layersMask) != 0)
            {
                var tileRect = new Rectangle(point.X * _tileSize, point.Y * _tileSize, _tileSize, _tileSize);
                var tileShape = new BoxShape(tileRect);

                if (translatedShape.Intersects(tileShape))
                {
                    results.Add(CollisionTestResult.HitTile(
                        new Vector2(tileRect.Center.X, tileRect.Center.Y),
                        Vector2.Zero,
                        0,
                        point,
                        layer));
                }
            }
        });

        // Check entity overlaps
        foreach (var (entityId, (component, entityPos)) in _entities)
        {
            if ((component.Layers & layersMask) == 0)
            {
                continue;
            }

            ICollisionShape entityShape = TranslateShape(component.Shape, entityPos);
            if (translatedShape.Intersects(entityShape))
            {
                results.Add(CollisionTestResult.HitEntity(
                    entityPos,
                    Vector2.Zero,
                    Vector2.Distance(position, entityPos),
                    entityId,
                    component.Layers));
            }
        }

        return results;
    }

    public CollisionTestResult ShapeCast(ICollisionShape shape, Vector2 start, Vector2 end,
        CollisionLayers layersMask = CollisionLayers.All)
    {
        Vector2 direction = end - start;
        var distance = direction.Length();

        if (distance.EqualsAppr(0))
        {
            return CollisionTestResult.NoHit();
        }

        direction.Normalize();

        // Simplified shapecast using multiple raycasts from shape bounds
        Rectangle bounds = shape.GetFinalBounds();
        var testPoints = new[]
        {
            new Vector2(bounds.Left, bounds.Top), new Vector2(bounds.Right, bounds.Top),
            new Vector2(bounds.Left, bounds.Bottom), new Vector2(bounds.Right, bounds.Bottom),
            new Vector2(bounds.Center.X, bounds.Center.Y)
        };

        CollisionTestResult closestHit = CollisionTestResult.NoHit();
        var minDistance = distance;

        foreach (Vector2 point in testPoints)
        {
            Vector2 rayStart = start + point - new Vector2(bounds.Center.X, bounds.Center.Y);
            CollisionTestResult hit = Raycast(rayStart, direction, distance, layersMask);

            if (hit.Hit && hit.Distance < minDistance)
            {
                minDistance = hit.Distance;
                closestHit = hit;
            }
        }

        return closestHit;
    }

    public Vector2 ResolveMovement(string entityId, Vector2 currentPos, Vector2 desiredPos)
    {
        if (!_entities.TryGetValue(entityId, out (CollisionComponent Component, Vector2 Position) entity))
        {
            logger.Error($"CollisionSystem: Entity {entityId} not found in collision system");
            return desiredPos;
        }

        ICollisionShape shape = TranslateShape(entity.Component.Shape, desiredPos);
        Rectangle bounds = shape.GetFinalBounds();

        // Check tile collision
        if (CheckTileCollision(bounds, entity.Component.CollidesWith))
        {
            // Try horizontal movement only
            var horizontalPos = new Vector2(desiredPos.X, currentPos.Y);
            ICollisionShape horizontalShape = TranslateShape(entity.Component.Shape, horizontalPos);
            if (!CheckTileCollision(horizontalShape.GetFinalBounds(), entity.Component.CollidesWith))
            {
                return horizontalPos;
            }

            // Try vertical movement only
            var verticalPos = new Vector2(currentPos.X, desiredPos.Y);
            ICollisionShape verticalShape = TranslateShape(entity.Component.Shape, verticalPos);
            if (!CheckTileCollision(verticalShape.GetFinalBounds(), entity.Component.CollidesWith))
            {
                return verticalPos;
            }

            return currentPos;
        }

        return desiredPos;
    }

    public void ProcessCollisions()
    {
        _processedCollisions.Clear();

        // Check all entity pairs
        List<KeyValuePair<string, (CollisionComponent Component, Vector2 Position)>> entityList = _entities.ToList();
        for (var i = 0; i < entityList.Count; i++)
        {
            for (var j = i + 1; j < entityList.Count; j++)
            {
                var (idA, (compA, posA)) = entityList[i];
                var (idB, (compB, posB)) = entityList[j];

                if (!compA.CanCollideWith(compB))
                {
                    continue;
                }

                ICollisionShape shapeA = TranslateShape(compA.Shape, posA);
                ICollisionShape shapeB = TranslateShape(compB.Shape, posB);

                if (shapeA.Intersects(shapeB))
                {
                    ProcessCollisionPair(idA, compA, posA, idB, compB, posB);
                }
            }
        }

        // Process trigger exits
        foreach (var (triggerId, overlaps) in _currentTriggerOverlaps)
        {
            var toRemove = new List<string>();
            (CollisionComponent Component, Vector2 Position) triggerData = _entities[triggerId];

            foreach (var entityId in overlaps)
            {
                if (!_entities.TryGetValue(entityId, out (CollisionComponent Component, Vector2 Position) entityData))
                {
                    toRemove.Add(entityId);
                    continue;
                }

                ICollisionShape triggerShape = TranslateShape(triggerData.Component.Shape, triggerData.Position);
                ICollisionShape entityShape = TranslateShape(entityData.Component.Shape, entityData.Position);

                if (!triggerShape.Intersects(entityShape))
                {
                    toRemove.Add(entityId);
                    messageBus.SendMessage(new TriggerExitMessage(triggerId, entityId, triggerData.Component.Layers));
                }
            }

            foreach (var id in toRemove)
            {
                overlaps.Remove(id);
            }
        }
    }

    public void Clear()
    {
        _tileCollision.Clear();
        _entities.Clear();
        _currentTriggerOverlaps.Clear();
        _processedCollisions.Clear();
    }

    private static Vector2 GetBoxNormal(Rectangle box, Vector2 point)
    {
        var distLeft = Math.Abs(point.X - box.Left);
        var distRight = Math.Abs(point.X - box.Right);
        var distTop = Math.Abs(point.Y - box.Top);
        var distBottom = Math.Abs(point.Y - box.Bottom);

        var minDist = Math.Min(Math.Min(distLeft, distRight), Math.Min(distTop, distBottom));

        if (minDist.EqualsAppr(distLeft))
        {
            return new Vector2(-1, 0);
        }

        if (minDist.EqualsAppr(distRight))
        {
            return new Vector2(1, 0);
        }

        if (minDist.EqualsAppr(distTop))
        {
            return new Vector2(0, -1);
        }

        return new Vector2(0, 1);
    }

    private static CollisionLayers ParseLayerType(string identifier)
    {
        var lower = identifier.ToLowerInvariant();

        return lower switch
        {
            _ when lower.Contains("solid") || lower.Contains("collision") || lower.Contains("wall") => CollisionLayers
                .Solid,
            _ when lower.Contains("platform") => CollisionLayers.Platform,
            _ when lower.Contains("trigger") => CollisionLayers.Trigger,
            _ when lower.Contains("water") => CollisionLayers.Water,
            _ when lower.Contains("hazard") || lower.Contains("damage") => CollisionLayers.Hazard,
            _ => CollisionLayers.Solid
        };
    }

    private void ProcessCollisionPair(
        string idA, CollisionComponent compA, Vector2 posA,
        string idB, CollisionComponent compB, Vector2 posB)
    {
        (string, string) key = string.Compare(idA, idB, StringComparison.Ordinal) < 0 ? (idA, idB) : (idB, idA);
        if (_processedCollisions.Contains(key))
        {
            return;
        }

        _processedCollisions.Add(key);

        // Handle triggers
        if ((compA.Layers & CollisionLayers.Trigger) != 0)
        {
            HandleTrigger(idA, compA, idB);
        }

        if ((compB.Layers & CollisionLayers.Trigger) != 0)
        {
            HandleTrigger(idB, compB, idA);
        }

        // Send collision message for non-trigger collisions
        if (!compA.IsTrigger && !compB.IsTrigger)
        {
            Vector2 contactPoint = (posA + posB) / 2;
            Vector2 normal = Vector2.Normalize(posB - posA);
            var penetration = CalculatePenetrationDepth(compA.Shape, posA, compB.Shape, posB);

            messageBus.SendMessage(new CollisionMessage(idA, idB, contactPoint, normal, penetration));
        }
    }

    private void HandleTrigger(string triggerId, CollisionComponent triggerComp, string entityId)
    {
        if (!_currentTriggerOverlaps.TryGetValue(triggerId, out HashSet<string>? overlaps))
        {
            overlaps = new HashSet<string>();
            _currentTriggerOverlaps[triggerId] = overlaps;
        }

        if (!overlaps.Contains(entityId))
        {
            overlaps.Add(entityId);
            messageBus.SendMessage(new TriggerEnterMessage(triggerId, entityId, triggerComp.Layers));
        }
    }

    private float CalculatePenetrationDepth(ICollisionShape shapeA, Vector2 posA, ICollisionShape shapeB, Vector2 posB)
    {
        // Simplified penetration calculation
        Rectangle boundsA = TranslateShape(shapeA, posA).GetFinalBounds();
        Rectangle boundsB = TranslateShape(shapeB, posB).GetFinalBounds();

        var xOverlap = Math.Min(boundsA.Right, boundsB.Right) - Math.Max(boundsA.Left, boundsB.Left);
        var yOverlap = Math.Min(boundsA.Bottom, boundsB.Bottom) - Math.Max(boundsA.Top, boundsB.Top);

        return Math.Min(xOverlap, yOverlap);
    }

    private ICollisionShape TranslateShape(ICollisionShape shape, Vector2 position)
    {
        return shape switch
        {
            BoxShape box => new BoxShape(box.Bounds, box.Offset + position),
            CircleShape circle => new CircleShape(circle.Center + position, circle.Radius),
            CompositeShape composite => new CompositeShape(
                composite.Shapes.Select(s => TranslateShape(s, position)).ToArray()),
            _ => shape
        };
    }

    private bool CheckTileCollision(Rectangle bounds, CollisionLayers layersMask)
    {
        Rectangle tileBounds = bounds.ToTileCoordinates(_tileSize);

        // Check if ANY tile has a collision that matches the layer mask
        for (var x = tileBounds.Left; x <= tileBounds.Right; x++)
        {
            for (var y = tileBounds.Top; y <= tileBounds.Bottom; y++)
            {
                var point = new Point(x, y);
                if (_tileCollision.TryGetValue(point, out CollisionLayers layer)
                    && (layer & layersMask) != 0)
                {
                    return true; // Found a collision tile that blocks movement
                }
            }
        }

        return false; // No blocking collision found
    }

    private CollisionTestResult RaycastTiles(Vector2 origin, Vector2 direction, float maxDistance,
        CollisionLayers layersMask)
    {
        // DDA line algorithm for tile traversal
        var currentTile = new Point((int)(origin.X / _tileSize), (int)(origin.Y / _tileSize));
        var stepX = Math.Sign(direction.X);
        var stepY = Math.Sign(direction.Y);

        float tMaxX;
        if (direction.X.EqualsAppr(0))
        {
            tMaxX = float.MaxValue;
        }
        else
        {
            if (stepX > 0)
            {
                tMaxX = (((currentTile.X + 1) * _tileSize) - origin.X) /
                        direction.X;
            }
            else
            {
                tMaxX = ((currentTile.X * _tileSize) - origin.X) /
                        direction.X;
            }
        }

        float tMaxY;
        if (direction.Y.EqualsAppr(0))
        {
            tMaxY = float.MaxValue;
        }
        else
        {
            if (stepY > 0)
            {
                tMaxY = (((currentTile.Y + 1) * _tileSize) - origin.Y) /
                        direction.Y;
            }
            else
            {
                tMaxY = ((currentTile.Y * _tileSize) - origin.Y) /
                        direction.Y;
            }
        }

        var tDeltaX = Math.Abs(_tileSize / direction.X);
        var tDeltaY = Math.Abs(_tileSize / direction.Y);

        var distance = 0f;

        while (distance < maxDistance)
        {
            if (_tileCollision.TryGetValue(currentTile, out CollisionLayers layer) && (layer & layersMask) != 0)
            {
                Vector2 hitPoint = origin + (direction * distance);
                Vector2 normal = GetTileNormal(origin, currentTile);
                return CollisionTestResult.HitTile(hitPoint, normal, distance, currentTile, layer);
            }

            if (tMaxX < tMaxY)
            {
                distance = tMaxX;
                tMaxX += tDeltaX;
                currentTile.X += stepX;
            }
            else
            {
                distance = tMaxY;
                tMaxY += tDeltaY;
                currentTile.Y += stepY;
            }
        }

        return CollisionTestResult.NoHit();
    }

    private Vector2 GetTileNormal(Vector2 rayOrigin, Point tile)
    {
        var tileCenter = new Vector2((tile.X * _tileSize) + (_tileSize / 2f), (tile.Y * _tileSize) + (_tileSize / 2f));
        Vector2 toTile = tileCenter - rayOrigin;

        return Math.Abs(toTile.X) > Math.Abs(toTile.Y)
            ? new Vector2(Math.Sign(toTile.X), 0)
            : new Vector2(0, Math.Sign(toTile.Y));
    }

    private static CollisionTestResult? RayShapeIntersection(
        Vector2 origin, Vector2 direction, ICollisionShape shape, float maxDistance)
    {
        // Simplified ray-shape intersection
        Rectangle bounds = shape.GetFinalBounds();

        // Ray-AABB intersection
        var tMin = (bounds.Left - origin.X) / direction.X;
        var tMax = (bounds.Right - origin.X) / direction.X;

        if (tMin > tMax)
        {
            (tMin, tMax) = (tMax, tMin);
        }

        var tyMin = (bounds.Top - origin.Y) / direction.Y;
        var tyMax = (bounds.Bottom - origin.Y) / direction.Y;

        if (tyMin > tyMax)
        {
            (tyMin, tyMax) = (tyMax, tyMin);
        }

        if (tMin > tyMax || tyMin > tMax)
        {
            return null;
        }

        tMin = Math.Max(tMin, tyMin);
        tMax = Math.Min(tMax, tyMax);

        if (tMin < 0)
        {
            tMin = tMax;
        }

        if (tMin < 0 || tMin > maxDistance)
        {
            return null;
        }

        Vector2 point = origin + (direction * tMin);
        Vector2 normal = GetBoxNormal(bounds, point);

        return new CollisionTestResult(point, normal, tMin);
    }
}

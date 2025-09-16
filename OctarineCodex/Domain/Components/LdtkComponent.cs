// OctarineCodex/Domain/Components/LdtkComponent.cs

using System;

namespace OctarineCodex.Domain.Components;

/// <summary>
///     Stores LDtk-specific entity data for ECS entities.
///     Bridge between LDtk level data and runtime ECS entities.
/// </summary>
public struct LdtkComponent
{
    public string EntityIid; // LDtk entity instance ID
    public string EntityIdentifier; // LDtk entity definition identifier
    public Guid LevelIid; // Source level ID
    public int LdtkX, LdtkY; // Original LDtk grid coordinates
    public int LdtkPixelX, LdtkPixelY; // Original LDtk pixel coordinates

    public LdtkComponent(string entityIid, string entityIdentifier, Guid levelIid,
        int ldtkX, int ldtkY, int pixelX, int pixelY)
    {
        EntityIid = entityIid;
        EntityIdentifier = entityIdentifier;
        LevelIid = levelIid;
        LdtkX = ldtkX;
        LdtkY = ldtkY;
        LdtkPixelX = pixelX;
        LdtkPixelY = pixelY;
    }
}

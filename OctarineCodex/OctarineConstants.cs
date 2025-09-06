using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex;

public static class OctarineConstants
{
    public const string WorldName = "Room4.ldtk";
    public const float PlayerSpeed = 100f; // pixels per second
    public const int PlayerSize = 8;
    public const float WorldRenderScale = 4.0f; // Scale factor for world/level rendering

    // Fixed resolution constants
    public const int FixedWidth = 640;
    public const int FixedHeight = 480;

    public static Texture2D Pixel { get; set; } = null!;
}
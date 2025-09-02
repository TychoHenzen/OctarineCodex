namespace LDtkTypes;

// This file was automatically generated, any modifications will be lost!
#pragma warning disable

using LDtk;
using Microsoft.Xna.Framework;

public partial class Button : ILDtkEntity
{
    public static Button Default() => new()
    {
        Identifier = "Button",
        Uid = 105,
        Size = new Vector2(10f, 10f),
        Pivot = new Vector2(0.5f, 0.5f),
        Tile = new TilesetRectangle()
        {
            X = 272,
            Y = 32,
            W = 16,
            H = 16
        },
        SmartColor = new Color(255, 0, 0, 255),

    };

    public string Identifier { get; set; }
    public System.Guid Iid { get; set; }
    public int Uid { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Pivot { get; set; }
    public Rectangle Tile { get; set; }

    public Color SmartColor { get; set; }

    public EntityReference[] targets { get; set; }
}
#pragma warning restore

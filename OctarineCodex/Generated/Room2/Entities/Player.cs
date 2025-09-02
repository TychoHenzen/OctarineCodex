namespace LDtkTypes;

// This file was automatically generated, any modifications will be lost!
#pragma warning disable

using LDtk;
using Microsoft.Xna.Framework;

public partial class Player : ILDtkEntity
{
    public static Player Default() => new()
    {
        Identifier = "Player",
        Uid = 59,
        Size = new Vector2(16f, 16f),
        Pivot = new Vector2(0.5f, 0.5f),
        Tile = new TilesetRectangle()
        {
            X = 0,
            Y = 240,
            W = 16,
            H = 16
        },
        SmartColor = new Color(3, 203, 124, 255),

        life = 100,
        ammo = 10,
    };

    public string Identifier { get; set; }
    public System.Guid Iid { get; set; }
    public int Uid { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Pivot { get; set; }
    public Rectangle Tile { get; set; }

    public Color SmartColor { get; set; }

    public int life { get; set; }
    public int ammo { get; set; }
}
#pragma warning restore

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

public class EntityData
{
    public string Type { get; set; } = "";
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}
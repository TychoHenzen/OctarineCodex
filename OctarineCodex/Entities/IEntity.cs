using Microsoft.Xna.Framework;

namespace OctarineCodex.Entities;

public interface IEntity
{
    string Identifier { get; }
    string Iid { get; }
    Vector2 Position { get; set; }
    Vector2 Size { get; }
}

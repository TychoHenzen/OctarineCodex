using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;

public interface IEntity
{
    string Identifier { get; }
    string Iid { get; }
    Vector2 Position { get; set; }
    Vector2 Size { get; }
}

public interface IEntityFactory
{
    bool CanCreate(string entityType);
    IEntity CreateEntity(EntityInstance ldtkEntity, LDtkLevel level);
}

public interface IEntityRegistry
{
    void RegisterFactory(IEntityFactory factory);
    void InitializeFromLevels(IEnumerable<LDtkLevel> levels);
    IEnumerable<T> GetEntities<T>() where T : IEntity;
}
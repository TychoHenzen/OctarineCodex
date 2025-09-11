using LDtk;

namespace OctarineCodex.Domain.Entities;

public interface IEntityFactory
{
    bool CanCreate(string entityType);
    IEntity CreateEntity(EntityInstance ldtkEntity, LDtkLevel level);
}

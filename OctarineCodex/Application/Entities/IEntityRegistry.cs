using System.Collections.Generic;
using LDtk;

namespace OctarineCodex.Entities;

public interface IEntityRegistry
{
    void RegisterFactory(IEntityFactory factory);
    void InitializeFromLevels(IEnumerable<LDtkLevel> levels);

    IEnumerable<T> GetEntities<T>()
        where T : IEntity;
}

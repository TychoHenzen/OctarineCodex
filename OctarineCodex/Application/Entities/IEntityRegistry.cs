using System.Collections.Generic;
using LDtk;
using OctarineCodex.Domain.Entities;

namespace OctarineCodex.Application.Entities;

public interface IEntityRegistry
{
    void RegisterFactory(IEntityFactory factory);
    void InitializeFromLevels(IEnumerable<LDtkLevel> levels);

    IEnumerable<T> GetEntities<T>()
        where T : IEntity;
}

using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

[Service<WorldLayerService>]
public interface IWorldLayerService
{
    int CurrentWorldDepth { get; }
    IReadOnlyList<LDtkLevel> GetCurrentLayerLevels();
    IReadOnlyList<LDtkLevel> GetAllLevels();
    bool SwitchToLayer(int worldDepth);
    LDtkLevel? GetLevelAt(Vector2 worldPosition, int? worldDepth = null);
    void InitializeLevels(IReadOnlyList<LDtkLevel> levels);
}
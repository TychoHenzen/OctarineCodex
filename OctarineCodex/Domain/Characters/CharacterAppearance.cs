using System.Collections.Generic;

namespace OctarineCodex.Domain.Characters;

public record CharacterAppearance(
    Dictionary<string, int> LayerSelections)
{
    public static CharacterAppearance Default => new(new Dictionary<string, int>
    {
        ["Bodies"] = 0,
        ["Eyes"] = 0,
        ["Outfits"] = 0,
        ["Hairstyles"] = 0,
        ["Accessories"] = 0
    });
}

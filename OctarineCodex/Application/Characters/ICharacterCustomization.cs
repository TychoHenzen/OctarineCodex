using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Domain.Characters;

namespace OctarineCodex.Application.Characters;

/// <summary>
///     Interface for individual character customization instances
/// </summary>
public interface ICharacterCustomization
{
    void InitializeLayers(Dictionary<string, CharacterLayerDefinition> layerDefinitions);
    void ChangeLayerSelection(string layerName, int variantIndex);
    CharacterAppearance GetCurrentAppearance();
    void Update(GameTime gameTime);
    void PlayAnimation(string animationName);
    IEnumerable<LayerRenderData> GetLayerRenderData();
}

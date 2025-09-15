using OctarineCodex.Application.Services;

namespace OctarineCodex.Application.Characters;

/// <summary>
///     Factory service for creating character customization instances
/// </summary>
[Service<CharacterCustomizationService>]
public interface ICharacterCustomizationService
{
    /// <summary>
    ///     Creates a new character customization instance
    /// </summary>
    ICharacterCustomization CreateCustomization();
}

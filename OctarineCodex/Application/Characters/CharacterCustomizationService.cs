using System;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Infrastructure.Logging;
using OctarineCodex.Infrastructure.MonoGame;

namespace OctarineCodex.Application.Characters;

/// <summary>
///     Factory service for creating character customization instances
/// </summary>
public class CharacterCustomizationService : ICharacterCustomizationService
{
    private readonly IAsepriteAnimationService _asepriteService;
    private readonly IContentManagerService _contentManager;
    private readonly ILoggingService _logger;

    public CharacterCustomizationService(
        IContentManagerService contentManager,
        IAsepriteAnimationService asepriteService,
        ILoggingService logger)
    {
        _contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
        _asepriteService = asepriteService ?? throw new ArgumentNullException(nameof(asepriteService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ICharacterCustomization CreateCustomization()
    {
        // Create new LayeredAnimationController for each character
        var animationController = new LayeredAnimationController();

        return new CharacterCustomization(
            animationController,
            _contentManager,
            _asepriteService,
            _logger);
    }
}

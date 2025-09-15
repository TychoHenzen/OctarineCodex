// using System;
// using OctarineCodex.Domain.Animation;
// using OctarineCodex.Infrastructure.Logging;
// using OctarineCodex.Infrastructure.MonoGame;
//
// namespace OctarineCodex.Application.Characters;
//
// /// <summary>
// ///     Factory service for creating character customization instances.
// /// </summary>
// public class CharacterCustomizationService(
//     IContentManagerService contentManager,
//     IAsepriteAnimationService asepriteService,
//     ILoggingService logger)
//     : ICharacterCustomizationService
// {
//     public ICharacterCustomization CreateCustomization()
//     {
//         // Create new LayeredAnimationController for each character
//         var animationController = new LayeredAnimationController();
//
//         return new CharacterCustomization(
//             animationController,
//             contentManager,
//             asepriteService,
//             logger);
//     }
// }



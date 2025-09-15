// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework.Content;
// using OctarineCodex.Application.Entities;
// using OctarineCodex.Domain.Animation;
// using OctarineCodex.Domain.Entities;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace OctarineCodex.Presentation.Rendering;
//
// [EntityBehavior(EntityType = "Player", Priority = 100)]
// public class PlayerRenderBehavior : EntityBehavior
// {
//     private readonly LayeredAnimationController _animationController;
//     private readonly ContentManager _contentManager;
//     private readonly Dictionary<string, Texture2D> _layerTextures = new();
//     private AsepriteAnimationData? _animationData;
//
//     public PlayerRenderBehavior(
//         LayeredAnimationController animationController)
//     {
//         _animationController = animationController;
//         _contentManager = OctarineConstants.Game.Content;
//     }
//
//     public override bool ShouldApplyTo(EntityWrapper entity)
//     {
//         return HasEntityType(entity, "Player");
//     }
//
//     public override void Initialize(EntityWrapper entity)
//     {
//         base.Initialize(entity);
//
//         // Load animation data
//         LoadAnimationData();
//
//         // Load character textures (this would be expanded based on actual character selection)
//         LoadDefaultCharacterTextures();
//     }
//
//     public override void Draw(SpriteBatch? spriteBatch)
//     {
//         if (spriteBatch == null || _animationData == null)
//             return;
//
//         IEnumerable<LayerRenderData> layerRenderData = _animationController.GetLayerRenderData();
//
//         foreach (LayerRenderData layer in layerRenderData)
//         {
//             if (_layerTextures.TryGetValue(layer.LayerName, out Texture2D? texture))
//             {
//                 DrawAnimatedSprite(spriteBatch, texture, layer);
//             }
//         }
//     }
//
//     private void DrawAnimatedSprite(SpriteBatch spriteBatch, Texture2D texture, LayerRenderData layer)
//     {
//         // Get current frame data from animation
//         var frameIndex = layer.TileId;
//         AsepriteFrame? frameData = GetFrameDataByIndex(frameIndex);
//
//         if (frameData != null)
//         {
//             var sourceRect = new Rectangle(
//                 frameData.Frame.X,
//                 frameData.Frame.Y,
//                 frameData.Frame.W,
//                 frameData.Frame.H);
//
//             var destRect = new Rectangle(
//                 (int)Entity.Position.X,
//                 (int)Entity.Position.Y,
//                 frameData.Frame.W,
//                 frameData.Frame.H);
//
//             spriteBatch.Draw(
//                 texture,
//                 destRect,
//                 sourceRect,
//                 Color.White * layer.Alpha);
//         }
//     }
//
//     private void LoadAnimationData()
//     {
//         // Load the animation.json file
//         var jsonContent = System.IO.File.ReadAllText(_contentManager.RootDirectory + "/animation.json");
//         _animationData = AsepriteAnimationData.FromJson(jsonContent);
//     }
//
//     private void LoadDefaultCharacterTextures()
//     {
//         // Load default textures for each layer
//         // You'll need to adjust these paths based on your actual file names
//         _layerTextures["Bodies"] = _contentManager.Load<Texture2D>("Character/Bodies/Body_01");
//
//         // Only load other layers if they exist
//         try { _layerTextures["Eyes"] = _contentManager.Load<Texture2D>("Character/Eyes/Eyes_01"); }
//         catch { }
//
//         try { _layerTextures["Hairstyles"] = _contentManager.Load<Texture2D>("Character/Hairstyles/Hair_01"); }
//         catch { }
//
//         try { _layerTextures["Outfits"] = _contentManager.Load<Texture2D>("Character/Outfits/Outfit_01"); }
//         catch { }
//
//         try { _layerTextures["Accessories"] = _contentManager.Load<Texture2D>("Character/Accessories/Accessory_01"); }
//         catch { }
//     }
//
//     private AsepriteFrame? GetFrameDataByIndex(int frameIndex)
//     {
//         if (_animationData == null)
//         {
//             return null;
//         }
//
//         AsepriteFrame[] frames = _animationData.Frames.Values.ToArray();
//         return frameIndex < frames.Length ? frames[frameIndex] : null;
//     }
// }



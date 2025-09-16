using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.GameState;
using OctarineCodex.Application.Systems;
using OctarineCodex.Infrastructure.Ecs;
using OctarineCodex.Infrastructure.Logging;
using static OctarineCodex.OctarineConstants;

namespace OctarineCodex;

public class OctarineGameHost(
    ILoggingService logger,
    IGameInitializationManager initializationManager,
    IGameUpdateManager updateManager,
    IGameRenderManager renderManager,
    SystemManager systemManager,
    RenderSystem renderSystem)
    : Game
{
    private GraphicsDeviceManager _graphics = null!;
    private RenderTarget2D _renderTarget = null!;
    private SpriteBatch _spriteBatch = null!;

    public void init()
    {
        _graphics = new GraphicsDeviceManager(this);
    }

    protected override void Initialize()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Improve smoothing of input by decoupling updates from VSync and fixed timestep
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override async void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, FixedWidth, FixedHeight);
        Pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        Pixel.SetData([Color.White]);

        try
        {
            var success = await initializationManager.InitializeWorldAsync(
                GraphicsDevice, Content, WorldName);

            if (!success)
            {
                logger.Error("Failed to initialize world");
            }
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        updateManager.Update(gameTime);
        systemManager.Update(gameTime);

        if (updateManager.ShouldExit)
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        if (initializationManager.IsWorldLoaded)
        {
            Matrix worldMatrix = renderManager.GetWorldTransformMatrix() *
                                 Matrix.CreateScale(WorldRenderScale);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: worldMatrix);

            // Delegate all rendering logic to the render manager
            renderManager.Draw(_spriteBatch, Vector2.Zero);

            // ECS rendering - need to set SpriteBatch first
            if (systemManager != null)
            {
                // Note: This is a simplified approach for Phase 1
                // In Phase 2, we'll have a better way to access systems
                renderSystem.SetSpriteBatch(_spriteBatch);
                systemManager.Draw(gameTime);
            }

            _spriteBatch.End();
        }

        // Render to screen with scaling
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        var destinationRect = CalculateDestinationRectangle();
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
        _spriteBatch.End();
    }

    protected override void UnloadContent()
    {
        _renderTarget?.Dispose();
        Pixel?.Dispose();
        base.UnloadContent();
    }

    private Rectangle CalculateDestinationRectangle()
    {
        var windowWidth = GraphicsDevice.Viewport.Width;
        var windowHeight = GraphicsDevice.Viewport.Height;

        var scaleX = (float)windowWidth / FixedWidth;
        var scaleY = (float)windowHeight / FixedHeight;
        var scale = Math.Min(scaleX, scaleY);

        var scaledWidth = (int)(FixedWidth * scale);
        var scaledHeight = (int)(FixedHeight * scale);

        var x = (windowWidth - scaledWidth) / 2;
        var y = (windowHeight - scaledHeight) / 2;

        return new Rectangle(x, y, scaledWidth, scaledHeight);
    }
}

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OctarineCodex.Input;
using OctarineCodex.Maps;

namespace OctarineCodex;

public class OctarineGameHost : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // Input
    private readonly IInputService _inputService;

    // LDTK services
    private readonly ILdtkMapService _mapService;
    private readonly ILdtkMapRenderer _mapRenderer;

    // Primitive 1x1 texture for drawing rectangles
    private Texture2D _pixel = null!;

    // Player state
    private Vector2 _playerPos;
    private const int PlayerSize = 32;
    private const float PlayerSpeed = 220f; // pixels per second

    // Grid state
    private const int TileSize = 32;



    public OctarineGameHost(IInputService inputService, ILdtkMapService mapService, ILdtkMapRenderer mapRenderer)
    {
        _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));
        _mapRenderer = mapRenderer ?? throw new ArgumentNullException(nameof(mapRenderer));
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Improve smoothing of input by decoupling updates from VSync and fixed timestep
        IsFixedTimeStep = false;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();
        // Start player roughly centered
        var vp = GraphicsDevice.Viewport;
        _playerPos = new Vector2((vp.Width - PlayerSize) / 2f, (vp.Height - PlayerSize) / 2f);
    }

    protected override async void LoadContent()
    {
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white texture to draw colored rectangles
        _pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        _pixel.SetData(new[] { Color.White });

        // Initialize LDTK renderer
        _mapRenderer.Initialize(GraphicsDevice);

        // Load LDTK map
        var ldtkPath = Path.Combine(Content.RootDirectory, "test_level2.ldtk");
        Console.WriteLine($"[DEBUG_LOG] Content.RootDirectory: '{Content.RootDirectory}'");
        Console.WriteLine($"[DEBUG_LOG] Current working directory: '{Directory.GetCurrentDirectory()}'");
        Console.WriteLine($"[DEBUG_LOG] Constructed LDTK path: '{ldtkPath}'");
        Console.WriteLine($"[DEBUG_LOG] Full resolved LDTK path: '{Path.GetFullPath(ldtkPath)}'");
        Console.WriteLine($"[DEBUG_LOG] LDTK file exists: {File.Exists(ldtkPath)}");
        var project = await _mapService.LoadProjectAsync(ldtkPath);
        if (project != null)
        {
            await _mapRenderer.LoadTilesetsAsync(project, Content.RootDirectory);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        _inputService.Update(gameTime);
        if (_inputService.IsExitPressed())
        {
            Exit();
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var dir = _inputService.GetMovementDirection();

        var delta = Movement.ComputeDelta(dir, PlayerSpeed, dt);
        _playerPos += delta;

        // Clamp within viewport
        var vp = GraphicsDevice.Viewport;
        _playerPos.X = MathHelper.Clamp(_playerPos.X, 0, vp.Width - PlayerSize);
        _playerPos.Y = MathHelper.Clamp(_playerPos.Y, 0, vp.Height - PlayerSize);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw LDTK map if loaded, otherwise draw fallback checker background
        if (_mapService.IsProjectLoaded)
        {
            Console.WriteLine("[DEBUG_LOG] LDTK project is loaded, attempting to render");
            var level = _mapService.GetLevel("Entrance");
            if (level != null)
            {
                Console.WriteLine($"[DEBUG_LOG] Found level 'Entrance' with {level.LayerInstances.Length} layers, rendering...");
                _mapRenderer.RenderLevel(level, _spriteBatch, Matrix.Identity);
            }
            else
            {
                Console.WriteLine("[DEBUG_LOG] Level 'Entrance' not found!");
            }
        }
        else
        {
            // Fallback: Draw pink/black checker background
            var vp = GraphicsDevice.Viewport;
            for (int y = 0; y < vp.Height; y += TileSize)
            {
                for (int x = 0; x < vp.Width; x += TileSize)
                {
                    bool isPink = (((x / TileSize) + (y / TileSize)) % 2) == 0;
                    var color = isPink ? Color.HotPink : Color.Black;
                    _spriteBatch.Draw(_pixel, new Rectangle(x, y, TileSize, TileSize), color);
                }
            }
        }

        // Draw the player as a red square
        _spriteBatch.Draw(_pixel, new Rectangle((int)_playerPos.X, (int)_playerPos.Y, PlayerSize, PlayerSize), Color.Red);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
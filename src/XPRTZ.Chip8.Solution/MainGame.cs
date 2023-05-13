namespace XPRTZ.Chip8.Solution;

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XPRTZ.Chip8.Solution.Fonts;
using XPRTZ.Chip8.Solution.Interfaces;
using XPRTZ.Chip8.Solution.Keyboards;
using XPRTZ.Chip8.Solution.ROMData;
using XPRTZ.Chip8.Solution.Screens;
using XPRTZ.Chip8.Solution.Sounds;

public class MainGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _canvas;

    private Chip8? _chip8;

    private double _deltaTime;
    private double _accumulator;

    private int _screenWidth;
    private int _screenHeight;

    private const int _scaleWidth = 10;
    private const int _scaleHeight = 10;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        Services.AddService<IKeyboard>(new HardwareKeyboard());
        Services.AddService<IScreen>(new Chip8Screen());
        Services.AddService<ISound>(new Buzzer());
        Services.AddService<IFont>(new Chip8Font());
        Services.AddService<IROMDataProvider>(new ROMDataProvider());
        Services.AddService(
            new Chip8(
                Services.GetService<IKeyboard>(),
                Services.GetService<IScreen>(),
                Services.GetService<ISound>(),
                Services.GetService<IFont>(),
                Services.GetService<IROMDataProvider>()));
    }

    protected override void Initialize()
    {
        IsMouseVisible = false;

        _screenWidth = Services.GetService<IScreen>().Width;
        _screenHeight = Services.GetService<IScreen>().Height;

        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = _screenWidth * _scaleWidth;
        _graphics.PreferredBackBufferHeight = _screenHeight * _scaleHeight;
        _graphics.SynchronizeWithVerticalRetrace = true;
        _graphics.ApplyChanges();

        IsFixedTimeStep = false;

        _canvas = new Texture2D(
                GraphicsDevice,
                _screenWidth,
                _screenHeight,
                false,
                SurfaceFormat.Color);

        _chip8 = Services.GetService<Chip8>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        if (_chip8 is null)
        {
            return;
        }

        _chip8.LoadRom("./ROMS/Tests/6-keypad.ch8");

        _deltaTime = Stopwatch.Frequency / (double)_chip8.ClockSpeed;

        Window.Title = _chip8.RomMetadata.Title;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // https://gafferongames.com/post/fix_your_timestep/
        _accumulator = gameTime.ElapsedGameTime.Ticks;

        while (_accumulator >= _deltaTime)
        {
            _chip8?.Cycle();
            _accumulator -= _deltaTime;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_canvas is not null)
        {
            _chip8?.Screen.Blit(_canvas, _chip8.RomMetadata.Options.BackgroundColor, _chip8.RomMetadata.Options.FillColor);

            _spriteBatch?.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
            _spriteBatch?.Draw(_canvas, Vector2.Zero, null, Color.White, 0, Vector2.Zero, new Vector2(_scaleWidth, _scaleHeight), SpriteEffects.None, 0);
            _spriteBatch?.End();
        }

        base.Draw(gameTime);
    }
}

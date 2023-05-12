namespace XPRTZ.Chip8;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XPRTZ.Chip8.Fonts;
using XPRTZ.Chip8.Interfaces;
using XPRTZ.Chip8.Keyboards;
using XPRTZ.Chip8.ROMData;
using XPRTZ.Chip8.Screens;
using XPRTZ.Chip8.Sounds;

public class MainGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _canvas;

    private Chip8? _chip8;

    private int _screenWidth;
    private int _screenHeight;

    private int _scaleWidth = 10;
    private int _scaleHeight = 10;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        Services.AddService<IKeyboard>(new EmulatedKeyboard());
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

        _chip8.LoadRom("./ROMS/Tests/1-chip8-logo.ch8");

        Window.Title = _chip8.RomMetadata.Title;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // TODO: Calculate the correct clockcycles
        // https://gafferongames.com/post/fix_your_timestep/
        _chip8?.Cycle();

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

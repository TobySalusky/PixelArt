using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PixelArt
{
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private static Main instance;
        
        public static KeyboardState lastKeyState;
        public static MouseState lastMouseState;

        public static List<UIElement> uiElements = new List<UIElement>();

        // OBJECTS
        public static Camera camera;
        public static Canvas canvas;

        // settings
        public static Tool tool= Tool.Brush;
        public static Color brushColor = Color.Black;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            instance = this;
        }

        protected override void Initialize()
        {

            // Graphics stuff and setup
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            Window.IsBorderless = false;
            Window.AllowUserResizing = true;
            graphics.ApplyChanges();
            
            // loading
            Textures.loadTextures();

            // var Init
            camera = new Camera(Vector2.Zero, 5);
            canvas = new Canvas(Textures.get("bush"));
            
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        private float delta(GameTime gameTime) {
            return (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void globalControls(float deltaTime, KeyInfo keys, MouseInfo mouse) {
            if (keys.down(Keys.Escape)) {
                Exit();
            }

            if (mouse.middleDown) {
                Vector2 worldDiff = camera.toWorld(mouse.pos) - camera.toWorld(new Vector2(lastMouseState.X, lastMouseState.Y));
                camera.pos -= worldDiff;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = delta(gameTime);

            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            KeyInfo keys = new KeyInfo(keyState, lastKeyState);
            MouseInfo mouse = new MouseInfo(mouseState, lastMouseState);
            
            globalControls(deltaTime, keys, mouse);

            canvas.input(deltaTime, keys, mouse);

            // end stuff
            lastKeyState = keyState;
            lastMouseState = mouseState;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Colors.background);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp);

            
            canvas.render(camera, spriteBatch);
            
            Texture2D rect = Textures.get("rect");
            spriteBatch.Draw(rect, new Rectangle(0, 0, 200, 1080), Colors.panel);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static GraphicsDevice getGraphicsDevice() {
            return instance.GraphicsDevice;
        }
    }
}

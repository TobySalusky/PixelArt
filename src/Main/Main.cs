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
        public static SpriteBatch spriteBatch;

        public static Main instance;
        
        // INPUT
        public static KeyboardState lastKeyState;
        public static MouseState lastMouseState;

        // UI
        public static List<UIElement> uiElements = new List<UIElement>();

        // SCREEN
        public static Vector2 screenDimen;
        public static Vector2 screenCenter;
        
        // OBJECTS
        public static Camera camera;
        public static Canvas canvas;

        // settings
        public static Tool tool = Tool.Brush;
        public static Color brushColor = Color.Black;

        // SPECIFICS
        public float startDragZoomScale, startDragZoomMag;
        
        // DEBUG
        public FrameCounter fpsCounter = new FrameCounter();
        public int secondsPassed;
        
        
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // REMOVES LIMIT FROM FPS
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            instance = this;

            screenDimen = new Vector2(1920, 1080);
            screenCenter = screenDimen / 2;
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
            canvas = new Canvas(64, 64);
            
            
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

            if (keys.down(Keys.LeftShift) && mouse.middlePressed || mouse.middleDown && keys.pressed(Keys.LeftShift)) {
                startDragZoomMag = Util.mag(screenCenter - mouse.pos);
                startDragZoomScale = camera.scale;
            }

            if (mouse.middleDown) {
                if (keys.down(Keys.LeftShift)) {
                    camera.scale = startDragZoomScale * (Util.mag(screenCenter - mouse.pos) / startDragZoomMag);
                }
                else {
                    Vector2 worldDiff = camera.toWorld(mouse.pos) - camera.toWorld(new Vector2(lastMouseState.X, lastMouseState.Y));
                    camera.pos -= worldDiff;
                }
            }
            
            toolControls(deltaTime, keys, mouse);
        }

        public void toolControls(float deltaTime, KeyInfo keys, MouseInfo mouse) {
            if (keys.pressed(Keys.A))
                tool = Tool.Brush;
            if (keys.pressed(Keys.S))
                tool = Tool.Eraser;
            if (keys.pressed(Keys.D))
                tool = Tool.FillBucket;
            if (keys.pressed(Keys.R))
                tool = Tool.Rect;
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = delta(gameTime);

            fpsCounter.update(deltaTime);
            if ((int) gameTime.TotalGameTime.TotalSeconds > secondsPassed) {
                secondsPassed = (int) gameTime.TotalGameTime.TotalSeconds;
                Window.Title = "FPS: " + (int) fpsCounter.AverageFramesPerSecond;
            }

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

        public static Vector2 lastMousePos() {
            return new Vector2(lastMouseState.X, lastMouseState.Y);
        }
    }
}

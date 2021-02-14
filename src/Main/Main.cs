using System;
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
        public static bool uiHit;
        
        public static List<UIElement> uiElements = new List<UIElement>();
        public static ColorWheel colorWheel;
        public static HueSlider hueSlider;

        public static bool exportOpen;

        // SCREEN
        public static Vector2 screenDimen;
        public static Vector2 screenCenter;
        public static int screenHeight, screenWidth;
        
        // OBJECTS
        public static Camera camera;
        public static Canvas canvas;

        // settings
        public static Tool tool = Tool.Brush;
        public static Tool lastTool;
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
            screenWidth = (int) screenDimen.X;
            screenHeight = (int) screenDimen.Y;
        }

        protected override void Initialize()
        {

            // Graphics stuff and setup
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            Window.IsBorderless = false;
            Window.AllowUserResizing = true;
            
            //Window.Position = new Point(0, 1080);

            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // loading
            Textures.loadTextures();
            Fonts.loadFonts(Content);
            
            ToolSettings.loadTools();


            // var Init
            camera = new Camera(Vector2.Zero, 5);
            canvas = new Canvas(1920, 1080);

            colorWheel = new ColorWheel(new Vector2(120, 100), new Vector2(130, 150));
            hueSlider = new HueSlider(new Vector2(205, 100), new Vector2(20, 150));

            uiElements.Add(new PanelSide(new Rectangle(0, 0, 225, screenHeight)));
            
            uiElements.Add(new PanelSide(new Rectangle(screenWidth - 150, 0, 150, screenHeight)));
            uiElements.Add(colorWheel);
            uiElements.Add(hueSlider);
            
            uiElements.Add(new UIText(new Vector2(235, 10), () => {
                Point point = canvas.toPixel(canvas.toCanvas(lastMousePos()));
                return "(" + point.X + ", " + point.Y + ")";
            }));

            foreach (Tool toolType in Util.GetValues<Tool>()) {
                uiElements.Add(new ToolButton(toolType, new Vector2(24, 230 + (int) toolType * 40)));
            }
            
            setTool(Tool.Brush);
        }

        private float delta(GameTime gameTime) {
            return (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void globalControls(float deltaTime, KeyInfo keys, MouseInfo mouse) {
            if (keys.down(Keys.Escape)) {
                Exit();
            }
            
            // CAMERA CONTROLS
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

            if (mouse.scroll != 0) {
                camera.scale *= 1 + mouse.scroll / 5F;
            }
            
            // Saving
            if (keys.pressed(Keys.S) && keys.down(Keys.LeftControl)) {
                if (!exportOpen)
                    exportPopUp();
            }


            // TOOLS
            toolControls(deltaTime, keys, mouse);
        }

        public static void setTool(Tool newTool) {
            tool = newTool;
            var list = ToolUtil.genToolSettings(tool);
            
            uiElements.AddRange(list);
        }

        public void toolControls(float deltaTime, KeyInfo keys, MouseInfo mouse) {
            // TOOLS
            if (keys.pressed(Keys.A))
                tool = Tool.Brush;
            if (keys.pressed(Keys.S) && !keys.down(Keys.LeftControl))
                tool = Tool.Eraser;
            if (keys.pressed(Keys.W))
                tool = Tool.ColorPick;
            if (keys.pressed(Keys.D))
                tool = Tool.FillBucket;
            if (keys.pressed(Keys.R))
                tool = Tool.Rect;
            if (keys.pressed(Keys.C))
                tool = Tool.Ellipse;

            if (tool == Tool.Brush || tool == Tool.Eraser) {
                try {
                    if (keys.pressed(Keys.D1))
                        ToolSettings.brush = ToolSettings.brushes[0];
                    if (keys.pressed(Keys.D2))
                        ToolSettings.brush = ToolSettings.brushes[1];
                    if (keys.pressed(Keys.D3))
                        ToolSettings.brush = ToolSettings.brushes[2];
                    if (keys.pressed(Keys.D4))
                        ToolSettings.brush = ToolSettings.brushes[3];
                    if (keys.pressed(Keys.D5))
                        ToolSettings.brush = ToolSettings.brushes[4];
                    if (keys.pressed(Keys.D6))
                        ToolSettings.brush = ToolSettings.brushes[5];
                    if (keys.pressed(Keys.D7))
                        ToolSettings.brush = ToolSettings.brushes[6];
                    if (keys.pressed(Keys.D8))
                        ToolSettings.brush = ToolSettings.brushes[7];
                    if (keys.pressed(Keys.D9))
                        ToolSettings.brush = ToolSettings.brushes[8];
                    if (keys.pressed(Keys.D0))
                        ToolSettings.brush = ToolSettings.brushes[9];
                }
                catch (IndexOutOfRangeException e) {
                    Logger.log("warning: used num-keys to try to change to out-of-range brush");
                }
            }

            // SETTINGS
            if (keys.pressed(Keys.G))
                canvas.grid = !canvas.grid;
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

            for (int i = uiElements.Count - 1; i >= 0; i--) {
                UIElement element = uiElements[i];

                if (element.delete) {
                    uiElements.RemoveAt(i);
                    continue;
                }
                
                element.update(mouse, keys, deltaTime);
            }

            if (!uiHit) {
                canvas.input(deltaTime, keys, mouse);
            }

            if (lastTool != tool) {
                setTool(tool);
                lastTool = tool;
            }

            // end stuff
            lastKeyState = keyState;
            lastMouseState = mouseState;
            
            if (mouse.leftUnpressed) uiHit = false;
            
            base.Update(gameTime);
        }

        public static void exportPopUp() {
            exportOpen = true;
            
            List<UIElement> elements = new List<UIElement> {
                new UIBack(screenCenter, screenDimen) {texture = Textures.get("Darken")},
                new UIBack(screenCenter, screenDimen * 0.7F) {color = Colors.exportBack, border = Color.LightGray},
                new UIButton(() => exportOpen = false, screenCenter + new Vector2(580, -320), Vector2.One * 60, "Exit Export") {
                    texture = Textures.get("PanelSide"), topTexture = Textures.get("ExitButton"), topColor = Colors.exportMid
                },
                new UIButton(exportImage, screenCenter + Vector2.UnitY * 250, new Vector2(800, 150), "Export as PNG") {
                    texture = Textures.get("PanelSide")
                }
            };


            foreach (var element in elements) {
                element.deleteCondition = () => !exportOpen;
            }
            uiElements.AddRange(elements);
        }

        public static void exportImage() {
            exportOpen = false;
            
            Texture2D texture = canvas.genSingleImage();
            
            Textures.exportTexture(texture, Paths.texturePath, "test" + Util.randInt(100000));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Colors.background);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp);

            
            canvas.render(camera, spriteBatch);
            
            Texture2D rect = Textures.get("rect");

            foreach (var uiElement in uiElements) {
                uiElement.render(spriteBatch);
            }

            spriteBatch.Draw(rect, new Rectangle(7, 37, 36, 56), Color.Black);
            spriteBatch.Draw(rect, new Rectangle(8, 38, 34, 54), Color.LightGray);
            spriteBatch.Draw(rect, new Rectangle(10, 40, 30, 50), brushColor);
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

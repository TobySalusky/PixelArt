using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX;
using SharpDX.Direct2D1;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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

        public static UIElement selectedUI;
        
        public static List<UIElement> uiElements = new List<UIElement>();
        public static List<UIElement> renderAgain = new List<UIElement>();
        
        public static ColorWheel colorWheel;
        public static HueSlider hueSlider;

        public static bool exportOpen;

        public static bool updateLayerButtons = true;

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
        public static FrameCounter fpsCounter = new FrameCounter();
        public static int secondsPassed;
        public static float timePassed;
        
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
            
            Window.Position = (true) ? new Point(0, 0) : Window.Position = new Point(0, 1080);

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
            canvas = new Canvas(32);
            //canvas.makeTiled(16);

            colorWheel = new ColorWheel(new Vector2(120, 100), new Vector2(130, 150));
            hueSlider = new HueSlider(new Vector2(205, 100), new Vector2(20, 150));

            uiElements.Add(new PanelSide(new Rectangle(0, 0, 225, screenHeight)));
            
            uiElements.Add(new PanelSide(new Rectangle(screenWidth - 175, 0, 175, screenHeight)));
            uiElements.Add(colorWheel);
            uiElements.Add(hueSlider);
            
            uiElements.Add(new UIText(new Vector2(235, 10), () => {
                Point point = canvas.toPixel(canvas.toCanvas(lastMousePos()));
                return "(" + point.X + ", " + point.Y + ")";
            }));
            
            const int rows = 4;
            foreach (Tool toolType in Util.GetValues<Tool>()) {
                int i = (int) toolType;
                uiElements.Add(new ToolButton(toolType, new Vector2(24 + (i / rows * 40), 230 + (i % rows) * 40)));
            }
            
            setTool(Tool.Brush);
        }

        private float delta(GameTime gameTime) {
            return (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void globalControls(float deltaTime, KeyInfo keys, MouseInfo mouse) {

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
                    Exporting.exportPopUp();
            }


            // TOOLS
            toolControls(deltaTime, keys, mouse);
            
            // Copying
            if (keys.pressed(Keys.C) && keys.control) {
                canvas.copyLayerToClipboard();
            }

            // Pasting
            if (keys.pressed(Keys.V) && keys.control) {
                Layer pasteLayer = Clipboard.getLayer();
                if (pasteLayer != null) {
                    canvas.pasteLayer(pasteLayer);
                }
                else {
                    Texture2D pasteTexture = Clipboard.getTexture();
                    if (pasteTexture != null)
                        canvas.pasteTexture(pasteTexture);
                }
            }
        }

        public static void setTool(Tool newTool) {
            tool = newTool;
            var list = ToolUtil.genToolSettings(tool);

            /*foreach (var element in list) { // TODO: update to remove flicker
                element.update();
            }*/
            
            uiElements.AddRange(list);

            canvas.switchOff(lastTool);
        }

        public void toolControls(float deltaTime, KeyInfo keys, MouseInfo mouse) {
            // TOOLS
            if (keys.pressed(Keys.A))
                tool = Tool.Brush;
            if (keys.pressed(Keys.S) && !keys.control)
                tool = Tool.Eraser;
            if (keys.pressed(Keys.W))
                tool = Tool.ColorPick;
            if (keys.pressed(Keys.D) && !keys.shift)
                tool = Tool.FillBucket;
            if (keys.pressed(Keys.R))
                tool = Tool.Rect;
            if (keys.pressed(Keys.C) && !keys.control)
                tool = Tool.Ellipse;
            if (keys.pressed(Keys.Q) || keys.pressed(Keys.E)) 
                tool = Tool.RectSelect;

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
                    Warnings.log("warning: used num-keys to try to change to out-of-range brush");
                }
            }

            // SETTINGS
            if (keys.pressed(Keys.L))
                canvas.grid ^= true;
            if (keys.pressed(Keys.T))
                if (canvas.tileGridTexture != null)
                    canvas.tileGrid ^= true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (renderAgain.Count > 0) {
                renderAgain.Clear();
            }

            float deltaTime = delta(gameTime);

            timePassed += deltaTime;

            fpsCounter.update(deltaTime);
            if ((int) gameTime.TotalGameTime.TotalSeconds > secondsPassed) {
                secondsPassed = (int) gameTime.TotalGameTime.TotalSeconds;
                Window.Title = "FPS: " + (int) fpsCounter.AverageFramesPerSecond;
            }

            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            KeyInfo keys = new KeyInfo(keyState, lastKeyState);
            MouseInfo mouse = new MouseInfo(mouseState, lastMouseState);

            if (selectedUI != null && selectedUI.delete) {
                selectedUI = null;
            }

            bool keyInputOverride = selectedUI != null && Util.isClassOrSub(selectedUI, typeof(UITextInput));
            
            if (keys.down(Keys.Escape)) {
                Exit();
            }
            
            if (!keyInputOverride) 
                globalControls(deltaTime, keys, mouse);

            for (int i = uiElements.Count - 1; i >= 0; i--) {
                UIElement element = uiElements[i];

                if (element.delete) {
                    uiElements.RemoveAt(i);
                    continue;
                }
                
                element.update(mouse, keys, deltaTime);
            }

            if (!uiHit && !keyInputOverride) {
                canvas.input(deltaTime, keys, mouse);
            }
            
            LayerButton.handleLayerButtons(mouse, keys, deltaTime);

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
            
            foreach (var uiElement in renderAgain) {
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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        
        public static KeyInfo latestKeys;
        public static MouseInfo latestMouse;

        // UI
        public static bool uiHit;

        public static UIElement selectedUI;
        
        public static List<UIElement> uiElements = new List<UIElement>();
        public static List<UIElement> renderAgain = new List<UIElement>();
        
        public static ColorWheel colorWheel;
        public static HueSlider hueSlider;

        public static bool popupOpen;

        public static bool updateLayerButtons = true;
        
        // UI SCREENS
        public static UIScreen uiScreen;

        public static FileOpenScreen fileOpenScreen;
        
        // SCREEN
        public static Vector2 screenDimen;
        public static Vector2 screenCenter;
        public static int screenHeight, screenWidth;
        
        // OBJECTS
        public static Camera camera;
        
        public static Canvas canvas;
        public static Project project;

        public static List<Project> projects = new List<Project>();

        // HTML UI
        public static HtmlNode htmlNode;
        public static float htmlTestVal;
        
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
        public static float currentDeltaTime;
        
        // OTHER
        public static List<Action> onNextUpdateStart = new List<Action>();
        
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // REMOVES LIMIT FROM FPS
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

            instance = this;

            var form = (Form)Form.FromHandle(Window.Handle);
            form.WindowState = FormWindowState.Maximized;
            
            Logger.log("size", form.Size);

            screenDimen = new Vector2(1920, 1080);
            Logger.log(screenDimen);
            screenCenter = screenDimen / 2;
            screenWidth = (int) screenDimen.X;
            screenHeight = (int) screenDimen.Y;
        }

        protected override void Initialize()
        {

            //Logger.log(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            Logger.log(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
            
            var form = (Form)Form.FromHandle(Window.Handle);
            System.Drawing.Rectangle screenRectangle = form.RectangleToScreen(form.ClientRectangle);
            int titleHeight = screenRectangle.Top - form.Top;
            const int taskBarHeight = 32;
            // TODO: https://stackoverflow.com/questions/1264406/how-do-i-get-the-taskbars-position-and-size
            // actually make task bar work properly
            screenDimen = new Vector2(GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height - taskBarHeight - titleHeight);
            Logger.log(screenDimen);
            screenCenter = screenDimen / 2;
            screenWidth = (int) screenDimen.X;
            screenHeight = (int) screenDimen.Y;
            
            
            // Graphics stuff and setup
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
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
            
            Project initProj = new Project(new Canvas(64));
            //canvas.makeTiled(16);

            projects.Add(initProj);
            setProject(initProj);

            resetCameraPosition();

            colorWheel = new ColorWheel(new Vector2(120, 100), new Vector2(130, 150));
            hueSlider = new HueSlider(new Vector2(205, 100), new Vector2(20, 150));

            uiElements.Add(new PanelSide(new Rectangle(0, 0, 225, screenHeight), 1));
            
            uiElements.Add(new PanelSide(new Rectangle(screenWidth - 175, 0, 175 + 1, screenHeight), -1));
            uiElements.Add(colorWheel);
            uiElements.Add(hueSlider);
            
            uiElements.Add(new UIText(new Vector2(235, 10), () => {
                Point point = canvas.toPixel(canvas.toCanvas(lastMousePos()));
                return canvas.xPix + "x" + canvas.yPix +"   (" + point.X + ", " + point.Y + ")";
            }));
            
            // Layer Util Buttons
            List<UIElement> layerUtilButtons = new List<UIElement> {
                new UIButton(() => canvas.addLayerAbove(), Vector2.Zero, Vector2.One * 32) {
                    topTexture = Textures.get("NewLayerButton"), colorFunc = () => Color.Gray
                },
                new UIButton(() => canvas.duplicateLayer(), Vector2.Zero, Vector2.One * 32) {
                    topTexture = Textures.get("DuplicateLayerButton"), colorFunc = () => Color.Gray
                },
                new UIButton(() => canvas.mergeLayerDown(), Vector2.Zero, Vector2.One * 32) {
                    topTexture = Textures.get("MergeLayerButton"), colorFunc = () => Color.Gray
                },
                new UIButton(() => canvas.deleteLayer(), Vector2.Zero, Vector2.One * 32) {
                    topTexture = Textures.get("DeleteLayerButton"), colorFunc = () => Color.Gray
                }
            };
            
            new FlexBox(layerUtilButtons, new Rectangle(screenWidth - 175, screenHeight - 100, 175, 0)).apply();
            uiElements.AddRange(layerUtilButtons);

            const int rows = 4;
            foreach (Tool toolType in Util.GetValues<Tool>()) {
                int i = (int) toolType;
                uiElements.Add(new ToolButton(toolType, new Vector2(24 + (i / rows * 40), 230 + (i % rows) * 40)));
            }
            
            setTool(Tool.Brush);


            startHTML();
        }

        public static async void startHTML() { 
            // HTML TESTING

            const string Apple = @"
const Counter = () => {
    
    int [count, setCount] = useState(0);
    string [color, setColor] = useState('black');
    string [textColor, setTextColor] = useState('white');
    float [speed, setSpeed] = useState(random(10) + 1);
    
    return (
        <div height='100%' width={300} flexDirection='column'>
            <div -flex={float: cos(@t * speed) * 0.5F + 0.5F}></div>
            <div -backgroundColor={string: color} onMouseEnter={()=^{
                setColor('white');
                setTextColor('black');
            }} onMouseExit={()=^{
                setColor('black');
                setTextColor('white');
            }} borderColor='#0F0F8B' borderWidth={3} borderRadius='50%' dimens={300} onPress={()=^ setCount(count+1)} align='center'>
                <h3 -color={string: textColor}>Count: {count}</h3>
            </div>
            <div -flex={float: 1 - (cos(@t * speed) * 0.5F + 0.5F)}></div>
        </div>
    );
}
";
            
            const string html = @"
<div flexDirection='row' dimens='100%' backgroundColor='black'>
    <div -flex={float: cos(@t) * 0.5F + 0.5F}></div>
    <Counter/>
    <div -flex={float: 1 - (cos(@t) * 0.5F + 0.5F)}></div>
</div>
";
            var statePack = new StatePack(
                
            );
            
            var macros = Macros.create(
                "div(html)", "<div>$$html</div>"
            );

            var components = PixelArt.Components.create(
                Apple
            );

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            
            htmlNode = await HtmlProcessor.genHTML(html, statePack, macros, components);

            watch.Stop();
            Logger.log("compiling HTML took:", watch.Elapsed.TotalSeconds);
        }

        public static void setProject(Project project) {
            
            // TODO: switch off/autosave prev project if not null
            
            Main.project = project;
            canvas = project.canvas;
            updateLayerButtons = true;
        }

        public static void addProject(Project project) {
            projects.Add(project);
            
        }

        public static void addActiveProject(Project project) {
            addProject(project);
            setProject(project);
        }

        private float delta(GameTime gameTime) {
            return (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void resetCameraPosition() { 
            camera.pos = Vector2.Zero;
            Vector2 dimen = canvas.dimen;
            Rectangle target = Util.useRatio(dimen, new Rectangle(0, 0, screenWidth - 400 - 50, screenHeight - 150));

            camera.scale = target.Width / dimen.X;
            
            // offsets for x-panels
            camera.pos.X -= 50 / 2F / camera.scale;
            
            
            // TODO: remove once variable screen size is in
            camera.pos.Y += 30 / camera.scale;
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
                Vector2 worldFrom = camera.toWorld(mouse.pos);
                camera.scale *= 1 + mouse.scroll / 5F;
                Vector2 worldTo = camera.toWorld(mouse.pos);

                Vector2 diff = worldTo - worldFrom;

                camera.pos -= diff;
            }

            if (keys.pressed(Keys.R) && (keys.shift || keys.control)) {
                resetCameraPosition();
            }

            // Saving
            if (keys.pressed(Keys.S) && keys.control) {
                if (!popupOpen)
                    Exporting.exportPopUp();
            }
            
            // Canvas Creation
            if (keys.pressed(Keys.N) && keys.control) {
                if (!popupOpen)
                    ProjectCreation.createPopup();
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

            if (keys.pressed(Keys.M)) {
                fileOpenScreen ??= new FileOpenScreen();
                uiScreen = fileOpenScreen;
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
            if (keys.pressed(Keys.D) && !keys.shift && !keys.control)
                tool = Tool.FillBucket;
            if (keys.pressed(Keys.R) && (!keys.shift && !keys.control))
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
            if (keys.pressed(Keys.T) && !keys.control)
                if (canvas.tileGridTexture != null)
                    canvas.tileGrid ^= true;
            
            // DEBUG
            if (keys.pressed(Keys.K)) {
                if (keys.shift) {
                    addActiveProject(new Project(XnaSerializer.Deserialize<ProjectSave>(Paths.exportPath + "project.pxl")));
                }
                else { 
                    XnaSerializer.Serialize(Paths.exportPath + "project.pxl", new ProjectSave(project));
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // starting stuff
            if (renderAgain.Count > 0) {
                renderAgain.Clear();
            }

            float deltaTime = delta(gameTime);

            currentDeltaTime = deltaTime;
            timePassed += deltaTime;

            if (onNextUpdateStart.Count > 0) {
                foreach (var action in onNextUpdateStart) {
                    action.Invoke();
                }
                onNextUpdateStart.Clear();                
            }

            fpsCounter.update(deltaTime);
            if ((int) gameTime.TotalGameTime.TotalSeconds > secondsPassed) {
                secondsPassed = (int) gameTime.TotalGameTime.TotalSeconds;
                Window.Title = "FPS: " + (int) fpsCounter.AverageFramesPerSecond;
            }

            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            KeyInfo keys = new KeyInfo(keyState, lastKeyState);
            MouseInfo mouse = new MouseInfo(mouseState, lastMouseState);

            latestKeys = keys;
            latestMouse = mouse;

            if (selectedUI != null && selectedUI.delete) {
                selectedUI = null;
            }

            bool keyInputOverride = selectedUI != null && Util.isClassOrSub(selectedUI, typeof(UITextInput));
            
            if (keys.down(Keys.Escape)) {
                Exit();
            }
            
            // main logic
            if (uiScreen != null) { 
                uiScreen.update(mouse, keys, deltaTime);
            }
            else {
                screenUpdate(deltaTime, keys, mouse, keyInputOverride);
            }

            // end stuff
            lastKeyState = keyState;
            lastMouseState = mouseState;
            
            if (mouse.leftUnpressed) uiHit = false;
            
            base.Update(gameTime);
        }

        public void screenUpdate(float deltaTime, KeyInfo keys, MouseInfo mouse, bool keyInputOverride) { 
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

            htmlTestVal += deltaTime;
            htmlNode?.update(deltaTime, mouse);
            
            if (mouse.leftPressed) {
                htmlNode?.clickRecurse(mouse.pos);
            }
        }

        protected override void Draw(GameTime gameTime)
        {

            if (uiScreen != null) { 
                uiScreen.render(this, spriteBatch);
                return;
            }


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

            htmlNode?.render(spriteBatch);

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
